import { computed, ref, ComputedRef, Ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";

import {
  IPushMessageRecipientSearchCriteria,
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
  totalCount: ComputedRef<number>;
  pages: ComputedRef<number>;
  currentPage: ComputedRef<number>;
  searchQuery: Ref<IPushMessageRecipientSearchCriteria>;
  loadRecipients: (query?: IPushMessageRecipientSearchCriteria) => Promise<void>;
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

  const { action: loadRecipients, loading: loadingRecipients } = useAsync<IPushMessageRecipientSearchCriteria>(
    async (_query) => {
      searchQuery.value = {
        ...searchQuery.value,
        ...(_query || {}),
        messageId: options.messageId, // Always preserve messageId
        withHidden: true,
      };

      const criteria = new PushMessageRecipientSearchCriteria(searchQuery.value);
      searchResult.value = await (await getApiClient()).searchRecipients(criteria);
    },
  );

  return {
    items: computed(() => searchResult.value?.results || []),
    totalCount: computed(() => searchResult.value?.totalCount || 0),
    pages: computed(() => Math.ceil((searchResult.value?.totalCount || 1) / pageSize)),
    currentPage: computed(() => Math.ceil((searchQuery.value?.skip || 0) / Math.max(1, pageSize) + 1)),
    searchQuery,
    loadRecipients,
    loading: useLoading(loadingRecipients),
  };
}
