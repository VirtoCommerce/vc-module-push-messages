import { computed, ref, type ComputedRef, type Ref } from "vue";
import { useBladeWidgets, useBlade, useApiClient } from "@vc-shell/framework";
import type { UseBladeWidgetsReturn } from "@vc-shell/framework";
import {
  PushMessageClient,
  PushMessageRecipientSearchCriteria,
} from "../../../api_client/virtocommerce.pushmessages";

interface UseRecipientsWidgetsOptions {
  itemId: Ref<string | undefined> | ComputedRef<string | undefined>;
  isVisible: ComputedRef<boolean> | Ref<boolean> | boolean;
}

export function useRecipientsWidgets(options: UseRecipientsWidgetsOptions): UseBladeWidgetsReturn {
  const { itemId, isVisible } = options;
  const { openBlade } = useBlade();
  const { getApiClient } = useApiClient(PushMessageClient);

  const count = ref(0);

  async function populateCounter() {
    const messageId = itemId.value;
    if (!messageId) {
      return;
    }
    const criteria = {
      messageId: messageId,
      withHidden: true,
      take: 0,
    } as PushMessageRecipientSearchCriteria;
    count.value = (await (await getApiClient()).searchRecipients(criteria).then((result) => result.totalCount)) ?? 0;
  }

  return useBladeWidgets([
    {
      id: "RecipientsWidget",
      icon: "lucide-user-check",
      title: "PUSH_MESSAGES.PAGES.DETAILS.WIDGETS.RECIPIENTS",
      badge: computed(() => count.value),
      isVisible,
      onClick: () =>
        openBlade({
          name: "PushMessageRecipientList",
          options: {
            messageId: itemId.value,
          },
        }),
      onRefresh: populateCounter,
    },
  ]);
}
