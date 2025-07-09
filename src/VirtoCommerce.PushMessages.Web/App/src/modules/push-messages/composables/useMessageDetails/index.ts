import { computed, ComputedRef, reactive, Ref, ref } from "vue";
import { useApiClient, useAsync, useModificationTracker, useLoading } from "@vc-shell/framework";

import {
  CustomerModuleClient,
  MemberSearchResult,
  MembersSearchCriteria,
} from "../../../../api_client/virtocommerce.customer";
import { IPushMessage, PushMessage, PushMessageClient } from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient: getCustomerApiClient } = useApiClient(CustomerModuleClient);
const { getApiClient: getPushMessageApiClient } = useApiClient(PushMessageClient);

export interface UseMessageDetailsOptions {
  id?: string;
  sourceMessage?: PushMessage;
}

export interface IUseMessageDetails {
  item: Ref<IPushMessage>;
  isModified: Readonly<Ref<boolean>>;
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
  const item = ref<IPushMessage>(new PushMessage());
  const isNew = ref(!options?.id);
  const memberCount = ref<number>();

  const { currentValue, isModified, resetModificationState } = useModificationTracker(item);

  // Async actions
  const { action: loadMessage, loading: loadingMessage } = useAsync(async () => {
    if (options?.id) {
      const apiClient = await getPushMessageApiClient();
      const result = await apiClient.get(options.id, "WithMembers");
      currentValue.value = reactive(result);
    } else if (options?.sourceMessage) {
      // Clone from source message
      const cloned = new PushMessage();
      cloned.topic = options.sourceMessage.topic;
      cloned.shortMessage = options.sourceMessage.shortMessage;
      cloned.memberIds = options.sourceMessage.memberIds;
      cloned.memberQuery = options.sourceMessage.memberQuery;
      cloned.trackNewRecipients = options.sourceMessage.trackNewRecipients;
      currentValue.value = reactive(cloned);
    } else {
      // New message
      currentValue.value = reactive(new PushMessage());
    }
    resetModificationState();
  });

  const { action: saveMessage, loading: savingMessage } = useAsync(async (status?: string) => {
    const apiClient = await getPushMessageApiClient();

    let result: PushMessage;

    if (isNew.value) {
      if (status) {
        currentValue.value.status = status;
      }
      result = await apiClient.create(new PushMessage(currentValue.value));
    } else if (currentValue.value.status !== "Sent") {
      if (status) {
        currentValue.value.status = status;
      }
      result = await apiClient.update(new PushMessage(currentValue.value));
    } else {
      // Only track new recipients for sent messages
      result = await apiClient.changeTracking(currentValue.value.id!, currentValue.value.trackNewRecipients!);
    }

    currentValue.value = reactive(result);
    resetModificationState();

    return result;
  });

  const { action: deleteMessage, loading: deletingMessage } = useAsync(async () => {
    if (currentValue.value.id) {
      const apiClient = await getPushMessageApiClient();
      await apiClient.delete([currentValue.value.id]);

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
    if (currentValue.value?.memberQuery) {
      const apiClient = await getCustomerApiClient();
      const result = await apiClient.searchMember({
        keyword: currentValue.value.memberQuery,
        deepSearch: true,
        take: 0,
      } as MembersSearchCriteria);
      memberCount.value = result.totalCount;
    }
  });

  // Computed properties
  const loading = useLoading(loadingMessage, savingMessage, deletingMessage);

  const showMemberIds = computed(() => {
    return !currentValue.value?.memberQuery;
  });

  const showMemberQuery = computed(() => {
    return !currentValue.value?.memberIds || currentValue.value.memberIds.length === 0;
  });

  return {
    // State
    item: currentValue,
    isModified,
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
