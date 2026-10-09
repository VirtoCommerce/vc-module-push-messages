<template>
  <VcBlade :title="title" width="50%" :toolbar-items="toolbarItems" :modified="changed">
    <div v-if="draft" class="tw-p-6">
      <AudienceBuilder
        v-model:member-query="draft.memberQuery"
        v-model:member-ids="draft.memberIds"
        v-model:invalid="invalid"
        :disabled="readonly"
        :sent-count="options?.sentCount"
        @update:estimate="estimate = $event"
      />
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { IBladeToolbar, useBlade, usePopup } from "@vc-shell/framework";
import { VcBlade } from "@vc-shell/framework/ui";
import AudienceBuilder from "../components/AudienceBuilder.vue";
import {
  AudienceDraft,
  AudienceEstimate,
  copyAudience,
  sameAudience,
  SelectRecipientsOptions,
  SET_AUDIENCE,
  SetAudiencePayload,
} from "../utils/audienceSync";

// Not routable: its draft lives in options, which a reload does not bring back.
defineBlade({
  name: "PushMessageSelectRecipients",
  url: "/select-recipients",
  routable: false,
});

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf, onBeforeClose } = useBlade<SelectRecipientsOptions>();
const { showConfirmation } = usePopup();

const readonly = computed(() => !!options.value?.readonly);
const title = computed(() => t(`PUSH_MESSAGES.PAGES.SELECT_RECIPIENTS.${readonly.value ? "TITLE_READONLY" : "TITLE"}`));

/** The author edits a copy; options.audience stays as it came and is what Cancel restores. */
const draft = ref<AudienceDraft | undefined>(options.value ? copyAudience(options.value.audience) : undefined);
const invalid = ref(options.value?.invalid ?? false);
const estimate = ref<AudienceEstimate>(options.value?.estimate ?? { failed: false, loading: false });

const changed = computed(() => !!draft.value && !!options.value && !sameAudience(draft.value, options.value.audience));

/** Set once Apply or Cancel has settled the draft, so the close they trigger asks nothing. */
let settled = false;

// Every change reaches Blade 1 at once, so its card never shows a number this blade does not.
watch(
  [draft, invalid, estimate],
  () => {
    if (!draft.value || readonly.value) {
      return;
    }

    callParent(SET_AUDIENCE, {
      audience: copyAudience(draft.value),
      invalid: invalid.value,
      estimate: estimate.value,
    } satisfies SetAudiencePayload);
  },
  { deep: true },
);

async function restore() {
  if (!options.value) {
    return;
  }

  await callParent(SET_AUDIENCE, {
    audience: copyAudience(options.value.audience),
    invalid: options.value.invalid,
    estimate: options.value.estimate,
  } satisfies SetAudiencePayload);
}

// vc-shell 2.6: true prevents the close, false allows it.
onBeforeClose(async () => {
  if (settled || readonly.value || !changed.value) {
    return false;
  }

  if (await showConfirmation(t("PUSH_MESSAGES.PAGES.SELECT_RECIPIENTS.DISCARD_CONFIRMATION"))) {
    await restore();

    return false;
  }

  return true;
});

const toolbarItems = computed((): IBladeToolbar[] => [
  {
    id: "apply",
    icon: "lucide-check",
    title: t("PUSH_MESSAGES.PAGES.SELECT_RECIPIENTS.TOOLBAR.APPLY"),
    isVisible: !readonly.value,
    clickHandler: async () => {
      settled = true;
      await closeSelf();
    },
  },
  {
    id: "cancel",
    icon: "lucide-undo-2",
    title: t("PUSH_MESSAGES.PAGES.SELECT_RECIPIENTS.TOOLBAR.CANCEL"),
    isVisible: !readonly.value,
    clickHandler: async () => {
      await restore();
      settled = true;
      await closeSelf();
    },
  },
]);

onMounted(() => {
  // Opened without its options (a restored URL): there is no draft to edit.
  if (!options.value) {
    closeSelf();
  }
});
</script>
