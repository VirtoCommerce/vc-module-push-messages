<template>
  <VcWidget
    v-loading="loading"
    :value="count"
    :title="$t('PUSH_MESSAGES.PAGES.DETAILS.WIDGETS.RECIPIENTS')"
    icon="material-how_to_reg"
    @click="clickHandler"
  >
  </VcWidget>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useAsync, useApiClient, useBladeNavigation } from "@vc-shell/framework";
import {
  PushMessageClient,
  PushMessageRecipientSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { openBlade } = useBladeNavigation();
const { getApiClient } = useApiClient(PushMessageClient);

interface Props {
  itemId: string;
}

const props = defineProps<Props>();
const count = ref(0);
const widgetOpened = ref(false);

async function populateCounter() {
  const messageId = props.itemId;
  if (!messageId) {
    return;
  }
  const criteria = new PushMessageRecipientSearchCriteria();
  criteria.messageId = messageId;
  criteria.withHidden = true;
  criteria.take = 0;
  count.value = (await getCount(criteria)) ?? 0;
}

const { loading, action: getCount } = useAsync<PushMessageRecipientSearchCriteria, number | undefined>(
  async (criteria) => {
    return (await getApiClient()).searchRecipients(criteria).then((result) => result.totalCount);
  },
);

function clickHandler() {
  if (!widgetOpened.value) {
    openBlade({
      blade: {
        name: "PushMessageRecipientList",
      },
      options: {
        messageId: props.itemId,
      },
      onOpen() {
        widgetOpened.value = true;
      },
      onClose() {
        widgetOpened.value = false;
      },
    });
  }
}

onMounted(async () => {
  if (props.itemId) {
    await populateCounter();
  }
});

defineExpose({
  updateActiveWidgetCount: populateCounter,
});
</script>
