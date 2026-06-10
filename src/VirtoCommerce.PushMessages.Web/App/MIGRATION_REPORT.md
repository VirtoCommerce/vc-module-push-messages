# Migration Report: 1.1.61 → 2.0.6

Generated: 2026-06-01

## Automated Changes (57 files)

- ✅ **define-app-module** — 1 file(s)
- ✅ **use-blade-migration** — 4 file(s)
- ✅ **icon-replace** — 8 file(s)
- ✅ **switch-tooltip-prop** — 1 file(s)
- ✅ **remove-global-components** — 5 file(s)
- ✅ **vc-blade-loading-prop** — 1 file(s)
- ✅ **remove-app-module-options** — 1 file(s)
- ✅ **define-options-to-blade** — 7 file(s)
- ✅ **remove-pathmatch-route** — 1 file(s)
- ✅ **blade-props-simplification** — 8 file(s)
- ✅ **define-expose-to-children** — 7 file(s)
- ✅ **blade-events-cleanup** — 6 file(s)
- ✅ **remove-expose-title** — 7 file(s)

## Completed by AI (/vc-app migrate)

All manual-migration topics below were completed by the AI migration agent. `vue-tsc --noEmit` and `yarn build` both pass (exit 0).

- ✅ **nswag-class-to-interface** — `useMessageDetails/index.ts` (clone-then-mutate → object literal), `recipients-widget.vue` (later deleted by widgets migration)
- ✅ **remove-deprecated-aliases (Injection Key)** — `messageDetails.vue`: `inject(BladeInstance)` → `useBlade()`
- ✅ **use-blade-form** — `messageDetails.vue` + `useMessageDetails/index.ts`: `useForm`/`useModificationTracker`/`onBeforeClose` → `useBladeForm`
- ✅ **widgets-migration** — `messageDetails.vue`: imperative `registerWidget` → declarative `useRecipientsWidgets` (`widgets/useRecipientsWidgets.ts`); deleted `components/widgets/`
- ✅ **vctable-audit** — `BaseListBlade.vue` + `recipientList.vue`: `VcTable` → `VcDataTable`, `useTableSort` → `useDataTableSort`
- ✅ **use-data-table-pagination-audit** — `useBaseList.ts` + `useRecipientList/index.ts` → `useDataTablePagination`; cascaded to 5 list pages
- ✅ **blade-props-simplification** — `BaseListBlade.vue`: dropped `expanded`/`closable`/`param`/`options`, uses `useBlade()`
- ✅ **icon-audit** — `messageDetails.vue`: `material-calculate` → `lucide-calculator`
- ✅ **notification-migration** — no-op (no `useNotifications`/`notificationTemplates` usage; `index.ts` already on `defineAppModule`)

## Manual Migration Required

### remove-release-config

- Removed @vc-shell/release-config from devDependencies
- Removed "release" script from package.json
- Deleted scripts/release.ts
- Deleted empty scripts/ directory

### define-app-module

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/index.ts: Migrated 4-arg createAppModule. notificationTemplates is deprecated — migrate to new notifications config format.

### Injection Key Migration

`inject(BladeInstance)` is removed. Blade context is now provided by `useBlade()` composable — it gives you `param`, `options`, `callParent`, `closeSelf`, etc.

**Affected files:**

- `src/modules/push-messages/pages/messageDetails.vue`

```ts
// OLD:
const blade = inject(BladeInstance);

// NEW:
const { param, options, callParent, closeSelf } = useBlade();
```

> See: [migration/21-injection-keys.md](migration/21-injection-keys.md)

### icon-replace

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/components/BaseListBlade.vue: Replaced 3 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/components/widgets/recipients-widget.vue: Replaced 1 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/draftList.vue: Replaced 1 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/messageDetails.vue: Replaced 4 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/messageList.vue: Replaced 1 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/scheduledList.vue: Replaced 1 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/sentList.vue: Replaced 1 icon(s) with lucide equivalents
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/trackingList.vue: Replaced 1 icon(s) with lucide equivalents

### Widget System Rewrite

The old imperative `registerWidget()`/`unregisterWidget()` API is replaced by a declarative `useBladeWidgets()` composable. Widget `.vue` files for standard sidebar items are no longer needed — use headless config objects instead. Create a `useXxxWidgets.ts` composable in your module's `widgets/` directory.

**Affected files:**

- `src/modules/push-messages/pages/messageDetails.vue`

```ts
// OLD: imperative registration in blade page
const { registerWidget, clearBladeWidgets } = useWidgets();
const blade = inject(BladeInstance);
registerWidget({ id: "MyWidget", component: MyWidgetVue, props: { item } }, blade.id);
onUnmounted(() => clearBladeWidgets(blade.id));

// NEW: declarative composable (create widgets/useMyWidgets.ts)
import { useBladeWidgets, useBlade } from "@vc-shell/framework";

export function useMyWidgets(options: { item: Ref<MyItem>; isVisible: ComputedRef<boolean> }) {
  const { openBlade } = useBlade();
  return useBladeWidgets([
    {
      id: "MyWidget",
      icon: "lucide-tag",
      title: "MY_MODULE.WIDGETS.MY_WIDGET.TITLE",
      badge: computed(() => options.item.value?.count ?? 0),
      isVisible: options.isVisible,
      onClick: () => openBlade({ name: "MyWidgetBlade" }),
      onRefresh: () => loadData(),
    },
  ]);
}

// In blade page: const { refreshAll } = useMyWidgets({ item, isVisible: computed(() => !!param.value) });
```

> See: [migration/13-widgets.md](migration/13-widgets.md)

### vctable-audit

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/components/BaseListBlade.vue: Uses <VcTable> — must be migrated to <VcDataTable>. See migration guide: VcTable → VcDataTable.
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/recipientList.vue: Uses <VcTable> — must be migrated to <VcDataTable>. See migration guide: VcTable → VcDataTable.

### NSwag DTO Class → Interface Migration

API client DTOs changed from classes (with `new DtoClass()`) to interfaces (with `{} as DtoClass`). The migrator handles simple cases automatically. Clone-then-mutate patterns (`new X(); x.field = value;`) require manual rewrite.

**Affected files:**

- `src/modules/push-messages/components/widgets/recipients-widget.vue`
- `src/modules/push-messages/composables/useMessageDetails/index.ts`

```ts
// Clone-then-mutate (manual migration):
// OLD:
const criteria = new SearchCriteria();
criteria.take = 20;
criteria.sort = "name:ASC";

// NEW:
const criteria = { take: 20, sort: "name:ASC" } as SearchCriteria;
```

> See: [migration/nswag-class-to-interface.md](migration/nswag-class-to-interface.md)

### Form Management with useBladeForm()

`useForm()` (vee-validate) + manual `onBeforeClose()` + `modified` tracking are replaced by a single `useBladeForm()` composable. Remove all three and replace with one call. `useBladeForm` handles close confirmation, modification tracking, and form validation automatically.

```ts
// OLD:
import { useForm } from "vee-validate";
const { meta } = useForm({ validateOnMount: false });
const isModified = computed(() => meta.value.dirty);
onBeforeClose(async () => {
  if (isModified.value) {
    return !(await showConfirmation(t("CLOSE_CONFIRMATION")));
  }
});

// NEW:
import { useBladeForm } from "@vc-shell/framework";
const form = useBladeForm({
  data: item, // your reactive data ref
  closeConfirmMessage: computed(() => t("CLOSE_CONFIRMATION")),
});
// form.canSave, form.isModified, form.setBaseline(), form.markReady(), form.revert()
// onBeforeClose is handled automatically — DELETE it
```

> See: [migration/37-use-blade-form.md](migration/37-use-blade-form.md)

### use-data-table-pagination-audit

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/components/BaseListBlade.vue: Uses manual onPaginationClick — delete it and bind @pagination-click="pagination.goToPage". See migration guide: useDataTablePagination.
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/composables/useBaseList.ts: Manual pagination triple (totalCount/pages/currentPage). Replace with useDataTablePagination(). See migration guide: useDataTablePagination.
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/composables/useRecipientList/index.ts: Manual pagination triple (totalCount/pages/currentPage). Replace with useDataTablePagination(). See migration guide: useDataTablePagination.
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/recipientList.vue: Uses manual onPaginationClick — delete it and bind @pagination-click="pagination.goToPage". See migration guide: useDataTablePagination.

### icon-audit

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/messageDetails.vue: [Material] material-calculate → replace with lucide- equivalent

### Reusable Blade Components

Components with blade props (expanded/closable) but no `defineBlade()` were skipped — they are reusable components, not blade pages. If these components pass blade props to child blades, remove the forwarding — child blades should call `useBlade()` directly.

**Affected files:**

- `src/modules/push-messages/components/BaseListBlade.vue`

```vue
<!-- OLD: wrapper forwarding blade props -->
<MyBlade :expanded="expanded" :closable="closable" :param="param" @close:blade="$emit('close:blade')" />

<!-- NEW: wrapper passes only domain props, child calls useBlade() -->
<MyBlade :config="config" />
```

> See: [migration/11-blade-props.md](migration/11-blade-props.md)

### blade-events-cleanup

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/components/BaseListBlade.vue: Removed blade lifecycle events
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/draftList.vue: Removed blade lifecycle events
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/messageList.vue: Removed blade lifecycle events
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/scheduledList.vue: Removed blade lifecycle events
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/sentList.vue: Removed blade lifecycle events
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/modules/push-messages/pages/trackingList.vue: Removed blade lifecycle events

## Dependencies Updated

- @vc-shell/config-generator: ^1.1.61 → ^2.0.6
- @vc-shell/framework: ^1.1.61 → ^2.0.6
- @vc-shell/api-client-generator: ^1.1.61 → ^2.0.6
- @vc-shell/ts-config: ^1.1.61 → ^2.0.6
- vue: ^3.5.13 → ^3.5.30
- vue-router: ^4.2.5 → ^5.0.3
- @commitlint/cli: ^18.4.3 → ^20.4.1
- @commitlint/config-conventional: ^18.4.3 → ^20.4.1
- @vitejs/plugin-vue: 5.0.3 → ^5.2.3
- @vue/eslint-config-prettier: ^9.0.0 → ^10.2.0
- @vue/eslint-config-typescript: ^13.0.0 → ^14.6.0
- conventional-changelog-cli: ^4.1.0 → ^5.0.0
- eslint: ^8.57.0 → ^9.35.0
- eslint-plugin-vue: ^9.19.2 → ^10.4.0
- vite-plugin-checker: ^0.9.1 → ^0.13.0
- vue-tsc: ^2.2.10 → ^3.2.5

## Not Covered by Migrator

_These migration guides may be relevant — check manually:_

- **16-login-form** — useLogin composable API changes
  Check: `grep -rn "useLogin" src/`
- **29-vc-table-to-data-table** — Old VcTable → VcDataTable migration
  Check: `grep -rn "VcTable\b" src/`

<details>
<summary>Transform Log (10 entries)</summary>

- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/tsconfig.json: added @vc-shell/framework/globals to compilerOptions.types
- /Users/symbot/DEV/vc-module-push-messages/src/VirtoCommerce.PushMessages.Web/App/src/shims-vue.d.ts: standard boilerplate — deleted
- Registry: 29 DTO classes, 29 interface→class mappings
- Found 34 consumer files to scan.
- src/modules/push-messages/components/BaseListBlade.vue: modified
- src/modules/push-messages/composables/useBaseList.ts: modified
- src/modules/push-messages/composables/useDraftList/index.ts: modified
- src/modules/push-messages/composables/useMessageDetails/index.ts: modified
- src/modules/push-messages/composables/useRecipientList/index.ts: modified
- Done. 5 file(s) modified out of 34 scanned.

</details>
