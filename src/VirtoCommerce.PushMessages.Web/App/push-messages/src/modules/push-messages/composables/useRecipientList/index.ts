import { ListComposableArgs, useApiClient, useListFactory } from "@vc-shell/framework";

import {
  PushMessageClient,
  PushMessageRecipient,
  PushMessageRecipientSearchCriteria,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export default (args: ListComposableArgs<{ options: { messageId: string } }>) => {
  const listFactory = useListFactory<PushMessageRecipient[], PushMessageRecipientSearchCriteria>({
    load: async (_query) => {
      const criteria = { ...(_query || {}) } as PushMessageRecipientSearchCriteria;
      criteria.messageId = args.props.options.messageId;
      criteria.withHidden = true;
      return (await getApiClient()).searchRecipients(criteria);
    },
  });

  const { load, remove, items, pagination, loading, query } = listFactory({
    sort: "MemberName;UserName",
    pageSize: 20,
  });

  return {
    items,
    load,
    remove,
    loading,
    pagination,
    query,
  };
};
