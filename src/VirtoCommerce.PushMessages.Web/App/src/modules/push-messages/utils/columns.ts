import { ITableColumns } from "@vc-shell/framework";
import { useI18n } from "vue-i18n";

export interface ColumnConfig {
  showTrackNewRecipients?: boolean;
  showStartDate?: boolean;
  showReadStats?: boolean;
  hiddenColumns?: string[];
}

export function useMessageListColumns(config: ColumnConfig = {}): ITableColumns[] {
  const { t } = useI18n({ useScope: "global" });

  const columns: ITableColumns[] = [];

  // Track New Recipients column
  if (config.showTrackNewRecipients) {
    columns.push({
      id: "trackNewRecipients",
      title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.TRACK_NEW_RECIPIENTS"),
      sortable: true,
    });
  }

  // Status column (always visible)
  columns.push({
    id: "status",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.STATUS"),
    sortable: true,
    alwaysVisible: true,
  });

  // Topic column
  columns.push({
    id: "topic",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.TOPIC"),
    sortable: true,
  });

  // Short Message column (always visible)
  columns.push({
    id: "shortMessage",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.SHORT_MESSAGE"),
    type: "html",
    alwaysVisible: true,
  });

  // Start Date column
  columns.push({
    id: "startDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.START_DATE"),
    type: "date-time",
    sortable: true,
    visible: config.showStartDate ?? false,
  });

  // Created Date column
  columns.push({
    id: "createdDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.CREATED_DATE"),
    type: "date-time",
    sortable: true,
    visible: false,
  });

  // Modified Date column (always visible)
  columns.push({
    id: "modifiedDate",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.MODIFIED_DATE"),
    type: "date-time",
    sortable: true,
    alwaysVisible: true,
  });

  // Recipients stats columns
  columns.push({
    id: "recipientsTotalCount",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_TOTAL_COUNT"),
    visible: config.showReadStats ?? false,
  });

  columns.push({
    id: "recipientsReadCount",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_COUNT"),
    visible: false,
  });

  columns.push({
    id: "recipientsReadPercent",
    title: t("PUSH_MESSAGES.PAGES.LIST.TABLE.HEADER.RECIPIENTS_READ_PERCENT"),
    visible: config.showReadStats ?? false,
  });

  // Filter out hidden columns
  return columns.filter(col => !config.hiddenColumns?.includes(col.id));
}
