import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/push-messages",
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
        title: "Refresh",
        method: "refresh",
      },
      {
        id: "add",
        icon: "fas fa-plus",
        title: "New",
        method: "openAddBlade",
      },
    ],
    menuItem: {
      title: "PUSH_MESSAGES.MENU.TITLE",
      icon: "fas fa-file-alt",
      priority: 1,
    },
  },
  content: [
    {
      id: "itemsGrid",
      component: "vc-table",
      multiselect: true,
      actions: [
        {
          id: "deleteAction",
          icon: "fas fa-trash",
          title: "Delete",
          type: "danger",
          position: "left",
          method: "deleteItem",
        },
      ],
      columns: [
        {
          id: "shortMessage",
          title: "Short message2",
          alwaysVisible: true,
        },
        {
          id: "createdDate",
          title: "Created date",
          sortable: true,
          type: "date-ago",
        },
      ],
    },
  ],
};
