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
      {
        id: "delete",
        icon: "fas fa-trash",
        title: "PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.DELETE",
        method: "remove",
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
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.MESSAGE.LABEL",
          property: "shortMessage",
          rules: { required: true },
          disabled: { method: "isReadOnly" },
        },
        {
          id: "memberIds",
          component: "vc-select",
          searchable: true,
          emitValue: true,
          multiple: true,
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.RECIPIENTS.LABEL",
          property: "memberIds",
          placeholder: "PUSH_MESSAGES.PAGES.DETAILS.FORM.RECIPIENTS.PLACEHOLDER",
          optionValue: "id",
          optionLabel: "name",
          optionsMethod: "loadMembers",
          rules: { required: true },
          disabled: { method: "isReadOnly" },
        },
        {
          id: "topic",
          component: "vc-input",
          variant: "text",
          property: "topic",
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.TOPIC.LABEL",
          rules: { max: 128 },
          disabled: { method: "isReadOnly" },
        },
        {
          id: "startDate",
          component: "vc-input",
          variant: "datetime-local",
          property: "startDate",
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.START_DATE.LABEL",
          rules: {},
          disabled: { method: "isReadOnly" },
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
