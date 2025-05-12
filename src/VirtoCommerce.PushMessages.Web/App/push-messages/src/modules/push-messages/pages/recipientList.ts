import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/recipients",
    id: "PushMessageRecipientList",
    titleTemplate: "PUSH_MESSAGES.PAGES.RECIPIENTS.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    composable: "useRecipientList",
    component: "DynamicBladeList",
    routable: false,
  },
  content: [
    {
      id: "recipientsGrid",
      component: "vc-table",
      columns: [
        {
          id: "memberId",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_ID",
          sortable: true,
          width: "21em",
          visible: false,
        },
        {
          id: "memberName",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MEMBER_NAME",
          sortable: true,
        },
        {
          id: "userId",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_ID",
          sortable: true,
          width: "21em",
          visible: false,
        },
        {
          id: "userName",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.USER_NAME",
          sortable: true,
        },
        {
          id: "isRead",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_READ",
          sortable: true,
          width: "6em",
        },
        {
          id: "isHidden",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.IS_HIDDEN",
          sortable: true,
          width: "6em",
          visible: false,
        },
        {
          id: "createdDate",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.CREATED_DATE",
          type: "date-time",
          sortable: true,
          visible: false,
        },
        {
          id: "modifiedDate",
          title: "PUSH_MESSAGES.PAGES.RECIPIENTS.TABLE.HEADER.MODIFIED_DATE",
          type: "date-time",
          sortable: true,
          visible: false,
        },
      ],
    },
  ],
};
