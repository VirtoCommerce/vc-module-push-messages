import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/recipients",
    id: "PushMessageRecipientList",
    titleTemplate: "PUSH_MESSAGES.PAGES.RECIPIENTS.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    composable: "useRecipientList",
    component: "DynamicBladeList",
  },
  content: [
    {
      id: "recipientsGrid",
      component: "vc-table",
      columns: [
        {
          id: "userId",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.ID",
          sortable: true,
        },
        {
          id: "userName",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.NAME",
          sortable: true,
        },
        {
          id: "isRead",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.READ",
          sortable: true,
        },
      ],
    },
  ],
};
