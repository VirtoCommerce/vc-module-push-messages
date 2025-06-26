<template>
  <BaseListBlade
    v-bind="$props"
    :title="title"
    state-key="message_list"
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
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { IParentCallArgs, useBladeNavigation } from "@vc-shell/framework";
import { useMessageList } from "../composables/useMessageList";
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
  name: "PushMessageList",
  url: "/all",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.ALL",
    icon: "material-mail",
    priority: 1,
  },
});

const { t } = useI18n({ useScope: "global" });

const { loadMessages, searchQuery, currentPage, removeMessages, totalCount, items, loading, pages } = useMessageList();

const title = computed(() => t("PUSH_MESSAGES.PAGES.LIST.TITLE"));

const columns = useMessageListColumns({
  showTrackNewRecipients: true,
  hiddenColumns: ["recipientsTotalCount", "recipientsReadCount", "recipientsReadPercent"],
});

// Expose the same API as the original component
const reload = async () => {
  await loadMessages({
    ...searchQuery.value,
    skip: (currentPage.value - 1) * (searchQuery.value.take ?? 20),
  });
};

function onAddNewMessage(...args: unknown[]) {
  const { openBlade } = useBladeNavigation();
  openBlade({
    blade: {
      name: "PushMessageDetails",
    },
    ...args,
  });
}

function onItemClick(item: PushMessage) {
  const { openBlade } = useBladeNavigation();
  openBlade({
    blade: {
      name: "PushMessageDetails",
    },
    param: item.id,
  });
}

defineExpose({
  title,
  reload,
  onAddNewMessage,
  onItemClick,
});
</script>
