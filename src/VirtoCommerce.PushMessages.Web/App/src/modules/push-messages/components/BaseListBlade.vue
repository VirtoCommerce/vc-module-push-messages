<template>
  <VcBlade :title="title" width="50%" :toolbar-items="bladeToolbar"
  >
    <VcDataTable
      :loading="loading"
      :items="items"
      :total-count="pagination.totalCount"
      :pagination="pagination"
      :selection-mode="'multiple'"
      :item-action-builder="actionBuilder"
      enable-item-actions
      v-model:active-item-id="selectedItemId"
      v-model:selection="selectedMessages"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :searchable="true"
      :pull-to-refresh="true"
      :state-key="stateKey"
      @search="onSearchList"
      @row-click="handleRowClick"
      @pagination-click="pagination.goToPage"
      @pull-refresh="reload"
    >
      <VcColumn
        v-for="col in columns"
        :key="col.id"
        :id="col.id"
        :title="unref(col.title)"
        :sortable="col.sortable"
        :always-visible="col.alwaysVisible"
        :type="normalizeColumnType(col.type)"
        :width="col.width"
        :visible="col.visible"
        :field="col.id"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { IActionBuilderResult, IBladeToolbar, ITableColumns, useBlade, usePopup, useDataTableSort } from "@vc-shell/framework";
import type { VcColumnCellType } from "@vc-shell/framework";
import { computed, onMounted, ref, unref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { debounce } from "lodash-es";
import { PushMessage, PushMessageSearchCriteria } from "../../../api_client/virtocommerce.pushmessages";
import type { UseDataTablePaginationReturn } from "@vc-shell/framework";

import { VcBlade, VcDataTable, VcColumn } from "@vc-shell/framework/ui";

export interface Props {
  // Configuration props
  title: string;
  stateKey: string;
  columns: ITableColumns[];
  // Composable props
  items: PushMessage[];
  pagination: UseDataTablePaginationReturn;
  loading: boolean;
  // Methods
  loadMessages: (query?: PushMessageSearchCriteria) => Promise<void>;
  removeMessages: (query?: { ids: string[] }) => Promise<void>;
  searchQuery: PushMessageSearchCriteria;
  // Optional customization
  customActions?: (item: PushMessage) => IActionBuilderResult[];
  hideDeleteSelected?: boolean;
  customToolbarItems?: IBladeToolbar[];
}

const props = defineProps<Props>();

const { t } = useI18n({ useScope: "global" });
const { param, openBlade } = useBlade();
const { showConfirmation } = usePopup();

const { sortField, sortOrder, sortExpression } = useDataTableSort({
  initialDirection: "DESC",
  initialField: "modifiedDate",
});

const selectedItemId = ref<string>();
const selectedMessages = ref<PushMessage[]>([]);

const selectedMessageIds = computed(() => selectedMessages.value.map((message) => message.id!));

function normalizeColumnType(type?: string): VcColumnCellType | undefined {
  if (!type) {
    return undefined;
  }
  return (type === "date-time" ? "datetime" : type) as VcColumnCellType;
}

const deletableMessageIds = computed(() => {
  if (selectedMessageIds.value.length === 0) {
    return [];
  }

  // Filter out messages with status "Sent"
  return props.items.filter((item) => canDelete(item) && selectedMessageIds.value.includes(item.id!)).map((item) => item.id!);
});

const bladeToolbar = computed((): IBladeToolbar[] => {
  const defaultItems: IBladeToolbar[] = [
    {
      id: "refresh",
      icon: "lucide-refresh-cw",
      title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.REFRESH"),
      clickHandler: async () => {
        await reload();
      },
    },
    {
      id: "add",
      icon: "lucide-plus",
      title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.NEW"),
      clickHandler: onAddNewMessage,
    },
  ];

  if (!props.hideDeleteSelected) {
    defaultItems.push({
      id: "deleteSelected",
      icon: "lucide-trash-2",
      title: t("PUSH_MESSAGES.PAGES.LIST.TOOLBAR.DELETE"),
      disabled: deletableMessageIds.value.length === 0,
      clickHandler: async () => {
        if (
          await showConfirmation(
            t("PUSH_MESSAGES.PAGES.ALERTS.DELETE_SELECTED_CONFIRMATION.MESSAGE", {
              count: deletableMessageIds.value.length,
            }),
          )
        ) {
          await props.removeMessages({ ids: deletableMessageIds.value });
          await reload();
          selectedMessages.value = [];
        }
      },
    });
  }

  return [...defaultItems, ...(props.customToolbarItems || [])];
});

watch(
  () => param.value,
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

  if (canDelete(item)) {
    result.push({
      icon: "lucide-trash-2",
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

function canDelete(item: PushMessage) {
  return item.status !== "Sent";
}

const reload = async () => {
  selectedMessages.value = [];

  await props.loadMessages({
    ...props.searchQuery,
    skip: props.pagination.skip,
    sort: sortExpression.value,
  });
};

const onSearchList = debounce(async (keyword: string | undefined) => {
  console.debug(`List search by ${keyword}`);
  await props.loadMessages({
    ...props.searchQuery,
    keyword,
  });
}, 1000);

function handleRowClick(event: { data: PushMessage }) {
  onItemClick(event.data);
}

function onItemClick(item: PushMessage) {
  console.log("onItemClick", item);
  openBlade({
    name: "PushMessageDetails",
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
    name: "PushMessageDetails",
    ...args,
  });
}

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
