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
        id: "saveAndPublish",
        icon: "fas fa-paper-plane",
        title: "PUSH_MESSAGES.PAGES.DETAILS.TOOLBAR.SAVE_AND_PUBLISH",
        method: "saveAndPublish",
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
          assetsFolder: "push-messages",
          maxlength: 1024,
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
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_IDS.LABEL",
          placeholder: "PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_IDS.PLACEHOLDER",
          property: "memberIds",
          optionValue: "id",
          optionLabel: "name",
          optionsMethod: "loadMembers",
          disabled: { method: "isReadOnly" },
          visibility: { method: "showMemberIds" },
        },
        {
          id: "memberQuery",
          component: "vc-input",
          variant: "text",
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_QUERY.LABEL",
          placeholder: "PUSH_MESSAGES.PAGES.DETAILS.FORM.MEMBER_QUERY.PLACEHOLDER",
          property: "memberQuery",
          rules: { max: 1024 },
          disabled: { method: "isReadOnly" },
          visibility: { method: "showMemberQuery" },
          append: {
            id: "1",
            component: "vc-button",
            icon: "fas fa-calculator",
            content: "PUSH_MESSAGES.PAGES.DETAILS.FORM.COUNT.LABEL",
            method: "countMembers",
          },
          appendInner: { id: "2", component: "vc-field", variant: "text", property: "memberCount" },
        },
        {
          id: "trackNewRecipients",
          component: "vc-switch",
          property: "trackNewRecipients",
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.TRACK_NEW_RECIPIENTS.LABEL",
          tooltip: "PUSH_MESSAGES.PAGES.DETAILS.FORM.TRACK_NEW_RECIPIENTS.DESCRIPTION",
        },
        {
          id: "topic",
          component: "vc-input",
          variant: "text",
          property: "topic",
          rules: { max: 128 },
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.TOPIC.LABEL",
          disabled: { method: "isReadOnly" },
        },
        {
          id: "startDate",
          component: "vc-input",
          variant: "datetime-local",
          property: "startDate",
          label: "PUSH_MESSAGES.PAGES.DETAILS.FORM.START_DATE.LABEL",
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
