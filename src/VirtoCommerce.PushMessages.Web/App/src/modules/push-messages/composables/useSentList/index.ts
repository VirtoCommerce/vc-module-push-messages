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
    load: async (_query) => {
      const criteria = { ...(_query || {}) } as PushMessageSearchCriteria;
      criteria.statuses = ["Sent"];
      criteria.responseGroup = "WithReadRate";
      return (await getApiClient()).search(criteria);
    },
    remove: async (_query, customQuery) => {
      const ids = customQuery.ids;
      if (ids) {
        return (await getApiClient()).delete(ids);
      }
    },
  });

  const { load, remove, items, pagination, loading, query } = listFactory({ sort: "modifiedDate:desc", pageSize: 20 });
  const { openBlade, resolveBladeByName } = useBladeNavigation();

  async function openDetailsBlade(data?: Omit<Parameters<typeof openBlade>["0"], "blade">) {
    await openBlade({
      blade: resolveBladeByName("PushMessageDetails"),
      ...data,
    });
  }

  const scope: PushMessageListScope = {
    openDetailsBlade,
    isReadOnly: (data: { item: PushMessage }) => {
      return data.item.status === "Sent";
    },
  };

  return {
    items,
    load,
    remove,
    loading,
    pagination,
    query,
    scope,
  };
};
