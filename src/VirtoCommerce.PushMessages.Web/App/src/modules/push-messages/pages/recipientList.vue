<template>
  <VcBlade
    :title="title"
    width="50%"
    :expanded="expanded"
    :closable="closable"
    @close="$emit('close:blade')"
    @expand="$emit('expand:blade')"
    @collapse="$emit('collapse:blade')"
  >
    <!--@vue-generic {PushMessageRecipient}-->
    <VcTable
      :loading="loading"
      :expanded="expanded"
      :items="items"
      :columns="columns"
      :sort="sortExpression"
      :pages="pages"
      :current-page="currentPage"
      :total-count="totalCount"
      :search-value="searchValue"
      state-key="recipient_list"
      @header-click="onHeaderClick"
      @pagination-click="onPaginationClick"
      @scroll:ptr="reload"
      @search:change="onSearchList"
    ></VcTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { ITableColumns, IParentCallArgs, useTableSort } from "@vc-shell/framework";
import { useRecipientList } from "../composables/useRecipientList";
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { PushMessageRecipient } from "../../../api_client/virtocommerce.pushmessages";
import { debounce } from "lodash-es";

export interface Props {
  expanded?: boolean;
  closable?: boolean;
  param?: string;
  options: {
    messageId: string;
  };
}

export interface Emits {
  (event: "close:blade"): void;
  (event: "collapse:blade"): void;
  (event: "expand:blade"): void;
  (event: "parent:call", args: IParentCallArgs): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

defineOptions({
  name: "PushMessageRecipientList",
  url: "/recipients",
  routable: false,
});

const { t } = useI18n({ useScope: "global" });

const {
  currentSort,
  sortExpression,
  handleSortChange: tableSortHandler,
} = useTableSort({
  initialDirection: "ASC",
  initialProperty: "MemberName;UserName",
});

const { items, totalCount, pages, currentPage, loadRecipients, loading, searchQuery } = useRecipientList({
  messageId: props.options.messageId,
  pageSize: 20,
});

const searchValue = ref();

const title = computed(() => t("PUSH_MESSAGES.PAGES.RECIPIENTS.TITLE"));

const columns = computed(() => [
  {
    id: "memberId",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_ID"),
    sortable: true,
    width: "21em",
    visible: false,
  },
  {
    id: "memberName",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_NAME"),
    sortable: true,
  },
  {
    id: "userId",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_ID"),
    sortable: true,
    width: "21em",
    visible: false,
  },
  {
    id: "userName",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_NAME"),
    sortable: true,
  },
  {
    id: "isRead",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_READ"),
    sortable: true,
    width: "6em",
  },
  {
    id: "isHidden",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_HIDDEN"),
    sortable: true,
    width: "6em",
    visible: false,
  },
  {
    id: "createdDate",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.CREATED_DATE"),
    type: "date-time",
    sortable: true,
    visible: false,
  },
  {
    id: "modifiedDate",
    title: t("PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MODIFIED_DATE"),
    type: "date-time",
    sortable: true,
    visible: false,
  },
]);

// Watch for sort changes
watch(
  () => sortExpression.value,
  async (newVal) => {
    await loadRecipients({
      ...searchQuery.value,
      sort: newVal,
    });
  },
);

// Methods
const reload = async () => {
  await loadRecipients({
    ...searchQuery.value,
    skip: (currentPage.value - 1) * (searchQuery.value.take ?? 20),
    sort: sortExpression.value,
  });
};

function onHeaderClick(item: ITableColumns) {
  tableSortHandler(item.id);
}

const onPaginationClick = async (page: number) => {
  await loadRecipients({
    ...searchQuery.value,
    skip: (page - 1) * (searchQuery.value.take ?? 20),
  });
};

const onSearchList = debounce(async (keyword: string | undefined) => {
  console.debug(`Recipients list search by ${keyword}`);
  searchValue.value = keyword;
  await loadRecipients({
    ...searchQuery.value,
    keyword,
  });
}, 1000);

// Lifecycle
onMounted(async () => {
  await loadRecipients({
    sort: sortExpression.value,
  });
});

defineExpose({
  title,
  reload,
});
</script>