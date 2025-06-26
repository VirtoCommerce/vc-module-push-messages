import { computed, ref, ComputedRef, Ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";

import {
  IPushMessageSearchCriteria,
  PushMessage,
  PushMessageClient,
  PushMessageSearchCriteria,
  PushMessageSearchResult,
} from "../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface BaseListOptions {
  pageSize?: number;
  sort?: string;
  statuses?: string[];
  responseGroup?: string;
  trackNewRecipients?: boolean;
  isDraft?: boolean;
}

export interface IUseBaseList {
  items: ComputedRef<PushMessage[]>;
  totalCount: ComputedRef<number>;
  pages: ComputedRef<number>;
  currentPage: ComputedRef<number>;
  searchQuery: Ref<IPushMessageSearchCriteria>;
  loadMessages: (query?: IPushMessageSearchCriteria) => Promise<void>;
  removeMessages: (query?: { ids: string[] }) => Promise<void>;
  loading: ComputedRef<boolean>;
}

export function useBaseList(options?: BaseListOptions): IUseBaseList {
  const pageSize = options?.pageSize || 20;
  const defaultSort = options?.sort || "modifiedDate:desc";

  const searchQuery = ref<IPushMessageSearchCriteria>({
    take: pageSize,
    sort: defaultSort,
    skip: 0,
    ...(options?.statuses && { statuses: options.statuses }),
    ...(options?.trackNewRecipients !== undefined && { trackNewRecipients: options.trackNewRecipients }),
    ...(options?.isDraft !== undefined && { isDraft: options.isDraft }),
  });

  const searchResult = ref<PushMessageSearchResult>();

  const { action: loadMessages, loading: loadingMessages } = useAsync<IPushMessageSearchCriteria>(async (_query) => {
    searchQuery.value = {
      ...searchQuery.value,
      ...(_query || {}),
      // Preserve filter options
      ...(options?.statuses && { statuses: options.statuses }),
      ...(options?.trackNewRecipients !== undefined && { trackNewRecipients: options.trackNewRecipients }),
      ...(options?.isDraft !== undefined && { isDraft: options.isDraft }),
    };

    const criteria = new PushMessageSearchCriteria(searchQuery.value);

    // Apply response group if specified
    if (options?.responseGroup) {
      criteria.responseGroup = options.responseGroup;
    }

    // Apply status filters if specified
    if (options?.statuses) {
      criteria.statuses = options.statuses;
    }

    // Apply additional filters
    if (options?.trackNewRecipients !== undefined) {
      criteria.trackNewRecipients = options.trackNewRecipients;
    }

    if (options?.isDraft !== undefined) {
      criteria.isDraft = options.isDraft;
    }

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
