import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/drafts",
    id: "PushMessageDraftList",
    titleTemplate: "PUSH_MESSAGES.PAGES.LIST.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    isWorkspace: true,
    composable: "useDraftList",
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
      title: "PUSH_MESSAGES.MENU.DRAFTS",
      icon: "fas fa-edit",
      priority: 2,
    },
  },
  content: [
    {
      id: "draftsList",
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
          id: "startDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.START_DATE",
          type: "date-time",
          sortable: true,
          visible: false,
        },
        {
          id: "recipientsTotalCount",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_TOTAL_COUNT",
          visible: false,
        },
        {
          id: "recipientsReadCount",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_COUNT",
          visible: false,
        },
        {
          id: "recipientsReadPercent",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_PERCENT",
          visible: false,
        },
      ],
    },
  ],
};
