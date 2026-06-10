<template>
  <BaseListBlade
    v-bind="$props"
    ref="baseListBladeRef"
    :title="title"
    state-key="draft_list"
    :columns="columns"
    :items="items"
    :pagination="pagination"
    :loading="loading"
    :load-messages="loadDrafts"
    :remove-messages="removeDrafts"
    :search-query="searchQuery"
  />
</template>

<script setup lang="ts">
import { computed, useTemplateRef } from "vue";
import { useI18n } from "vue-i18n";
import { useDraftList } from "../composables/useDraftList";
import { useMessageListColumns } from "../utils/columns";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
import BaseListBlade from "../components/BaseListBlade.vue";
import { useBlade } from "@vc-shell/framework";

const { exposeToChildren } = useBlade();

defineBlade({
  name: "PushMessageDraftList",
  url: "/drafts",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.DRAFTS",
    icon: "lucide-square-pen",
    priority: 2,
  },
});

const { t } = useI18n({ useScope: "global" });

const { loadDrafts, searchQuery, removeDrafts, items, loading, pagination } = useDraftList();

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

function onAddNewMessage(args: { options?: Record<string, unknown> }) {
  baseListBladeRef.value?.onAddNewMessage(args);
}

// Expose the same API as the original component
exposeToChildren({
  reload,
  onAddNewMessage,
  onItemClick,
});
</script>
