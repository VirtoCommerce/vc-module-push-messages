import { useBaseList, BaseListOptions, IUseBaseList } from "../useBaseList";

export interface IUseTrackingList extends IUseBaseList {}

export function useTrackingList(options?: { pageSize?: number; sort?: string }): IUseTrackingList {
  const baseListOptions: BaseListOptions = {
    pageSize: options?.pageSize || 20,
    sort: options?.sort || "modifiedDate:desc",
    trackNewRecipients: true,
    isDraft: false,
    responseGroup: "WithReadRate",
  };

  return useBaseList(baseListOptions);
}
