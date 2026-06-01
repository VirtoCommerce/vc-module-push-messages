import { computed, ref, ComputedRef, Ref } from "vue";
import { useApiClient, useAsync, useLoading, useDataTablePagination, type UseDataTablePaginationReturn } from "@vc-shell/framework";

import {
  PushMessageClient,
  PushMessageRecipient,
  PushMessageRecipientSearchCriteria,
  PushMessageRecipientSearchResult,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface UseRecipientListOptions {
  messageId: string;
  pageSize?: number;
  sort?: string;
}

export interface IUseRecipientList {
  items: ComputedRef<PushMessageRecipient[]>;
  pagination: UseDataTablePaginationReturn;
  searchQuery: Ref<PushMessageRecipientSearchCriteria>;
  loadRecipients: (query?: PushMessageRecipientSearchCriteria) => Promise<void>;
  loading: ComputedRef<boolean>;
}

export function useRecipientList(options: UseRecipientListOptions): IUseRecipientList {
  const pageSize = options.pageSize || 20;
  const searchQuery = ref({
    messageId: options.messageId,
    withHidden: true,
    take: pageSize,
    sort: options.sort || "MemberName;UserName",
    skip: 0,
  });
  const searchResult = ref<PushMessageRecipientSearchResult>();

  const { action: loadRecipients, loading: loadingRecipients } = useAsync<PushMessageRecipientSearchCriteria>(
    async (_query) => {
      searchQuery.value = {
        ...searchQuery.value,
        ...(_query || {}),
        messageId: options.messageId, // Always preserve messageId
        withHidden: true,
      };

      const criteria = {
        ...searchQuery.value
      } as PushMessageRecipientSearchCriteria;
      searchResult.value = await (await getApiClient()).searchRecipients(criteria);
    },
  );

  const pagination = useDataTablePagination({
    pageSize,
    totalCount: computed(() => searchResult.value?.totalCount ?? 0),
    onPageChange: ({ skip }) => loadRecipients({ ...searchQuery.value, skip }),
  });

  return {
    items: computed(() => searchResult.value?.results || []),
    pagination,
    searchQuery,
    loadRecipients,
    loading: useLoading(loadingRecipients),
  };
}
