import { useBaseList, BaseListOptions, IUseBaseList } from "../useBaseList";
import { IPushMessageSearchCriteria } from "../../../../api_client/virtocommerce.pushmessages";

export interface IUseDraftList extends IUseBaseList {
  loadDrafts: (query?: IPushMessageSearchCriteria) => Promise<void>;
  removeDrafts: (query?: { ids: string[] }) => Promise<void>;
}

export function useDraftList(options?: { pageSize?: number; sort?: string }): IUseDraftList {
  const baseListOptions: BaseListOptions = {
    pageSize: options?.pageSize || 20,
    sort: options?.sort,
    statuses: ["Draft"],
    responseGroup: "None",
  };

  const baseList = useBaseList(baseListOptions);

  return {
    ...baseList,
    loadDrafts: baseList.loadMessages,
    removeDrafts: baseList.removeMessages,
  };
}
