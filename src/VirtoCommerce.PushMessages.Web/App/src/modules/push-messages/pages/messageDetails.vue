<template>
  <VcBlade
    :title="bladeTitle"
    width="50%"
    :expanded="expanded"
    :closable="closable"
    :toolbar-items="toolbarItems"
    v-loading="loading"
    :modified="isModified"
    @close="$emit('close:blade')"
    @expand="$emit('expand:blade')"
    @collapse="$emit('collapse:blade')"
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
        <!-- @vue-generic {Member, MemberSearchResult}-->
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
                icon="material-calculate"
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
          :tooltip="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.TRACK_NEW_RECIPIENTS.DESCRIPTION')"
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
import { computed, inject, onBeforeUnmount, onMounted, ref, toRef, toRefs, watch } from "vue";
import { useI18n } from "vue-i18n";
import {
  VcBlade,
  VcForm,
  VcField,
  VcEditor,
  VcSelect,
  VcInput,
  VcSwitch,
  VcButton,
  useBladeNavigation,
  useWidgets,
  BladeInstance,
  IBladeToolbar,
  usePopup,
  IParentCallArgs,
} from "@vc-shell/framework";
import { useMessageDetails } from "../composables/useMessageDetails";
import { PushMessage } from "../../../api_client/virtocommerce.pushmessages";
import { Member, MemberSearchResult } from "../../../api_client/virtocommerce.customer";
import { Field, useForm } from "vee-validate";
import RecipientsWidget from "../components/widgets/recipients-widget.vue";

export interface Props {
  expanded?: boolean;
  closable?: boolean;
  param?: string;
  options?: {
    sourceMessage?: PushMessage;
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
  name: "PushMessageDetails",
  url: "/details",
});

const { t } = useI18n({ useScope: "global" });
const { onBeforeClose } = useBladeNavigation();
const { meta } = useForm({ validateOnMount: false });
const { registerWidget, clearBladeWidgets, updateActiveWidget } = useWidgets();
const { showConfirmation } = usePopup();
const blade = inject(BladeInstance);

// Initialize composable
const {
  item,
  loading,
  showMemberIds,
  showMemberQuery,
  isModified,
  memberCount,
  loadMessage,
  saveMessage,
  deleteMessage,
  loadMembers,
  countMembers,
  countingMembers,
} = useMessageDetails({
  id: props.param,
  sourceMessage: props.options?.sourceMessage,
});

// Local state
const itemId = computed(() => item.value?.id);
const itemStatus = computed(() => item.value?.status);

const isReadOnly = computed(() => {
  return !!props.param && item.value?.status === "Sent";
});
const isEditable = computed(() => {
  return !props.param || (item.value != null && item.value.status !== "Sent");
});

const bladeTitle = computed(() => {
  return !props.param ? "New push message" : "Push message details";
});

// Toolbar items
const toolbarItems = computed((): IBladeToolbar[] => [
  {
    id: "save",
    icon: "material-save",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE"),
    disabled: !isModified.value || !meta.value.valid,
    clickHandler: async () => {
      await handleSave();
    },
  },
  {
    id: "saveAndPublish",
    icon: "material-send",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE_AND_PUBLISH"),
    disabled:
      !meta.value.valid ||
      item.value == null ||
      (!item.value.memberQuery && (!item.value.memberIds || item.value.memberIds.length == 0)),
    isVisible: isEditable.value && item.value != null && item.value.status !== "Scheduled",
    clickHandler: async () => {
      const status = item.value?.startDate ? "Scheduled" : "Sent";
      await handleSave(status);

      updateActiveWidget();
    },
  },
  {
    id: "clone",
    icon: "material-content_copy",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.CLONE"),
    isVisible: !!props.param,
    clickHandler: () => {
      emit("parent:call", {
        method: "onAddNewDraft",
        args: {
          options: {
            sourceMessage: item,
          },
        },
      });
    },
  },
  {
    id: "delete",
    icon: "material-delete",
    title: t("PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.DELETE"),
    isVisible: !!props.param && isEditable.value,
    clickHandler: async () => {
      if (await showConfirmation(t("PUSH_MESSAGES.PAGES.ALERTS.DELETE"))) {
        await deleteMessage();

        emit("parent:call", {
          method: "reload",
        });

        updateActiveWidget();

        emit("close:blade");
      }
    },
  },
]);

// Methods
async function handleSave(status?: string) {
  const message = await saveMessage(status);

  emit("parent:call", { method: "reload" });

  if (item.value.id || message.id) {
    emit("parent:call", { method: "onItemClick", args: message.id ? message : item.value });
  }
}

// Watchers
watch(
  () => props.param,
  (newParam) => {
    if (newParam) {
      loadMessage();
    }
  },
  { immediate: true },
);

// Lifecycle
onMounted(async () => {
  await loadMessage();
});

onBeforeUnmount(() => {
  if (blade?.value.id) {
    clearBladeWidgets(blade.value.id);
  }
});

onBeforeClose(async () => {
  if (isModified.value) {
    return showConfirmation(t("PUSH_MESSAGES.PAGES.ALERTS.CLOSE_CONFIRMATION"));
  }
});

registerWidget(
  {
    id: "RecipientsWidget",
    component: RecipientsWidget,
    isVisible: computed(() => item.value?.status === "Sent"),
    props: {
      itemId: itemId,
      status: itemStatus,
    },
    updateFunctionName: "updateActiveWidgetCount",
  },
  blade?.value.id ?? "",
);

defineExpose({
  title: bladeTitle,
});
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
