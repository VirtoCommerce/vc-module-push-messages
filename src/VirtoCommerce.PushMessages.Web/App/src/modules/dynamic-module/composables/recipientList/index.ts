import { Ref } from "vue";
import { DynamicBladeList, useApiClient, useListFactory } from "@vc-shell/framework";

import {
  PushMessageClient,
  PushMessageRecipient,
  PushMessageRecipientSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export default (args: {
  props: InstanceType<typeof DynamicBladeList>["$props"] & { options: { messageId: string } };
  emit: InstanceType<typeof DynamicBladeList>["$emit"];
  mounted: Ref<boolean>;
}) => {
  const factory = useListFactory<PushMessageRecipient[], PushMessageRecipientSearchCriteria>({
    load: async () => {
      const criteria = new PushMessageRecipientSearchCriteria();
      criteria.messageId = args.props.options.messageId;
      criteria.take = 20;
      return (await getApiClient()).searchRecipients(criteria);
    },
  });

  const { load, remove, items, pagination, loading, query } = factory();

  return {
    items,
    load,
    remove,
    loading,
    pagination,
    query,
  };
};
