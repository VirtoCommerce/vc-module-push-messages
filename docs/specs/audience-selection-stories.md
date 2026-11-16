# Epic: Push message audience selection

| | |
|---|---|
| **Owning module** | `VirtoCommerce.PushMessages` |
| **Admin UI** | VC-Shell 2 app at `src/VirtoCommerce.PushMessages.Web/App` |
| **Reads from (no changes owned here)** | `VirtoCommerce.Customer` — member index schema and `IMemberSearchService`<br>`VirtoCommerce.Search` — `ISearchPhraseParser` |
| **Origin** | How-to request from Innovadis on behalf of their Installatiebalie customer, September 2026 |
| **User guide** | [Manage push messages](https://docs.virtocommerce.org/platform/user-guide/latest/push-messages/manage-push-messages/) · [Search query syntax](https://docs.virtocommerce.org/platform/user-guide/latest/search-query-syntax/) |

## Why

A marketing manager selecting recipients for a push message today gets two fields: a member picker and a
free-text **Recipients query**. The query field is the raw keyword of the Customer member search index,
passed through untouched — no field list, no validation, and no way to discover what can be typed into it.

The customer reported this as missing documentation. Investigation found three defects underneath it:

1. A query the platform cannot parse resolves to *no filters and no keyword*, which the member index
   answers with every member. One mistyped character broadcasts to the entire customer base, with no
   error and no confirmation step. Verified by running `SearchPhraseParser` over `*:*`.
2. Wrong field names fail just as silently in the other direction — `email:` is not a field, the index
   calls it `emails`, and filtering an unknown field matches nothing.
3. The **Count** button counts matched *members*. The send resolves those to *user accounts* and expands
   organisations into their descendant members. The only number an operator sees before an irreversible
   action is the wrong one.

Better documentation alone would make defect 1 easier to trigger, not safer.

## Goal

A marketing manager can define the audience for a push message from named, discoverable options, see how
many people will actually receive it, review that list, and confirm before sending — without knowing the
search query syntax. Operators who do know it keep the query field, with validation.

## Out of scope

- Changes to the member index schema in `VirtoCommerce.Customer`.
- Indexing role permissions onto members (see PM-11 for the workaround this epic ships instead).
- FCM delivery, message content, scheduling, and read tracking.
- The Frontend (customer-facing) push message experience.

## Sequencing

| Phase | Stories | Intent |
|---|---|---|
| 1 | PM-1 | Answer the customer's question. Docs only, no code, unblocked today. |
| 2 | PM-2, PM-3, PM-4, PM-5 | Close the safety defects. Nobody sends blind or by accident. |
| 3 | PM-6, PM-7, PM-8, PM-9, PM-11 | The audience builder itself. |
| 4 | PM-10 | Make audiences reusable. |

PM-1 must land before or with PM-2: PM-2 removes `*:*` as a way to reach everyone, and PM-1 is what
documents `membertype:Contact` as the replacement until PM-6 ships the **Everyone** option.

---

## PM-1 · Document the recipients query

**Type** Documentation · **Size** S · **Repo** `vc-docs` (the only story outside the module)

> **As a** marketing manager
> **I want** the push message documentation to list the fields I can query and show worked examples
> **So that** I can target an audience without guessing what the field accepts.

**Acceptance criteria**

```
GIVEN I am reading the Manage push messages user-guide page
WHEN  I reach the step describing recipients
THEN  I see a table of every queryable member field, with its display name and its index field name
AND   I see worked examples for: all customers, an exact name, one email address, an email domain,
      a role, a permission, a dynamic property value, and one or more companies
AND   I see the syntax rules that apply — comma for OR within a field, space for AND, "!" for
      negation, quotes around any value containing a space or a wildcard, and [a TO b] for date ranges
AND   I see a note that the member index must be built before any query returns results
AND   the page states that Count reports matched members, not recipients
```

**Notes**

- Field names come from `MemberDocumentBuilder.BuildSchemaAsync`. Every dynamic property is queryable
  under its own name, lowercased, once members are reindexed.
- Permissions are never indexed onto members. The documented approach is to look up which roles carry
  the permission, then filter on those role IDs.
- Wildcard and case behaviour was confirmed against Elasticsearch 8. Other search providers implement
  wildcard term filters differently — confirm before publishing the wildcard examples.

---

## PM-2 · A query that resolves to nothing must not send to everyone

**Type** Bug · **Size** S · **Priority** Highest

> **As a** marketing manager
> **I want** a recipients query the platform cannot resolve to be rejected
> **So that** a typo cannot send a message to my entire customer base.

**Acceptance criteria**

```
GIVEN a push message whose recipients query parses to no filters and no keyword
WHEN  it is saved with status Sent, or the scheduled send job picks it up
THEN  the operation is rejected with a validation error identifying the query as unresolvable
AND   no PushMessageRecipient records are created
AND   the message keeps its previous status

GIVEN the same query
WHEN  I edit the message in the back office
THEN  the query field shows a blocking error and Send is disabled
AND   the error names the supported way to reach every customer
```

**Notes**

- The failure path is `PushMessage.MemberQuery` → `MembersSearchCriteria.Keyword` →
  `MemberSearchRequestBuilder.GetFilters`, which replaces the keyword with the parse result. An empty
  result reaches the index as an unconstrained search.
- Rejection must happen on the send path, not only in the UI — the scheduled send job and the API both
  reach it without the Admin UI.

---

## PM-3 · Show the number of people who will actually receive the message

**Type** Bug · **Size** M · **Priority** Highest

> **As a** marketing manager
> **I want** the count beside my audience to be the number of people who will receive the message
> **So that** I can trust it before I send.

**Acceptance criteria**

```
GIVEN an audience defined by a query, a member list, or both
WHEN  I request an estimate
THEN  the number shown is the count of distinct user accounts that will receive the message
AND   organisations in the audience are expanded to their descendant members, recursively
AND   members with no user account are excluded
AND   a member with more than one user account is counted once per account
AND   a breakdown is shown: members matched, companies expanded, people in scope,
      people without a login, additional logins

GIVEN an estimate is displayed
WHEN  I change any part of the audience
THEN  the displayed number is cleared immediately and re-estimated
AND   a number from a previous audience is never shown beside the current one
```

**Notes**

- New endpoint `POST /api/push-messages/audience/estimate`, reusing the resolution in
  `PushMessageJobService.GetNewRecipients` without persisting recipients.
- Today `countMembers` in `useMessageDetails` calls the Customer module directly with `take: 0` and
  reports `totalCount`, and nothing clears it when the query changes.
- Estimate calls must be debounced and must respect the module's configured batch size.

---

## PM-4 · Warn me when my query does not mean what I typed

**Type** Story · **Size** M

> **As a** marketing manager writing a query by hand
> **I want** to be told when my query will not do what I intended
> **So that** I find out before I send rather than afterwards.

**Acceptance criteria**

```
GIVEN a query naming a field that does not exist on the member index
WHEN  the query is validated
THEN  I see a blocking error naming the field, and a suggested field where a close match exists

GIVEN a query with an unquoted wildcard, such as emails:*@example.com
WHEN  the query is validated
THEN  I see a blocking error explaining the wildcard is discarded, and the quoted form to use instead

GIVEN a query with an unquoted multi-word value, such as name:John Doe
WHEN  the query is validated
THEN  I see a warning that the trailing words are treated as free text, and the quoted form to use

GIVEN a query that resolves to no filters and no keyword
WHEN  the query is validated
THEN  I see the blocking error from PM-2

GIVEN a query with no problems
WHEN  the query is validated
THEN  I see it confirmed valid, with the resolved recipient count from PM-3
```

**Notes**

- Blocking errors disable Send. Warnings do not.
- Whether validation rides on the PM-3 estimate response or a separate lightweight endpoint is an open
  question — the estimate is the more expensive call, so per-keystroke feedback likely wants its own.

---

## PM-5 · Confirm before an irreversible send

**Type** Story · **Size** S

> **As a** marketing manager
> **I want** to confirm the audience and the recipient count before the message goes out
> **So that** I do not discover a mistake after it has reached customers.

**Acceptance criteria**

```
GIVEN an audience with at least one recipient
WHEN  I click Send
THEN  I am shown a confirmation naming the audience in plain language and the recipient count
AND   the message is not sent until I confirm
AND   cancelling leaves the message in its previous status

GIVEN an audience whose recipient count exceeds the configured large-send threshold
WHEN  the confirmation is shown
THEN  I must type a confirmation word before Send is enabled

GIVEN an audience that resolves to zero recipients
WHEN  I view the message
THEN  Send is disabled and the reason is stated
```

**Notes**

- Add a module setting for the threshold, alongside the existing `PushMessages.BatchSize`.
- "Plain language" means the audience read back as a sentence, not the query string — the query stays
  available behind **Show generated query**.

---

## PM-6 · Choose an audience by mode, including Everyone

**Type** Story · **Size** M

> **As a** marketing manager
> **I want** to pick how I am defining the audience from named options
> **So that** I do not have to know a query syntax to send to an obvious group.

**Acceptance criteria**

```
GIVEN I am editing a push message
WHEN  I open the audience section
THEN  I can choose exactly one of: Everyone, Specific people or companies, Match by conditions,
      Advanced query

GIVEN I choose Everyone
THEN  I see how many customers that reaches, and how many recipients that resolves to
AND   the audience is stored so that it is recognised as Everyone when the message is reopened

GIVEN I have conditions defined and I switch to Advanced query
THEN  the query generated from those conditions is placed in the query field

GIVEN I am in Advanced query mode with a query the condition builder can represent
WHEN  I switch back to Match by conditions
THEN  the query is loaded as conditions
AND   a query the builder cannot represent leaves me in Advanced query mode with an explanation
```

**Notes**

- Everyone should persist without a schema migration. Storing `MemberQuery = "membertype:Contact"` and
  recognising that exact value in the UI is the cheap option; a dedicated field on `PushMessage` is the
  explicit one. Architect's call — see open questions.
- Existing saved messages must keep working: any message with a `MemberQuery` that the builder cannot
  represent opens in Advanced query mode.

---

## PM-7 · Build an audience from conditions

**Type** Story · **Size** L

> **As a** marketing manager
> **I want** to build the audience from a list of conditions with the available fields offered to me
> **So that** I can target customers by company, role, tag or property without writing a query.

**Acceptance criteria**

```
GIVEN I am in Match by conditions mode
WHEN  I add a condition
THEN  I choose the field from a grouped list labelled in business terms, not index field names
AND   the list includes every filterable field on the member index and every dynamic property

GIVEN I have chosen a field
THEN  the operators offered are only those the search query syntax can express for that field's type
AND   the value input matches the field — a company picker for company, the role list for role,
      a date picker for dates, a yes/no choice for booleans, free text otherwise

GIVEN I choose a company as a value
THEN  the organisation's ID is stored, and its name is what I see

GIVEN two or more conditions
THEN  I can choose whether all of them or any of them must match
AND   I can remove any condition

GIVEN any set of conditions
WHEN  I ask to see the generated query
THEN  I see exactly what will be stored in PushMessage.MemberQuery
```

**Notes**

- New endpoint `GET /api/push-messages/audience/fields`, returning the member index schema plus dynamic
  properties with display labels, so the picker cannot drift from the index.
- Offering no operator the parser cannot express is what prevents the builder generating a query that
  silently means something else. Supported mappings: `is` → `field:value`, `is not` → `!field:value`,
  `is any of` → `field:a,b`, `starts with` / `ends with` / `contains` → quoted wildcards,
  `on or after` / `on or before` → `field:[a TO]` / `field:[TO b]`.
- Values containing a space or a wildcard must be quoted when generated.

---

## PM-8 · Preview the recipient list

**Type** Story · **Size** S

> **As a** marketing manager
> **I want** to see who is in my audience before I send
> **So that** I can sanity-check the rule rather than trusting a number.

**Acceptance criteria**

```
GIVEN an audience that resolves to at least one recipient
WHEN  I open the recipient preview
THEN  I see a page of resolved recipients with name, email, company and role
AND   I see the total count from PM-3
AND   the list reflects the audience as currently edited, including unsaved changes

GIVEN an audience that resolves to zero recipients
WHEN  I open the preview
THEN  I am told nobody matches, rather than shown an empty table
```

**Notes** New endpoint `POST /api/push-messages/audience/preview`, paged.

---

## PM-9 · Combine a recipient list with conditions

**Type** Story · **Size** S

> **As a** marketing manager
> **I want** to add named people or companies on top of a condition-based audience
> **So that** I can send to "everyone at Acme, plus these three dealers" in one message.

**Acceptance criteria**

```
GIVEN I am in Match by conditions mode
WHEN  I add specific people or companies alongside my conditions
THEN  both sets are included in the audience
AND   the estimate from PM-3 counts the union, without double-counting anyone in both

GIVEN a company is added by name
THEN  everyone in that company and in the companies beneath it is included
AND   the number of recipients that company contributes is shown beside it
```

**Notes**

- Backend already unions `MemberIds` and `MemberQuery` in `PushMessageJobService.GetNewRecipients`. The
  mutual exclusion is a UI constraint only: `showMemberIds` and `showMemberQuery` are each computed from
  the other. This story is Admin UI work.

---

## PM-10 · Reuse an audience

**Type** Story · **Size** M · **Priority** Lowest

> **As a** marketing manager sending similar messages regularly
> **I want** to save an audience and start from common ones
> **So that** I do not rebuild the same rule every time.

**Acceptance criteria**

```
GIVEN an audience I have defined
WHEN  I save it under a name
THEN  it is available when composing any later message

GIVEN I am starting a new audience
THEN  I am offered starting points for common cases — all customers, one company's users,
      a role, an email domain, registered since a date, a tag
AND   choosing one fills in the conditions, which I can then adjust

GIVEN a saved audience is applied to a message
THEN  the message stores the resolved conditions, so later edits to the saved audience
      do not change a message that has already been sent
```

**Notes**

- Needs a new entity and migration, and its own read/update permissions. Confirm this is wanted before
  building — contact tags (`groups`) already cover recurring segments with no new concepts, and pushing
  operators towards tagging may be the better answer.

---

## PM-11 · Target customers by permission

**Type** Story · **Size** S

> **As a** marketing manager
> **I want** to target customers who hold a particular permission
> **So that** I can reach, for example, everyone who can approve orders.

**Acceptance criteria**

```
GIVEN I am choosing a value for a Role condition
WHEN  I filter the role list by a permission
THEN  I see only the roles that carry that permission
AND   selecting them produces a condition matching any of those roles

GIVEN roles are later changed so that a different set carries that permission
THEN  a message already sent is unaffected
AND   a message tracking new recipients continues against the roles it was saved with
```

**Notes**

- Permissions are held on roles and never indexed onto members, so no query can filter on a permission
  directly. Resolving permission → roles → `roleid` at authoring time is the cheap answer and keeps the
  member index unchanged. Indexing permissions onto members was considered and rejected as out of scope.

---

## Non-functional requirements

- Estimate and preview calls must not create or modify `PushMessageRecipient` records.
- Estimate calls are debounced in the Admin UI and honour `PushMessages.BatchSize` when paging members.
- All new endpoints are guarded by the existing `PushMessages:read` and `PushMessages:update` permissions.
- Audience resolution must behave identically whether it is reached from the Admin UI, the API, or the
  scheduled send job.
- New Admin UI strings are added to all locale files shipped by the module.

## Definition of done

- Unit tests cover audience resolution: organisation expansion, members without accounts, members with
  several accounts, the union of list and conditions, and the unresolvable-query rejection.
- Existing push messages with a hand-written `MemberQuery` open and send unchanged.
- The user-guide page from PM-1 is updated to describe the audience section as shipped.
- A note in the module release notes calls out the changed `Count` behaviour, since the number operators
  are used to seeing will change.

## Open questions

1. **Everyone persistence** — recognised query string, or a dedicated field on `PushMessage` with a
   migration? Affects PM-6.
2. **Validation transport** — warnings on the estimate response, or a separate lightweight validate
   endpoint for per-keystroke feedback? Affects PM-4.
3. **Large-send threshold** — what default, and should it be per-store?
4. **Saved audiences** — wanted, or is pushing operators towards contact tags the better answer? Affects
   PM-10.
5. **Search providers** — wildcard term filter behaviour was confirmed on Elasticsearch 8 only. Which
   providers must PM-7's `starts with` / `ends with` / `contains` operators support?
