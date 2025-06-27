<template>
  <BaseListBlade
    v-bind="$props"
    ref="baseListBladeRef"
    :title="title"
    state-key="draft_list"
    :columns="columns"
    :items="items"
    :total-count="totalCount"
    :pages="pages"
    :current-page="currentPage"
    :loading="loading"
    :load-messages="loadDrafts"
    :remove-messages="removeDrafts"
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
import { useDraftList } from "../composables/useDraftList";
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
  name: "PushMessageDraftList",
  url: "/drafts",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.DRAFTS",
    icon: "edit",
    priority: 2,
  },
});

const { t } = useI18n({ useScope: "global" });

const { loadDrafts, searchQuery, currentPage, removeDrafts, totalCount, items, loading, pages } = useDraftList();

const baseListBladeRef = useTemplateRef("baseListBladeRef");

const title = computed(() => t("PUSH_MESSAGES.PAGES.LIST.TITLE"));

const columns = useMessageListColumns({
  hiddenColumns: ["trackNewRecipients", "recipientsTotalCount", "recipientsReadCount", "recipientsReadPercent"],
});

const reload = () => {
  baseListBladeRef.value?.reload();
};

const onItemClick = (item: PushMessage) => {
  baseListBladeRef.value?.onItemClick(item);
};

function onAddNewDraft(...args: unknown[]) {
  baseListBladeRef.value?.onAddNewMessage(...args);
}

// Expose the same API as the original component
defineExpose({
  title,
  reload,
  onAddNewDraft,
  onItemClick,
});
</script>
