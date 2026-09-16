import { ComputedRef, reactive, Ref, ref } from "vue";
import { useApiClient, useAsync, useLoading } from "@vc-shell/framework";

import { PushMessage, PushMessageClient } from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient: getPushMessageApiClient } = useApiClient(PushMessageClient);

export interface UseMessageDetailsOptions {
  id?: string;
  sourceMessage?: PushMessage;
}

export interface IUseMessageDetails {
  item: Ref<PushMessage>;
  loading: ComputedRef<boolean>;
  loadMessage: () => Promise<void>;
  saveMessage: (status?: string) => Promise<PushMessage>;
  deleteMessage: () => Promise<void>;
}

export function useMessageDetails(options?: UseMessageDetailsOptions): IUseMessageDetails {
  const item = ref<PushMessage>({} as PushMessage);
  const isNew = ref(!options?.id);

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
        ...item.value,
      } as PushMessage);
    } else if (item.value.status !== "Sent") {
      if (status) {
        item.value.status = status;
      }
      result = await apiClient.update({
        ...item.value,
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

  // Computed properties
  const loading = useLoading(loadingMessage, savingMessage, deletingMessage);

  return {
    // State
    item,
    loading,

    // Actions
    loadMessage,
    saveMessage,
    deleteMessage,
  };
}
