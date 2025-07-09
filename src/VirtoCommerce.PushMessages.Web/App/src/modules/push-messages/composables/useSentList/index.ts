import { useBaseList, BaseListOptions, IUseBaseList } from "../useBaseList";

export interface IUseSentList extends IUseBaseList {}

export function useSentList(options?: { pageSize?: number; sort?: string }): IUseSentList {
  const baseListOptions: BaseListOptions = {
    pageSize: options?.pageSize || 20,
    sort: options?.sort || "modifiedDate:desc",
    statuses: ["Sent"],
    responseGroup: "WithReadRate",
  };

  return useBaseList(baseListOptions);
}
