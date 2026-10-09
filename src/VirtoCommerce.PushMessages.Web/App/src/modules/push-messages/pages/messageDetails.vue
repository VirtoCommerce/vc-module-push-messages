<template>
  <VcBlade
    :loading="loading"
    :title="bladeTitle"
    width="70%"
    :toolbar-items="toolbarItems"
  >
    <VcHint v-if="missing" class="tw-p-6">
      {{ $t("PUSH_MESSAGES.PAGES.DETAILS.MISSING") }}
    </VcHint>

    <VcForm v-else>
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

        <AudienceSummaryCard
          :total="cardTotal"
          :failed="estimate.failed"
          :loading="estimate.loading"
          :source-line="sourceLine"
          :readonly="isReadOnly"
          :disabled="missing || recipientsOpen"
          @open="openRecipients"
        />

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
            :placeholder="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TOPIC.PLACEHOLDER')"
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
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useBlade, useBladeForm, IBladeToolbar, usePopup } from "@vc-shell/framework";
import { useAudiencePreview } from "../composables/useAudiencePreview";
import { AudienceSummaryCard } from "../components";
import { detectAudienceMode } from "../utils/audienceQuery";
import { conditionCount, formatSourceLine, sourceParts } from "../utils/audienceSummary";
import { AudienceEstimate, SelectRecipientsOptions, SET_AUDIENCE, SetAudiencePayload } from "../utils/audienceSync";
import { useMessageDetails } from "../composables/useMessageDetails";
import { useRecipientsWidgets } from "../widgets/useRecipientsWidgets";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
import { Field } from "vee-validate";

import { VcBlade, VcEditor, VcForm, VcHint, VcInput, VcSwitch } from "@vc-shell/framework/ui";
defineBlade({
  name: "PushMessageDetails",
  url: "/details",
});

const { t } = useI18n({ useScope: "global" });
const { param, options, callParent, closeSelf, openBlade, exposeToChildren } = useBlade<{ sourceMessage?: PushMessage }>();
const { showConfirmation } = usePopup();

// Initialize composable
const { item, loading, loadMessage, saveMessage, deleteMessage } = useMessageDetails({
  id: param.value,
  sourceMessage: options.value?.sourceMessage,
});

/** The audience rows are outside vee-validate, so the form asks the builder directly. */
const audienceInvalid = ref(false);

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

/** A link can outlive the message it points at; there is nothing to edit then. */
const missing = computed(() => !!param.value && !loading.value && !item.value?.id);

const { preview, failed: previewFailed, loading: previewLoading, refresh } = useAudiencePreview();

/** Set by the recipients blade while it is open; otherwise this blade's own estimate stands. */
const childEstimate = ref<AudienceEstimate>();

const estimate = computed<AudienceEstimate>(
  () => childEstimate.value ?? { result: preview.value, failed: previewFailed.value, loading: previewLoading.value },
);

/** While the recipients blade is open, saving would leave its Cancel snapshot behind. */
const recipientsOpen = ref(false);

const sourceLine = computed(() =>
  formatSourceLine(
    sourceParts(
      detectAudienceMode(item.value?.memberQuery, item.value?.memberIds),
      conditionCount(item.value?.memberQuery),
      estimate.value.result,
    ),
    (key, n) => (n === undefined ? t(key) : t(key, n)),
  ),
);

/** A sent message shows whom it went to; anything else shows the live estimate. */
const cardTotal = computed(() =>
  isReadOnly.value ? (item.value?.recipientsTotalCount ?? 0) : estimate.value.result?.totalCount,
);

async function loadEstimate() {
  childEstimate.value = undefined;
  await refresh({ memberQuery: item.value?.memberQuery, memberIds: item.value?.memberIds });
  audienceInvalid.value = previewFailed.value;
}

function openRecipients() {
  recipientsOpen.value = true;

  openBlade({
    name: "PushMessageSelectRecipients",
    options: {
      audience: {
        memberQuery: item.value.memberQuery,
        memberIds: item.value.memberIds ? [...item.value.memberIds] : undefined,
      },
      invalid: audienceInvalid.value,
      estimate: estimate.value,
      readonly: isReadOnly.value,
      sentCount: item.value.recipientsTotalCount,
    } satisfies SelectRecipientsOptions,
    onClose: () => {
      recipientsOpen.value = false;
    },
  });
}

exposeToChildren({
  [SET_AUDIENCE]: (payload: SetAudiencePayload) => {
    item.value.memberQuery = payload.audience.memberQuery;
    item.value.memberIds = payload.audience.memberIds;
    audienceInvalid.value = payload.invalid;
    childEstimate.value = payload.estimate;
  },
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
    disabled: !canSave.value || audienceInvalid.value || recipientsOpen.value,
    clickHandler: async () => {
      await handleSave();
    },
  },
  {
    id: "saveAndPublish",
    icon: "lucide-send",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE_AND_PUBLISH"),
    disabled:
      recipientsOpen.value ||
      !formMeta.value.valid ||
      audienceInvalid.value ||
      item.value == null ||
      (!item.value.memberQuery && (!item.value.memberIds || item.value.memberIds.length == 0)),
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
      await loadEstimate();
      setBaseline();
      refreshAll();
    }
  },
);

// Lifecycle
onMounted(async () => {
  await loadMessage();
  await loadEstimate();
  setBaseline();
  refreshAll();
});
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
