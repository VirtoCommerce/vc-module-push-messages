import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/sent",
    id: "PushMessageSentList",
    titleTemplate: "PUSH_MESSAGES.PAGES.LIST.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    isWorkspace: true,
    composable: "useSentList",
    component: "DynamicBladeList",
    //permissions: "PushMessages:read",
    toolbar: [
      {
        id: "refresh",
        icon: "fas fa-sync-alt",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.REFRESH",
        method: "refresh",
      },
      {
        id: "add",
        icon: "fas fa-plus",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.NEW",
        method: "openAddBlade",
      },
      {
        id: "deleteSelected",
        icon: "fas fa-trash",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.DELETE",
        method: "removeItems",
      },
    ],
    menuItem: {
      title: "PUSH_MESSAGES.MENU.SENT",
      icon: "fas fa-paper-plane",
      priority: 5,
    },
  },
  content: [
    {
      id: "sentList",
      component: "vc-table",
      multiselect: true,
      actions: [
        {
          id: "deleteAction",
          icon: "fas fa-trash",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.ACTIONS.DELETE",
          type: "danger",
          position: "left",
          method: "removeItems",
          disabled: { method: "isReadOnly" },
        },
      ],
      columns: [
        {
          id: "trackNewRecipients",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.TRACK_NEW_RECIPIENTS",
          sortable: true,
        },
        {
          id: "status",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.STATUS",
          sortable: true,
          alwaysVisible: true,
        },
        {
          id: "topic",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.TOPIC",
          sortable: true,
        },
        {
          id: "shortMessage",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.SHORT_MESSAGE",
          type: "html",
          alwaysVisible: true,
        },
        {
          id: "startDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.START_DATE",
          type: "date-time",
          sortable: true,
          visible: false,
        },
        {
          id: "createdDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.CREATED_DATE",
          type: "date-time",
          sortable: true,
          visible: false,
        },
        {
          id: "modifiedDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.MODIFIED_DATE",
          type: "date-time",
          sortable: true,
          alwaysVisible: true,
        },
        {
          id: "recipientsTotalCount",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_TOTAL_COUNT",
        },
        {
          id: "recipientsReadCount",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_COUNT",
          visible: false,
        },
        {
          id: "recipientsReadPercent",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_PERCENT",
        },
      ],
    },
  ],
};
