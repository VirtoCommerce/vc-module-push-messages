<template>
  <VcWidget
    v-if="modelValue.item?.id"
    v-loading="loading"
    :value="count"
    :title="$t('PUSH_MESSAGES.PAGES.DETAILS.WIDGETS.RECIPIENTS')"
    icon="fas fa-user-check"
    @click="clickHandler"
  >
  </VcWidget>
</template>

<script setup lang="ts">
import { UnwrapNestedRefs, onMounted, ref } from "vue";
import { useAsync, useApiClient, useBladeNavigation } from "@vc-shell/framework";
import { useDetails } from "../../composables";
import {
  PushMessageClient,
  PushMessageRecipientSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { openBlade, resolveBladeByName } = useBladeNavigation();
const { getApiClient } = useApiClient(PushMessageClient);

interface Props {
  modelValue: UnwrapNestedRefs<ReturnType<typeof useDetails>>;
}

const props = defineProps<Props>();
const count = ref(0);
const widgetOpened = ref(false);

defineExpose({
  updateActiveWidgetCount: populateCounter,
});

onMounted(async () => {
  if (props.modelValue?.item?.id) {
    await populateCounter();
  }
});

async function populateCounter() {
  const messageId = props.modelValue?.item?.id;
  if (!messageId) {
    return;
  }
  const criteria = new PushMessageRecipientSearchCriteria();
  criteria.messageId = messageId;
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
      blade: resolveBladeByName("PushMessageRecipientList"),
      options: {
        messageId: props.modelValue?.item?.id,
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
</script>
