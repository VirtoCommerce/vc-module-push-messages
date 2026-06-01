import { computed, ref, ComputedRef, Ref } from "vue";
import { useApiClient, useAsync, useLoading, useDataTablePagination, type UseDataTablePaginationReturn } from "@vc-shell/framework";

import {
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
  pagination: UseDataTablePaginationReturn;
  searchQuery: Ref<PushMessageSearchCriteria>;
  loadMessages: (query?: PushMessageSearchCriteria) => Promise<void>;
  removeMessages: (query?: { ids: string[] }) => Promise<void>;
  loading: ComputedRef<boolean>;
}

export function useBaseList(options?: BaseListOptions): IUseBaseList {
  const pageSize = options?.pageSize || 20;
  const defaultSort = options?.sort || "modifiedDate:desc";

  const searchQuery = ref<PushMessageSearchCriteria>({
    take: pageSize,
    sort: defaultSort,
    skip: 0,
    ...(options?.statuses && { statuses: options.statuses }),
    ...(options?.trackNewRecipients !== undefined && { trackNewRecipients: options.trackNewRecipients }),
    ...(options?.isDraft !== undefined && { isDraft: options.isDraft }),
  });

  const searchResult = ref<PushMessageSearchResult>();

  const { action: loadMessages, loading: loadingMessages } = useAsync<PushMessageSearchCriteria>(async (_query) => {
    searchQuery.value = {
      ...searchQuery.value,
      ...(_query || {}),
      // Preserve filter options
      ...(options?.statuses && { statuses: options.statuses }),
      ...(options?.trackNewRecipients !== undefined && { trackNewRecipients: options.trackNewRecipients }),
      ...(options?.isDraft !== undefined && { isDraft: options.isDraft }),
    };

    const criteria = {
      ...searchQuery.value
    } as PushMessageSearchCriteria;

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

  const pagination = useDataTablePagination({
    pageSize,
    totalCount: computed(() => searchResult.value?.totalCount ?? 0),
    onPageChange: ({ skip }) => loadMessages({ ...searchQuery.value, skip }),
  });

  return {
    items: computed(() => searchResult.value?.results || []),
    pagination,
    searchQuery,
    loadMessages,
    removeMessages,
    loading: useLoading(loadingMessages, loadingRemoveMessages),
  };
}
