<template>
  <VcBlade
    :title="title"
    width="50%"
    :expanded="expanded"
    :closable="closable"
    :toolbar-items="bladeToolbar"
    @close="$emit('close:blade')"
    @expand="$emit('expand:blade')"
    @collapse="$emit('collapse:blade')"
  >
    <!--@vue-generic {PushMessage}-->
    <VcTable
      :loading="loading"
      :expanded="expanded"
      :items="items"
      :columns="columns"
      multiselect
      :item-action-builder="actionBuilder"
      enable-item-actions
      :selected-item-id="selectedItemId"
      :selected-items="selectedMessageIds"
      :sort="sortExpression"
      :pages="pages"
      :current-page="currentPage"
      :search-value="searchValue"
      :total-count="totalCount"
      state-key="scheduled_message_list"
      @search:change="onSearchList"
      @item-click="onItemClick"
      @header-click="onHeaderClick"
      @pagination-click="onPaginationClick"
      @scroll:ptr="reload"
      @selection-changed="onSelectionChanged"
    ></VcTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  IActionBuilderResult,
  IBladeToolbar,
  IParentCallArgs,
  ITableColumns,
  useBladeNavigation,
  usePopup,
  useTableSort,
} from "@vc-shell/framework";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useScheduledList } from "../composables/useScheduledList";
import { debounce } from "lodash-es";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";

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

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

defineOptions({
  name: "PushMessageScheduledList",
  url: "/scheduled",
  isWorkspace: true,
  menuItem: {
    title: "PUSH_MESSAGES.MENU.SCHEDULED",
    icon: "material-schedule",
    priority: 3,
  },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBladeNavigation();

const {
  currentSort,
  sortExpression,
  handleSortChange: tableSortHandler,
  resetSort,
} = useTableSort({
  initialDirection: "DESC",
  initialProperty: "modifiedDate",
});

const { showConfirmation } = usePopup();

const searchValue = ref();
const selectedItemId = ref<string>();
const selectedMessageIds = ref<string[]>([]);

const { loadMessages, searchQuery, currentPage, removeMessages, totalCount, items, loading, pages } =
  useScheduledList();

const title = computed(() => t("PUSH_MESSAGES.PAGES.LIST.TITLE"));

const bladeToolbar = computed((): IBladeToolbar[] => [
  {
    id: "refresh",
    icon: "material-refresh",
    title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.REFRESH"),
    clickHandler: async () => {
      await reload();
    },
  },
  {
    id: "add",
    icon: "material-add",
    title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.NEW"),
    clickHandler: onAddNewMessage,
  },
  {
    id: "deleteSelected",
    icon: "material-delete",
    title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.DELETE"),
    disabled: selectedMessageIds.value.length === 0,
    clickHandler: async () => {
      if (
        await showConfirmation(
          t("PUSH_MESSAGES.PAGES.ALERTS.DELETE_SELECTED_CONFIRMATION.MESSAGE", {
            count: selectedMessageIds.value.length,
          }),
        )
      ) {
        await removeMessages({ ids: selectedMessageIds.value });
        await reload();
        selectedMessageIds.value = [];
      }
    },
  },
]);

const columns = computed(() => [
  {
    id: "status",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.STATUS"),
    sortable: true,
    alwaysVisible: true,
  },
  {
    id: "topic",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.TOPIC"),
    sortable: true,
  },
  {
    id: "shortMessage",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.SHORT_MESSAGE"),
    type: "html",
    alwaysVisible: true,
  },
  {
    id: "startDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.START_DATE"),
    type: "date-time",
    sortable: true,
    visible: true,
  },
  {
    id: "createdDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.CREATED_DATE"),
    type: "date-time",
    sortable: true,
    visible: false,
  },
  {
    id: "modifiedDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.MODIFIED_DATE"),
    type: "date-time",
    sortable: true,
    alwaysVisible: true,
  },
  {
    id: "recipientsTotalCount",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_TOTAL_COUNT"),
    visible: false,
  },
  {
    id: "recipientsReadCount",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_COUNT"),
    visible: false,
  },
  {
    id: "recipientsReadPercent",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_PERCENT"),
    visible: false,
  },
]);

watch(
  () => props.param,
  (newVal) => {
    selectedItemId.value = newVal;
  },
  { immediate: true },
);

watch(
  () => sortExpression.value,
  async (newVal) => {
    await loadMessages({
      ...searchQuery.value,
      sort: newVal,
    });
  },
);

const actionBuilder = (item: PushMessage): IActionBuilderResult[] => {
  const result: IActionBuilderResult[] = [];

  // Only show delete action for scheduled messages (not sent)
  if (item.status !== "Sent") {
    result.push({
      icon: "material-delete",
      title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.ACTIONS.DELETE"),
      type: "danger",
      async clickHandler() {
        if (item.id && (await showConfirmation(t("PUSH_MESSAGES.PAGES.ALERTS.DELETE")))) {
          await removeMessages({ ids: [item.id] });
          await reload();
        }
      },
    });
  }

  return result;
};

const reload = async () => {
  selectedMessageIds.value = [];

  await loadMessages({
    ...searchQuery.value,
    skip: (currentPage.value - 1) * (searchQuery.value.take ?? 20),
    sort: sortExpression.value,
  });
};

const onSearchList = debounce(async (keyword: string | undefined) => {
  console.debug(`Scheduled messages list search by ${keyword}`);
  searchValue.value = keyword;
  await loadMessages({
    ...searchQuery.value,
    keyword,
  });
}, 1000);

function onItemClick(item: PushMessage) {
  openBlade({
    blade: {
      name: "PushMessageDetails",
    },
    param: item.id,
    onOpen() {
      selectedItemId.value = item.id;
    },
    onClose() {
      selectedItemId.value = undefined;
    },
  });
}

function onAddNewMessage(...args: unknown[]) {
  openBlade({
    blade: {
      name: "PushMessageDetails",
    },
    ...args,
  });
}

function onHeaderClick(item: ITableColumns) {
  tableSortHandler(item.id);
}

const onPaginationClick = async (page: number) => {
  await loadMessages({
    ...searchQuery.value,
    skip: (page - 1) * (searchQuery.value.take ?? 20),
  });
};

const onSelectionChanged = (items: PushMessage[]) => {
  selectedMessageIds.value = items.map((item) => item.id!);
};

onMounted(async () => {
  await loadMessages();
});

defineExpose({
  title,
  reload,
  onAddNewMessage,
  onItemClick,
});
</script>
