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
      :state-key="stateKey"
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
import { IActionBuilderResult, IBladeToolbar, IParentCallArgs, ITableColumns, useBladeNavigation, usePopup, useTableSort } from "@vc-shell/framework";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { debounce } from "lodash-es";
import { PushMessage, IPushMessageSearchCriteria } from "../../../api_client/virtocommerce.pushmessages";

export interface Props {
  expanded?: boolean;
  closable?: boolean;
  param?: string;
  options?: unknown;
  // Configuration props
  title: string;
  stateKey: string;
  columns: ITableColumns[];
  // Composable props
  items: PushMessage[];
  totalCount: number;
  pages: number;
  currentPage: number;
  loading: boolean;
  // Methods
  loadMessages: (query?: IPushMessageSearchCriteria) => Promise<void>;
  removeMessages: (query?: { ids: string[] }) => Promise<void>;
  searchQuery: IPushMessageSearchCriteria;
  // Optional customization
  customActions?: (item: PushMessage) => IActionBuilderResult[];
  hideDeleteSelected?: boolean;
  customToolbarItems?: IBladeToolbar[];
}

export interface Emits {
  (event: "parent:call", args: IParentCallArgs): void;
  (event: "close:blade"): void;
  (event: "collapse:blade"): void;
  (event: "expand:blade"): void;
}

const props = defineProps<Props>();

const { t } = useI18n({ useScope: "global" });
const { openBlade } = useBladeNavigation();
const { showConfirmation } = usePopup();

const { sortExpression, handleSortChange: tableSortHandler } = useTableSort({
  initialDirection: "DESC",
  initialProperty: "modifiedDate",
});

const searchValue = ref();
const selectedItemId = ref<string>();
const selectedMessageIds = ref<string[]>([]);

const bladeToolbar = computed((): IBladeToolbar[] => {
  const defaultItems: IBladeToolbar[] = [
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
  ];

  if (!props.hideDeleteSelected) {
    defaultItems.push({
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
          await props.removeMessages({ ids: selectedMessageIds.value });
          await reload();
          selectedMessageIds.value = [];
        }
      },
    });
  }

  return [...defaultItems, ...(props.customToolbarItems || [])];
});

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
    await props.loadMessages({
      ...props.searchQuery,
      sort: newVal,
    });
  },
);

const actionBuilder = (item: PushMessage): IActionBuilderResult[] => {
  // Use custom actions if provided
  if (props.customActions) {
    return props.customActions(item);
  }

  // Default action builder
  const result: IActionBuilderResult[] = [];

  if (item.status !== "Sent") {
    result.push({
      icon: "material-delete",
      title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.ACTIONS.DELETE"),
      type: "danger",
      async clickHandler() {
        if (item.id && (await showConfirmation(t("PUSH_MESSAGES.PAGES.ALERTS.DELETE")))) {
          await props.removeMessages({ ids: [item.id] });
          await reload();
        }
      },
    });
  }

  return result;
};

const reload = async () => {
  selectedMessageIds.value = [];

  await props.loadMessages({
    ...props.searchQuery,
    skip: (props.currentPage - 1) * (props.searchQuery.take ?? 10),
    sort: sortExpression.value,
  });
};

const onSearchList = debounce(async (keyword: string | undefined) => {
  console.debug(`List search by ${keyword}`);
  searchValue.value = keyword;
  await props.loadMessages({
    ...props.searchQuery,
    keyword,
  });
}, 1000);

function onItemClick(item: PushMessage) {
  console.log("onItemClick", item);
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

function onAddNewMessage(args: { options?: Record<string, unknown> }) {
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
  await props.loadMessages({
    ...props.searchQuery,
    skip: (page - 1) * (props.searchQuery.take ?? 10),
  });
};

const onSelectionChanged = (messages: PushMessage[]) => {
  selectedMessageIds.value = messages.map((message) => message.id!);
};

onMounted(async () => {
  await props.loadMessages({
    sort: sortExpression.value,
  });
});

defineExpose({
  reload,
  onItemClick,
  onAddNewMessage,
});
</script>
