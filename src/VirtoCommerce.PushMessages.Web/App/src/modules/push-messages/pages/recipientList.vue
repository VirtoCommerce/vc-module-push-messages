<template>
  <VcBlade
    :title="title"
    width="50%"
  >
    <VcDataTable
      :loading="loading"
      :items="items"
      :total-count="pagination.totalCount"
      :pagination="pagination"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :searchable="true"
      :pull-to-refresh="true"
      state-key="recipient_list"
      @pagination-click="pagination.goToPage"
      @pull-refresh="reload"
      @search="onSearchList"
    >
      <VcColumn
        id="memberId"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_ID')"
        :sortable="true"
        width="21em"
        :visible="false"
        field="memberId"
      />
      <VcColumn
        id="memberName"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_NAME')"
        :sortable="true"
        field="memberName"
      />
      <VcColumn
        id="userId"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_ID')"
        :sortable="true"
        width="21em"
        :visible="false"
        field="userId"
      />
      <VcColumn
        id="userName"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_NAME')"
        :sortable="true"
        field="userName"
      />
      <VcColumn
        id="isRead"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_READ')"
        :sortable="true"
        width="6em"
        field="isRead"
      />
      <VcColumn
        id="isHidden"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_HIDDEN')"
        :sortable="true"
        width="6em"
        :visible="false"
        field="isHidden"
      />
      <VcColumn
        id="createdDate"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.CREATED_DATE')"
        type="datetime"
        :sortable="true"
        :visible="false"
        field="createdDate"
      />
      <VcColumn
        id="modifiedDate"
        :title="t('PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MODIFIED_DATE')"
        type="datetime"
        :sortable="true"
        :visible="false"
        field="modifiedDate"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useDataTableSort, useBlade } from "@vc-shell/framework";
import { useRecipientList } from "../composables/useRecipientList";
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { PushMessageRecipient } from "../../../api_client/virtocommerce.pushmessages";
import { debounce } from "lodash-es";

import { VcBlade, VcDataTable, VcColumn } from "@vc-shell/framework/ui";

const { options, exposeToChildren } = useBlade<{ messageId: string }>();
defineBlade({
  name: "PushMessageRecipientList",
  url: "/recipients",
  routable: false,
});

const { t } = useI18n({ useScope: "global" });

const { sortField, sortOrder, sortExpression } = useDataTableSort({
  initialDirection: "ASC",
  initialField: "MemberName;UserName",
});

const { items, pagination, loadRecipients, loading, searchQuery } = useRecipientList({
  messageId: options.value?.messageId ?? "",
  pageSize: 20,
});

const title = computed(() => t("PUSH_MESSAGES.PAGES.RECIPIENTS.TITLE"));

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
    skip: pagination.skip,
    sort: sortExpression.value,
  });
};

const onSearchList = debounce(async (keyword: string | undefined) => {
  console.debug(`Recipients list search by ${keyword}`);
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

exposeToChildren({
  reload,
});
</script>
