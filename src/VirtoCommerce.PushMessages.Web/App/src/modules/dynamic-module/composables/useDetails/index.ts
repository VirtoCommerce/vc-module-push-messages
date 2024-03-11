import { computed, ref, Ref } from "vue";
import {
  DetailsBaseBladeScope,
  DynamicBladeForm,
  IBladeToolbar,
  useApiClient,
  useDetailsFactory,
} from "@vc-shell/framework";

import { IPushMessage, PushMessageClient } from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface DynamicItemScope extends DetailsBaseBladeScope {
  toolbarOverrides: {
    refresh: IBladeToolbar;
  };
}

export default (args: {
  props: InstanceType<typeof DynamicBladeForm>["$props"];
  emit: InstanceType<typeof DynamicBladeForm>["$emit"];
  mounted: Ref<boolean>;
}) => {
  const factory = useDetailsFactory<IPushMessage>({
    load: async (payload) => {
      if (payload?.id) {
        return (await getApiClient()).get(payload.id, "WithMembers");
      }
    },
    saveChanges: () => {
      throw new Error("Function not implemented.");
    },
    remove: () => {
      throw new Error("Function not implemented.");
    },
  });

  const { load, saveChanges, remove, loading, item, validationState } = factory();

  const scope = ref<DynamicItemScope>({
    toolbarOverrides: {
      refresh: {
        async clickHandler() {
          if (args.props.param) {
            await load({ id: args.props.param });
          }
        },
      },
    },
  });

  const bladeTitle = computed(() => {
    return "Push message details";
  });

  return {
    load,
    saveChanges,
    remove,
    loading,
    item,
    validationState,
    bladeTitle,
    scope: computed(() => scope.value),
  };
};
