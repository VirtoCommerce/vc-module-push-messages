import {
  ListBaseBladeScope,
  useApiClient,
  useAsync,
  useBladeNavigation,
  useListFactory,
  useLoading,
  AsyncAction,
} from "@vc-shell/framework";

import {
  IPushMessageSearchCriteria,
  PushMessage,
  PushMessageClient,
  PushMessageSearchCriteria,
  PushMessageSearchResult,
} from "../../../../api_client/virtocommerce.pushmessages";
import { computed, ComputedRef, Ref, ref } from "vue";

export interface IUseDraftList {
  items: ComputedRef<PushMessage[]>;
  totalCount: ComputedRef<number>;
  pages: ComputedRef<number>;
  currentPage: ComputedRef<number>;
  searchQuery: Ref<IPushMessageSearchCriteria>;
  loadDrafts: (query?: IPushMessageSearchCriteria) => Promise<void>;
  removeDrafts: (query?: { ids: string[] }) => Promise<void>;
  loading: ComputedRef<boolean>;
}

const { getApiClient } = useApiClient(PushMessageClient);

export function useDraftList(options?: { pageSize?: number; sort?: string }): IUseDraftList {
  const pageSize = options?.pageSize || 20;
  const searchQuery = ref<IPushMessageSearchCriteria>({
    take: pageSize,
    sort: options?.sort,
  });
  const searchResult = ref<PushMessageSearchResult>();

  const { action: loadDrafts, loading: loadingDrafts } = useAsync<IPushMessageSearchCriteria>(async (_query) => {
    searchQuery.value = {
      ...searchQuery.value,
      ...(_query || {}),
    };
    const criteria = { ...searchQuery.value } as PushMessageSearchCriteria;
    criteria.statuses = ["Draft"];
    criteria.responseGroup = "None";
    searchResult.value = await (await getApiClient()).search(criteria);
  });

  const { action: removeDrafts, loading: loadingRemoveDrafts } = useAsync<{ ids: string[] }>(async (_query) => {
    const ids = _query?.ids;
    if (ids) {
      await (await getApiClient()).delete(ids);
    }
  });

  return {
    items: computed(() => searchResult.value?.results || []),
    totalCount: computed(() => searchResult.value?.totalCount || 0),
    pages: computed(() => Math.ceil((searchResult.value?.totalCount || 1) / pageSize)),
    currentPage: computed(() => Math.ceil((searchQuery.value?.skip || 0) / Math.max(1, pageSize) + 1)),
    searchQuery,
    loadDrafts,
    removeDrafts,
    loading: useLoading(loadingDrafts, loadingRemoveDrafts),
  };
}
