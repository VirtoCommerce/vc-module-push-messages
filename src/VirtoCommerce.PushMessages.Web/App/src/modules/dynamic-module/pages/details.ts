import { DynamicDetailsSchema } from "@vc-shell/framework";

export const details: DynamicDetailsSchema = {
  settings: {
    url: "/dynamic-module-details",
    id: "PushMessageDetails",
    localizationPrefix: "PUSH_MESSAGES",
    composable: "useDetails",
    component: "DynamicBladeForm",
    toolbar: [
      {
        id: "save",
        icon: "fas fa-save",
        title: "PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE",
        method: "saveChanges",
      },
      {
        id: "refresh",
        icon: "fas fa-sync-alt",
        title: "Refresh",
        method: "refresh",
      },
    ],
  },
  content: [
    {
      id: "pushMessageForm",
      component: "vc-form",
      children: [
        {
          id: "shortMessage",
          component: "vc-input",
          label: "Short message",
          property: "shortMessage",
        },
        {
          id: "memberId",
          component: "vc-select",
          searchable: true,
          emitValue: true,
          label: "Recipients",
          property: "memberId",
          placeholder: "Select recipients",
          optionValue: "id",
          optionLabel: "name",
          optionsMethod: "loadMembers",
        },
      ],
    },
    {
      id: "recipientsWidgets",
      component: "vc-widgets",
      children: ["RecipientsWidget"],
    },
  ],
};
