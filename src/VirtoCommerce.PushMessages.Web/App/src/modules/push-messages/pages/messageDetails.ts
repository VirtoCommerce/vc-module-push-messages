import { DynamicDetailsSchema } from "@vc-shell/framework";

export const details: DynamicDetailsSchema = {
  settings: {
    url: "/details",
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
    ],
  },
  content: [
    {
      id: "pushMessageForm",
      component: "vc-form",
      children: [
        {
          id: "shortMessage",
          component: "vc-editor",
          label: "Message",
          property: "shortMessage",
          rules: { required: true },
        },
        {
          id: "memberIds",
          component: "vc-select",
          searchable: true,
          emitValue: true,
          multiple: true,
          label: "Recipients",
          property: "memberIds",
          placeholder: "Select recipients",
          optionValue: "id",
          optionLabel: "name",
          optionsMethod: "loadMembers",
          rules: { required: true },
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
