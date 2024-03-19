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
          id: "memberName",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_NAME",
          sortable: true,
        },
        {
          id: "userName",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_NAME",
          sortable: true,
        },
        {
          id: "isRead",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.READ",
          sortable: true,
          width: "6em",
        },
      ],
    },
  ],
};
