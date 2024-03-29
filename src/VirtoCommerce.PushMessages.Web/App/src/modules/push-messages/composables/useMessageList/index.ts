import { computed, ref } from "vue";
import { ListBaseBladeScope, useApiClient, useBladeNavigation, useListFactory } from "@vc-shell/framework";

import {
  IPushMessageSearchCriteria,
  PushMessage,
  PushMessageClient,
  PushMessageSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface PushMessageListScope extends ListBaseBladeScope {}

export default () => {
  const listFactory = useListFactory<PushMessage[], IPushMessageSearchCriteria>({
    load: async (query) => {
      const criteria = { ...(query || {}) } as PushMessageSearchCriteria;
      return (await getApiClient()).search(criteria);
    },
  });

  const { load, items, pagination, loading, query } = listFactory({ pageSize: 20 });
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
    loading,
    pagination,
    query,
    scope: computed(() => scope.value),
  };
};
