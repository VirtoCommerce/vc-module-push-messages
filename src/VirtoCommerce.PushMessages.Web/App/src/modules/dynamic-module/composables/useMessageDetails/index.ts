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
  };
}

export default (args: {
  props: InstanceType<typeof DynamicBladeForm>["$props"];
  emit: InstanceType<typeof DynamicBladeForm>["$emit"];
  mounted: Ref<boolean>;
}) => {
  const detailsFactory = useDetailsFactory<PushMessage>({
    load: async (message) => {
      if (message?.id) {
        return (await getPushMessageApiClient()).get(message.id, "WithMembers");
      }
    },
    saveChanges: async (message) => {
      return (await getPushMessageApiClient()).create(message);
    },
    remove: () => {
      throw new Error("Function not implemented.");
    },
  });

  const { load, saveChanges, remove, loading, item, validationState } = detailsFactory();

  const scope = ref<PushMessageDetailsScope>({
    toolbarOverrides: {
      saveChanges: {
        isVisible: computed(() => !args.props.param),
        disabled: computed(() => validationState.value.disabled),
      },
    },
    loadMembers: async (keyword?: string, skip?: number, ids?: string[]) => {
      return (await getCustomerApiClient()).searchContacts({
        keyword: keyword,
        objectIds: ids,
        sort: "name",
        skip: skip,
        take: ids?.length ?? 20,
      } as MembersSearchCriteria);
    },
  });

  const bladeTitle = computed(() => {
    return "Push message details";
  });

  watch(
    () => args?.mounted.value,
    async () => {
      if (!args.props.param) {
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
