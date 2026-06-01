import { computed, ComputedRef, reactive, Ref, ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";

import {
  CustomerModuleClient,
  MemberSearchResult,
  MembersSearchCriteria,
} from "../../../../api_client/virtocommerce.customer";
import { PushMessage, PushMessageClient } from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient: getCustomerApiClient } = useApiClient(CustomerModuleClient);
const { getApiClient: getPushMessageApiClient } = useApiClient(PushMessageClient);

export interface UseMessageDetailsOptions {
  id?: string;
  sourceMessage?: PushMessage;
}

export interface IUseMessageDetails {
  item: Ref<PushMessage>;
  memberCount: Ref<number | undefined>;
  loading: ComputedRef<boolean>;
  showMemberIds: ComputedRef<boolean>;
  showMemberQuery: ComputedRef<boolean>;
  loadMembers: (keyword?: string, skip?: number, ids?: string[]) => Promise<MemberSearchResult>;
  loadMessage: () => Promise<void>;
  saveMessage: (status?: string) => Promise<PushMessage>;
  deleteMessage: () => Promise<void>;
  countMembers: () => Promise<void>;
  countingMembers: Readonly<Ref<boolean>>;
}

export function useMessageDetails(options?: UseMessageDetailsOptions): IUseMessageDetails {
  const item = ref<PushMessage>({} as PushMessage);
  const isNew = ref(!options?.id);
  const memberCount = ref<number>();

  // Async actions
  const { action: loadMessage, loading: loadingMessage } = useAsync(async () => {
    if (options?.id) {
      const apiClient = await getPushMessageApiClient();
      const result = await apiClient.get(options.id, "WithMembers");
      item.value = reactive(result);
    } else if (options?.sourceMessage) {
      // Clone from source message
      const cloned = {
        topic: options.sourceMessage.topic,
        shortMessage: options.sourceMessage.shortMessage,
        memberIds: options.sourceMessage.memberIds,
        memberQuery: options.sourceMessage.memberQuery,
        trackNewRecipients: options.sourceMessage.trackNewRecipients,
      } as PushMessage;
      item.value = reactive(cloned);
    } else {
      // New message
      item.value = reactive({} as PushMessage);
    }
  });

  const { action: saveMessage, loading: savingMessage } = useAsync(async (status?: string) => {
    const apiClient = await getPushMessageApiClient();

    let result: PushMessage;

    if (isNew.value) {
      if (status) {
        item.value.status = status;
      }
      result = await apiClient.create({
        ...item.value
      } as PushMessage);
    } else if (item.value.status !== "Sent") {
      if (status) {
        item.value.status = status;
      }
      result = await apiClient.update({
        ...item.value
      } as PushMessage);
    } else {
      // Only track new recipients for sent messages
      result = await apiClient.changeTracking(item.value.id!, item.value.trackNewRecipients!);
    }

    item.value = reactive(result);

    return result;
  });

  const { action: deleteMessage, loading: deletingMessage } = useAsync(async () => {
    if (item.value.id) {
      const apiClient = await getPushMessageApiClient();
      await apiClient.delete([item.value.id]);

      console.log("Message deleted successfully");
    }
  });

  async function loadMembers(keyword?: string, skip?: number, ids?: string[]) {
    const apiClient = await getCustomerApiClient();
    return apiClient.searchMember({
      keyword: keyword,
      objectIds: ids,
      deepSearch: true,
      objectType: "Member",
      sort: "MemberType:desc;Name",
      skip: skip || 0,
      take: ids?.length ?? 20,
    } as MembersSearchCriteria);
  }

  const { action: countMembers, loading: countingMembers } = useAsync(async () => {
    if (item.value?.memberQuery) {
      const apiClient = await getCustomerApiClient();
      const result = await apiClient.searchMember({
        keyword: item.value.memberQuery,
        deepSearch: true,
        take: 0,
      } as MembersSearchCriteria);
      memberCount.value = result.totalCount;
    }
  });

  // Computed properties
  const loading = useLoading(loadingMessage, savingMessage, deletingMessage);

  const showMemberIds = computed(() => {
    return !item.value?.memberQuery;
  });

  const showMemberQuery = computed(() => {
    return !item.value?.memberIds || item.value.memberIds.length === 0;
  });

  return {
    // State
    item,
    memberCount,
    loading,

    // Computed
    showMemberIds,
    showMemberQuery,

    // Actions
    loadMessage,
    saveMessage,
    deleteMessage,
    loadMembers,
    countMembers,

    // Loading states
    countingMembers,
  };
}
