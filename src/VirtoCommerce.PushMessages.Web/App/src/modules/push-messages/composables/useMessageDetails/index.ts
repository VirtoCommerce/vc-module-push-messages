import { computed, reactive, watch } from "vue";
import {
  DetailsBaseBladeScope,
  DetailsComposableArgs,
  IBladeToolbar,
  useApiClient,
  useDetailsFactory,
} from "@vc-shell/framework";

import { CustomerModuleClient, MembersSearchCriteria } from "../../../../api_client/virtocommerce.customer";
import { PushMessage, PushMessageClient } from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient: getCustomerApiClient } = useApiClient(CustomerModuleClient);
const { getApiClient: getPushMessageApiClient } = useApiClient(PushMessageClient);

export interface PushMessageDetailsScope extends DetailsBaseBladeScope {
  toolbarOverrides: {
    saveChanges: IBladeToolbar;
    saveAndPublish: IBladeToolbar;
    clone: IBladeToolbar;
    remove: IBladeToolbar;
  };
}

export default (args: DetailsComposableArgs<{ options: { sourceMessage: PushMessage } }>) => {
  let isNew = !args.props.param;
  let newStatus: string | undefined;

  const detailsFactory = useDetailsFactory<PushMessage>({
    load: async (message) => {
      if (message?.id) {
        return (await getPushMessageApiClient()).get(message.id, "WithMembers");
      }
    },
    saveChanges: async (message) => {
      const apiClient = await getPushMessageApiClient();
      if (isNew) {
        message.status = newStatus;
        return apiClient.create(message);
      } else if (message.status !== "Sent") {
        if (newStatus) {
          message.status = newStatus;
        }
        return apiClient.update(message);
      }
      return apiClient.changeTracking(message.id!, message.trackNewRecipients!);
    },
    remove: async ({ id }) => {
      if (id) {
        return (await getPushMessageApiClient()).delete([id]);
      }
    },
  });

  const { load, saveChanges, remove, loading, item, validationState } = detailsFactory();

  const scope: PushMessageDetailsScope = {
    toolbarOverrides: {
      saveChanges: {
        disabled: computed(() => !validationState.value.modified || !validationState.value.valid),
        async clickHandler() {
          return saveMessage(item.value);
        },
      },
      saveAndPublish: {
        isVisible: computed(() => isEditable() && item.value != null && item.value.status !== "Scheduled"),
        disabled: computed(() => {
          return (
            !validationState.value.valid ||
            item.value == null ||
            (!item.value.memberQuery && (!item.value.memberIds || item.value.memberIds.length == 0))
          );
        }),
        clickHandler: async () => {
          await saveMessage(item.value, item.value?.startDate != null ? "Scheduled" : "Sent");
        },
      },
      clone: {
        isVisible: computed(() => !isNew),
        clickHandler: async () => {
          args.emit("parent:call", {
            method: "openDetailsBlade",
            args: {
              options: {
                sourceMessage: item,
              },
            },
          });
        },
      },
      remove: {
        isVisible: computed(() => !isNew && isEditable()),
      },
    },
    loadMembers: async (keyword?: string, skip?: number, ids?: string[]) => {
      return (await getCustomerApiClient()).searchMember({
        keyword: keyword,
        objectIds: ids,
        deepSearch: true,
        objectType: "Member",
        sort: "MemberType:desc;Name",
        skip: skip,
        take: ids?.length ?? 20,
      } as MembersSearchCriteria);
    },
    isReadOnly: () => !isEditable(),
    countMembers: countMembers,
  };

  async function saveMessage(message: PushMessage | undefined, status?: string) {
    if (message) {
      newStatus = status;
      const result = await saveChanges(item.value);
      newStatus = undefined;

      if (result != null) {
        isNew = false;
        validationState.value.resetModified(reactive(result), true);
      }

      args.emit("parent:call", { method: "reload" });
      args.emit("parent:call", { method: "updateActiveWidgetCount" });
    }
  }

  async function countMembers() {
    if (item.value?.memberQuery) {
      const apiClient = await getCustomerApiClient();
      const result = await apiClient.searchMember({
        keyword: item.value.memberQuery,
        deepSearch: true,
        take: 0,
      } as MembersSearchCriteria);
      scope.value.memberCount = result.totalCount;
    }
  }

  function isEditable(): boolean {
    return isNew || (item.value != null && item.value.status !== "Sent");
  }

  const bladeTitle = computed(() => {
    return isNew ? "New push message" : "Push message details";
  });

  watch(
    () => args?.mounted.value,
    async () => {
      if (isNew) {
        const message = new PushMessage();
        item.value = reactive(message);
        validationState.value.resetModified(item.value, true);

        const sourceMessage = args.props.options?.sourceMessage;
        if (sourceMessage) {
          message.topic = sourceMessage.topic;
          message.shortMessage = sourceMessage.shortMessage;
          message.memberIds = sourceMessage.memberIds;
          message.memberQuery = sourceMessage.memberQuery;
        }
      }
    },
  );

  return {
    load,
    saveChanges,
    remove,
    loading,
    item,
    validationState,
    bladeTitle,
    scope,
  };
};
