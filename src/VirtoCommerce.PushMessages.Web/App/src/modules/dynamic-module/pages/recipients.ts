import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/push-message-recipients",
    id: "PushMessageRecipientList",
    titleTemplate: "PUSH_MESSAGES.PAGES.RECIPIENTS.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    composable: "recipientList",
    component: "DynamicBladeList",
  },
  content: [
    {
      id: "recipientsGrid",
      component: "vc-table",
      columns: [
        {
          id: "userId",
          title: "Id",
          sortable: true,
        },
        {
          id: "isRead",
          title: "Read",
          sortable: true,
        },
      ],
    },
  ],
};
