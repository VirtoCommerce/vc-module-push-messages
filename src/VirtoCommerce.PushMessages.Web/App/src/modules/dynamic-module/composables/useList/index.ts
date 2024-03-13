import { computed, ref, Ref } from "vue";
import {
  DynamicBladeList,
  ListBaseBladeScope,
  useApiClient,
  useBladeNavigation,
  useListFactory,
} from "@vc-shell/framework";

import {
  IPushMessageSearchCriteria,
  PushMessage,
  PushMessageClient,
  PushMessageSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface PushMessageListScope extends ListBaseBladeScope {}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export default (args: {
  props: InstanceType<typeof DynamicBladeList>["$props"];
  emit: InstanceType<typeof DynamicBladeList>["$emit"];
  mounted: Ref<boolean>;
}) => {
  const factory = useListFactory<PushMessage[], IPushMessageSearchCriteria>({
    load: async (query) => {
      return (await getApiClient()).search({
        ...(query || {}),
        take: 20,
      } as PushMessageSearchCriteria);
    },
  });

  const { load, remove, items, pagination, loading, query } = factory();
  const { openBlade, resolveBladeByName } = useBladeNavigation();

  async function openDetailsBlade(data?: Omit<Parameters<typeof openBlade>["0"], "blade">) {
    await openBlade({
      blade: resolveBladeByName("PushMessageDetails"),
      ...data,
    });
  }

  const scope = ref<PushMessageListScope>({
    openDetailsBlade,
    deleteItem: () => {
      alert("Delete item");
    },
  });

  return {
    items,
    load,
    remove,
    loading,
    pagination,
    query,
    scope: computed(() => scope.value),
  };
};
