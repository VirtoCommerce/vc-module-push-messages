import { computed, reactive, ref, Ref, watch } from "vue";
import {
  DetailsBaseBladeScope,
  DynamicBladeForm,
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
    remove: IBladeToolbar;
  };
}

export default (args: {
  props: InstanceType<typeof DynamicBladeForm>["$props"];
  emit: InstanceType<typeof DynamicBladeForm>["$emit"];
  mounted: Ref<boolean>;
}) => {
  const messageId = args.props.param;

  const detailsFactory = useDetailsFactory<PushMessage>({
    load: async (message) => {
      if (message?.id) {
        return (await getPushMessageApiClient()).get(message.id, "WithMembers");
      }
    },
    saveChanges: async (message) => {
      const apiClient = await getPushMessageApiClient();
      return !messageId ? apiClient.create(message) : apiClient.update(message);
    },
    remove: async ({ id }) => {
      if (id) {
        return (await getPushMessageApiClient()).delete([id]);
      }
    },
  });

  const { load, saveChanges, remove, loading, item, validationState } = detailsFactory();

  const scope = ref<PushMessageDetailsScope>({
    toolbarOverrides: {
      saveChanges: {
        isVisible: computed(() => isEditable()),
        disabled: computed(() => validationState.value.disabled),
      },
      remove: {
        isVisible: computed(() => messageId != null && isEditable()),
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
  });

  function isEditable(): boolean {
    const message = item.value;
    return !messageId || (message != null && message.status !== "Sent");
  }

  const bladeTitle = computed(() => {
    return "Push message details";
  });

  watch(
    () => args?.mounted.value,
    async () => {
      if (!messageId) {
        item.value = reactive(new PushMessage());
        validationState.value.resetModified(item.value, true);
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
    scope: computed(() => scope.value),
  };
};
