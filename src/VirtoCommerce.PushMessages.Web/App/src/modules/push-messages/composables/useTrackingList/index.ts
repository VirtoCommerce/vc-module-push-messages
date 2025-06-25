import { computed, ref, ComputedRef, Ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";

import {
  IPushMessageSearchCriteria,
  PushMessage,
  PushMessageClient,
  PushMessageSearchCriteria,
  PushMessageSearchResult,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface UseTrackingListOptions {
  pageSize?: number;
  sort?: string;
}

export interface IUseTrackingList {
  items: ComputedRef<PushMessage[]>;
  totalCount: ComputedRef<number>;
  pages: ComputedRef<number>;
  currentPage: ComputedRef<number>;
  searchQuery: Ref<IPushMessageSearchCriteria>;
  loadMessages: (query?: IPushMessageSearchCriteria) => Promise<void>;
  removeMessages: (query?: { ids: string[] }) => Promise<void>;
  loading: ComputedRef<boolean>;
}

export function useTrackingList(options?: UseTrackingListOptions): IUseTrackingList {
  const pageSize = options?.pageSize || 20;
  const searchQuery = ref({
    take: pageSize,
    sort: options?.sort || "modifiedDate:desc",
    skip: 0,
    trackNewRecipients: true,
    isDraft: false,
  });
  const searchResult = ref<PushMessageSearchResult>();

  const { action: loadMessages, loading: loadingMessages } = useAsync<IPushMessageSearchCriteria>(async (_query) => {
    searchQuery.value = {
      ...searchQuery.value,
      ...(_query || {}),
      trackNewRecipients: true, // Always filter for messages tracking new recipients
      isDraft: false, // Exclude draft messages
    };

    const criteria = new PushMessageSearchCriteria(searchQuery.value);
    criteria.responseGroup = "WithReadRate";
    criteria.trackNewRecipients = true;
    criteria.isDraft = false;
    searchResult.value = await (await getApiClient()).search(criteria);
  });

  const { action: removeMessages, loading: loadingRemoveMessages } = useAsync<{ ids: string[] }>(async (_query) => {
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
    loadMessages,
    removeMessages,
    loading: useLoading(loadingMessages, loadingRemoveMessages),
  };
}
