import { useBaseList, BaseListOptions, IUseBaseList } from "../useBaseList";

export interface IUseScheduledList extends IUseBaseList {}

export function useScheduledList(options?: { pageSize?: number; sort?: string }): IUseScheduledList {
  const baseListOptions: BaseListOptions = {
    pageSize: options?.pageSize || 20,
    sort: options?.sort || "modifiedDate:desc",
    statuses: ["Scheduled"],
    responseGroup: "None",
  };

  return useBaseList(baseListOptions);
}
