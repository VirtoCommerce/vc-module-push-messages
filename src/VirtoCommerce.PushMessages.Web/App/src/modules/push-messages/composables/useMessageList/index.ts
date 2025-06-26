import { useBaseList, BaseListOptions, IUseBaseList } from "../useBaseList";

export interface IUseMessageList extends IUseBaseList {}

export function useMessageList(options?: { pageSize?: number; sort?: string }): IUseMessageList {
  const baseListOptions: BaseListOptions = {
    pageSize: options?.pageSize || 20,
    sort: options?.sort || "modifiedDate:desc",
    responseGroup: "None",
  };

  return useBaseList(baseListOptions);
}
