<template>
  <VcBlade
    :loading="loading"
    :title="bladeTitle"
    width="50%"
    :toolbar-items="toolbarItems"
  >
    <VcForm>
      <div class="tw-p-6 tw-space-y-6">
        <!-- Short Message Field -->
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="shortMessage"
          :model-value="item.shortMessage"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MESSAGE.LABEL')"
          rules="required"
        >
          <VcEditor
            v-model="item.shortMessage"
            :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MESSAGE.LABEL')"
            assets-folder="push-messages"
            :max-length="1024"
            :disabled="isReadOnly"
            required
            :error="errors.length > 0"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>

        <!-- Member Selection - Show either IDs or Query -->
        <!-- @vue-generic {string[], Member, MemberSearchResult}-->
        <VcSelect
          v-if="showMemberIds"
          v-model="item.memberIds"
          emit-value
          searchable
          multiple
          option-value="id"
          option-label="name"
          :options="loadMembers"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_IDS.LABEL')"
          :placeholder="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_IDS.PLACEHOLDER')"
          :disabled="isReadOnly"
        />

        <Field
          v-if="showMemberQuery"
          v-slot="{ errorMessage, handleChange, errors }"
          name="memberQuery"
          :model-value="item.memberQuery"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_QUERY.LABEL')"
          rules="max:1024"
        >
          <VcInput
            v-model="item.memberQuery"
            type="text"
            :placeholder="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_QUERY.PLACEHOLDER')"
            :disabled="isReadOnly"
            :error="errors.length > 0"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          >
            <template #append>
              <VcButton
                icon="lucide-calculator"
                variant="secondary"
                size="sm"
                :loading="countingMembers"
                @click="countMembers"
              >
                {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.COUNT.LABEL") }}
              </VcButton>
            </template>
            <template #append-inner>
              <VcField
                variant="text"
                :model-value="memberCount"
              />
            </template>
          </VcInput>
        </Field>

        <!-- Track New Recipients -->
        <VcSwitch
          v-model="item.trackNewRecipients"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TRACK_NEW_RECIPIENTS.LABEL')"
          :hint="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TRACK_NEW_RECIPIENTS.DESCRIPTION')"
        />

        <!-- Topic -->
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TOPIC.LABEL')"
          name="topic"
          :model-value="item.topic"
          rules="max:128"
        >
          <VcInput
            v-model="item.topic"
            type="text"
            :disabled="isReadOnly"
            :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TOPIC.LABEL')"
            :error="errors.length > 0"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>

        <!-- Start Date -->
        <VcInput
          v-model="item.startDate"
          type="datetime-local"
          :disabled="isReadOnly"
          :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.START_DATE.LABEL')"
        />
      </div>
    </VcForm>
  </VcBlade>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useBlade, useBladeForm, IBladeToolbar, usePopup } from "@vc-shell/framework";
import { useMessageDetails } from "../composables/useMessageDetails";
import { useRecipientsWidgets } from "../widgets/useRecipientsWidgets";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { Member, MemberSearchResult } from "../../../api_client/virtocommerce.customer";
import { Field } from "vee-validate";

import { VcBlade, VcButton, VcEditor, VcField, VcForm, VcInput, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
defineBlade({
  name: "PushMessageDetails",
  url: "/details",
});

const { t } = useI18n({ useScope: "global" });
const { param, options, callParent, closeSelf } = useBlade<{ sourceMessage?: PushMessage }>();
const { showConfirmation } = usePopup();

// Initialize composable
const { item, loading, showMemberIds, showMemberQuery, memberCount, loadMessage, saveMessage, deleteMessage, loadMembers, countMembers, countingMembers } = useMessageDetails({
  id: param.value,
  sourceMessage: options.value?.sourceMessage,
});

const { canSave, setBaseline, formMeta } = useBladeForm({
  data: item,
  closeConfirmMessage: () => t("PUSH_MESSAGES.PAGES.ALERTS.CLOSE_CONFIRMATION"),
});

// Widgets
const { refreshAll } = useRecipientsWidgets({
  itemId: computed(() => item.value?.id),
  isVisible: computed(() => item.value?.status === "Sent"),
});

// Local state
const isReadOnly = computed(() => {
  return !!param.value && item.value?.status === "Sent";
});
const isEditable = computed(() => {
  return !param.value || (item.value != null && item.value.status !== "Sent");
});

const bladeTitle = computed(() => {
  return !param.value ? "New push message" : "Push message details";
});

// Toolbar items
const toolbarItems = computed((): IBladeToolbar[] => [
  {
    id: "save",
    icon: "lucide-save",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE"),
    disabled: !canSave.value,
    clickHandler: async () => {
      await handleSave();
    },
  },
  {
    id: "saveAndPublish",
    icon: "lucide-send",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE_AND_PUBLISH"),
    disabled: !formMeta.value.valid || item.value == null || (!item.value.memberQuery && (!item.value.memberIds || item.value.memberIds.length == 0)),
    isVisible: isEditable.value && item.value != null && item.value.status !== "Scheduled",
    clickHandler: async () => {
      const status = item.value?.startDate ? "Scheduled" : "Sent";
      await handleSave(status);

      refreshAll();
    },
  },
  {
    id: "clone",
    icon: "lucide-copy",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.CLONE"),
    isVisible: !!param.value,
    clickHandler: () => {
      callParent("onAddNewMessage", {
        options: {
          sourceMessage: item,
        },
      });
    },
  },
  {
    id: "delete",
    icon: "lucide-trash-2",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.DELETE"),
    isVisible: !!param.value && isEditable.value,
    clickHandler: async () => {
      if (await showConfirmation(t("PUSH_MESSAGES.PAGES.ALERTS.DELETE"))) {
        await deleteMessage();

        callParent("reload");

        refreshAll();

        closeSelf();
      }
    },
  },
]);

// Methods
async function handleSave(status?: string) {
  const message = await saveMessage(status);

  setBaseline();

  callParent("reload");

  if (item.value.id || message.id) {
    callParent("onItemClick", message.id ? message : item.value);
  }
}

// Watchers
watch(
  () => param.value,
  async (newParam) => {
    if (newParam) {
      await loadMessage();
      setBaseline();
      refreshAll();
    }
  },
);

// Lifecycle
onMounted(async () => {
  await loadMessage();
  setBaseline();
  refreshAll();
});
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
