<template>
  <BaseListBlade
    v-bind="$props"
    ref="baseListBladeRef"
    :title="title"
    state-key="sent_message_list"
    :columns="columns"
    :items="items"
    :total-count="totalCount"
    :pages="pages"
    :current-page="currentPage"
    :loading="loading"
    :load-messages="loadMessages"
    :remove-messages="removeMessages"
    :search-query="searchQuery"
    @parent:call="$emit('parent:call', $event)"
    @close:blade="$emit('close:blade')"
    @collapse:blade="$emit('collapse:blade')"
    @expand:blade="$emit('expand:blade')"
  />
</template>

<script setup lang="ts">
import { computed, useTemplateRef } from "vue";
import { useI18n } from "vue-i18n";
import { IParentCallArgs } from "@vc-shell/framework";
import { useSentList } from "../composables/useSentList";
import { useMessageListColumns } from "../utils/columns";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
import BaseListBlade from "../components/BaseListBlade.vue";

export interface Props {
  expanded?: boolean;
  closable?: boolean;
  param?: string;
  options?: unknown;
}

export interface Emits {
  (event: "parent:call", args: IParentCallArgs): void;
  (event: "close:blade"): void;
  (event: "collapse:blade"): void;
  (event: "expand:blade"): void;
}

defineProps<Props>();
defineEmits<Emits>();

defineOptions({
  name: "PushMessageSentList",
  url: "/sent",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.SENT",
    icon: "material-send",
    priority: 5,
  },
});

const { t } = useI18n({ useScope: "global" });

const { loadMessages, searchQuery, currentPage, removeMessages, totalCount, items, loading, pages } = useSentList();
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

defineExpose({
  title,
  reload,
  onAddNewMessage,
  onItemClick,
});
</script>
