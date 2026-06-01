<template>
  <BaseListBlade
    v-bind="$props"
    ref="baseListBladeRef"
    :title="title"
    state-key="sent_message_list"
    :columns="columns"
    :items="items"
    :pagination="pagination"
    :loading="loading"
    :load-messages="loadMessages"
    :remove-messages="removeMessages"
    :search-query="searchQuery"
  />
</template>

<script setup lang="ts">
import { computed, useTemplateRef } from "vue";
import { useI18n } from "vue-i18n";
import { useSentList } from "../composables/useSentList";
import { useMessageListColumns } from "../utils/columns";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
import BaseListBlade from "../components/BaseListBlade.vue";
import { useBlade } from "@vc-shell/framework";

const { exposeToChildren } = useBlade();

defineBlade({
  name: "PushMessageSentList",
  url: "/sent",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.SENT",
    icon: "lucide-send",
    priority: 5,
  },
});

const { t } = useI18n({ useScope: "global" });

const { loadMessages, searchQuery, removeMessages, items, loading, pagination } = useSentList();
const baseListBladeRef = useTemplateRef("baseListBladeRef");

const title = computed(() => t("PUSH_MESSAGES.PAGES.LIST.TITLE"));

const columns = useMessageListColumns({
  showTrackNewRecipients: true,
  showReadStats: true,
});

const reload = () => {
  baseListBladeRef.value?.reload();
};

const onItemClick = (item: PushMessage) => {
  baseListBladeRef.value?.onItemClick(item);
};

function onAddNewMessage(args: { options?: Record<string, unknown> }) {
  baseListBladeRef.value?.onAddNewMessage(args);
}

exposeToChildren({
  reload,
  onAddNewMessage,
  onItemClick,
});
</script>
