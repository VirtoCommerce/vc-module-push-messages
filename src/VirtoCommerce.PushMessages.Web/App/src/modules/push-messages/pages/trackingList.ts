import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/tracking",
    id: "PushMessageTrackingList",
    titleTemplate: "PUSH_MESSAGES.PAGES.LIST.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    isWorkspace: true,
    composable: "useTrackingList",
    component: "DynamicBladeList",
    //permissions: "PushMessages:read",
    toolbar: [
      {
        id: "refresh",
        icon: "material-refresh",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.REFRESH",
        method: "refresh",
      },
      {
        id: "add",
        icon: "material-add",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.NEW",
        method: "openAddBlade",
      },
      {
        id: "deleteSelected",
        icon: "material-delete",
        title: "PUSH_MESSAGES.PAGES.LIST.TOOLBAR.DELETE",
        method: "removeItems",
      },
    ],
    menuItem: {
      title: "PUSH_MESSAGES.MENU.TRACK_NEW_RECIPIENTS",
      icon: "material-person_add",
      priority: 4,
    },
  },
  content: [
    {
      id: "trackingList",
      component: "vc-table",
      multiselect: true,
      actions: [
        {
          id: "deleteAction",
          icon: "material-delete",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.ACTIONS.DELETE",
          type: "danger",
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
