import { DynamicGridSchema } from "@vc-shell/framework";

export const grid: DynamicGridSchema = {
  settings: {
    url: "/dynamic-module-list",
    id: "DynamicItems",
    titleTemplate: "Dynamic module blade",
    localizationPrefix: "DynamicModule",
    isWorkspace: true,
    composable: "useList",
    component: "DynamicBladeList",
    toolbar: [
      {
        id: "refresh",
        icon: "fas fa-sync-alt",
        title: "Refresh",
        method: "refresh",
      },
    ],
    menuItem: {
      title: "DYNAMICMODULE.MENU.TITLE",
      icon: "fas fa-file-alt",
      priority: 1,
    },
  },
  content: [
    {
      id: "itemsGrid",
      component: "vc-table",
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
      mobileTemplate: {
        component: "DynamicItemsMobileGridView",
      },
      multiselect: true,
      columns: [
        {
          id: "shortMessage",
          title: "Short message",
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
