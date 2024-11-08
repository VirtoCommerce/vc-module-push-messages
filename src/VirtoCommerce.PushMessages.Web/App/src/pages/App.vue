<template>
  <VcApp
    :is-ready="isReady"
    :logo="logoImage"
    :title="$t('PUSH_MESSAGES.TITLE')"
    :version="version"
  >
  </VcApp>
</template>

<script lang="ts" setup>
import { useUser } from "@vc-shell/framework";
import { onMounted, ref } from "vue";
// eslint-disable-next-line import/no-unresolved
import logoImage from "/img/icons/safari-pinned-tab.svg";

const isReady = ref(false);
const version = import.meta.env.PACKAGE_VERSION;

const { isAuthenticated } = useUser();

onMounted(async () => {
  if (isAuthenticated.value) {
    isReady.value = true;
  }
});

console.debug(`Initializing App`);
</script>
<style lang="scss">@import "./../styles/index.scss";</style>
