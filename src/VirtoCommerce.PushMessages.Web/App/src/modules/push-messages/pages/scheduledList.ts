import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/scheduled",
    id: "PushMessageScheduledList",
    titleTemplate: "PUSH_MESSAGES.PAGES.LIST.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    isWorkspace: false,
    composable: "useScheduledList",
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
      title: "PUSH_MESSAGES.MENU.SCHEDULED",
      icon: "fas fa-clock",
      priority: 3,
    },
  },
  content: [
    {
      id: "scheduledList",
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
          id: "startDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.START_DATE",
          sortable: true,
          type: "date-time",
          alwaysVisible: true,
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
        },
      ],
    },
  ],
};