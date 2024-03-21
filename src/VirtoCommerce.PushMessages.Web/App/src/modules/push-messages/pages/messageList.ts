import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/messages",
    id: "PushMessageList",
    titleTemplate: "PUSH_MESSAGES.PAGES.LIST.TITLE",
    localizationPrefix: "PUSH_MESSAGES",
    isWorkspace: true,
    composable: "useList",
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
    ],
    menuItem: {
      title: "PUSH_MESSAGES.MENU.TITLE",
      icon: "fas fa-message",
      priority: 1,
    },
  },
  content: [
    {
      id: "itemsGrid",
      component: "vc-table",
      columns: [
        {
          id: "createdDate",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.CREATED_DATE",
          sortable: true,
          type: "date-time",
          width: "14em",
          alwaysVisible: true,
        },
        {
          id: "shortMessage",
          title: "PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.MESSAGE",
          alwaysVisible: true,
        },
      ],
    },
  ],
};
