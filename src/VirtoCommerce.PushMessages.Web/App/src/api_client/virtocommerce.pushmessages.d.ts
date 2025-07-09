export declare class AuthApiBase {
    authToken: string;
    protected constructor();
    getBaseUrl(defaultUrl: string, baseUrl: string): string;
    setAuthToken(token: string): void;
    protected transformOptions(options: any): Promise<any>;
}
export declare class PushMessageClient extends AuthApiBase {
    private http;
    private baseUrl;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined;
    constructor(baseUrl?: string, http?: {
        fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
    });
    /**
     * @param body (optional)
     * @return Success
     */
    searchRecipients(body?: PushMessageRecipientSearchCriteria | undefined): Promise<PushMessageRecipientSearchResult>;
    protected processSearchRecipients(response: Response): Promise<PushMessageRecipientSearchResult>;
    /**
     * @param body (optional)
     * @return Success
     */
    search(body?: PushMessageSearchCriteria | undefined): Promise<PushMessageSearchResult>;
    protected processSearch(response: Response): Promise<PushMessageSearchResult>;
    /**
     * @param body (optional)
     * @return Success
     */
    create(body?: PushMessage | undefined): Promise<PushMessage>;
    protected processCreate(response: Response): Promise<PushMessage>;
    /**
     * @param body (optional)
     * @return Success
     */
    update(body?: PushMessage | undefined): Promise<PushMessage>;
    protected processUpdate(response: Response): Promise<PushMessage>;
    /**
     * @param ids (optional)
     * @return No Content
     */
    delete(ids?: string[] | undefined): Promise<void>;
    protected processDelete(response: Response): Promise<void>;
    /**
     * @return Success
     */
    changeTracking(id: string, value: boolean): Promise<PushMessage>;
    protected processChangeTracking(response: Response): Promise<PushMessage>;
    /**
     * @param responseGroup (optional)
     * @return Success
     */
    get(id: string, responseGroup?: string | undefined): Promise<PushMessage>;
    protected processGet(response: Response): Promise<PushMessage>;
}
export declare class PushMessage implements IPushMessage {
    topic?: string | undefined;
    shortMessage?: string | undefined;
    startDate?: Date | undefined;
    status?: string | undefined;
    trackNewRecipients?: boolean;
    memberQuery?: string | undefined;
    memberIds?: string[] | undefined;
    recipientsTotalCount?: number;
    recipientsReadCount?: number;
    recipientsReadPercent?: number;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IPushMessage);
    init(_data?: any): void;
    static fromJS(data: any): PushMessage;
    toJSON(data?: any): any;
}
export interface IPushMessage {
    topic?: string | undefined;
    shortMessage?: string | undefined;
    startDate?: Date | undefined;
    status?: string | undefined;
    trackNewRecipients?: boolean;
    memberQuery?: string | undefined;
    memberIds?: string[] | undefined;
    recipientsTotalCount?: number;
    recipientsReadCount?: number;
    recipientsReadPercent?: number;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class PushMessageRecipient implements IPushMessageRecipient {
    messageId?: string | undefined;
    memberId?: string | undefined;
    memberName?: string | undefined;
    userId?: string | undefined;
    userName?: string | undefined;
    isRead?: boolean;
    isHidden?: boolean;
    message?: PushMessage | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IPushMessageRecipient);
    init(_data?: any): void;
    static fromJS(data: any): PushMessageRecipient;
    toJSON(data?: any): any;
}
export interface IPushMessageRecipient {
    messageId?: string | undefined;
    memberId?: string | undefined;
    memberName?: string | undefined;
    userId?: string | undefined;
    userName?: string | undefined;
    isRead?: boolean;
    isHidden?: boolean;
    message?: PushMessage | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class PushMessageRecipientSearchCriteria implements IPushMessageRecipientSearchCriteria {
    messageId?: string | undefined;
    userId?: string | undefined;
    isRead?: boolean | undefined;
    withHidden?: boolean;
    responseGroup?: string | undefined;
    /** Search object type */
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    /** Search phrase */
    keyword?: string | undefined;
    /** Property is left for backward compatibility */
    searchPhrase?: string | undefined;
    /** Search phrase language */
    languageCode?: string | undefined;
    sort?: string | undefined;
    readonly sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
    constructor(data?: IPushMessageRecipientSearchCriteria);
    init(_data?: any): void;
    static fromJS(data: any): PushMessageRecipientSearchCriteria;
    toJSON(data?: any): any;
}
export interface IPushMessageRecipientSearchCriteria {
    messageId?: string | undefined;
    userId?: string | undefined;
    isRead?: boolean | undefined;
    withHidden?: boolean;
    responseGroup?: string | undefined;
    /** Search object type */
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    /** Search phrase */
    keyword?: string | undefined;
    /** Property is left for backward compatibility */
    searchPhrase?: string | undefined;
    /** Search phrase language */
    languageCode?: string | undefined;
    sort?: string | undefined;
    sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
}
export declare class PushMessageRecipientSearchResult implements IPushMessageRecipientSearchResult {
    totalCount?: number;
    results?: PushMessageRecipient[] | undefined;
    constructor(data?: IPushMessageRecipientSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): PushMessageRecipientSearchResult;
    toJSON(data?: any): any;
}
export interface IPushMessageRecipientSearchResult {
    totalCount?: number;
    results?: PushMessageRecipient[] | undefined;
}
export declare class PushMessageSearchCriteria implements IPushMessageSearchCriteria {
    isDraft?: boolean | undefined;
    trackNewRecipients?: boolean | undefined;
    createdDateBefore?: Date | undefined;
    startDateBefore?: Date | undefined;
    statuses?: string[] | undefined;
    responseGroup?: string | undefined;
    /** Search object type */
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    /** Search phrase */
    keyword?: string | undefined;
    /** Property is left for backward compatibility */
    searchPhrase?: string | undefined;
    /** Search phrase language */
    languageCode?: string | undefined;
    sort?: string | undefined;
    readonly sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
    constructor(data?: IPushMessageSearchCriteria);
    init(_data?: any): void;
    static fromJS(data: any): PushMessageSearchCriteria;
    toJSON(data?: any): any;
}
export interface IPushMessageSearchCriteria {
    isDraft?: boolean | undefined;
    trackNewRecipients?: boolean | undefined;
    createdDateBefore?: Date | undefined;
    startDateBefore?: Date | undefined;
    statuses?: string[] | undefined;
    responseGroup?: string | undefined;
    /** Search object type */
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    /** Search phrase */
    keyword?: string | undefined;
    /** Property is left for backward compatibility */
    searchPhrase?: string | undefined;
    /** Search phrase language */
    languageCode?: string | undefined;
    sort?: string | undefined;
    sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
}
export declare class PushMessageSearchResult implements IPushMessageSearchResult {
    totalCount?: number;
    results?: PushMessage[] | undefined;
    constructor(data?: IPushMessageSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): PushMessageSearchResult;
    toJSON(data?: any): any;
}
export interface IPushMessageSearchResult {
    totalCount?: number;
    results?: PushMessage[] | undefined;
}
export declare enum SortDirection {
    Ascending = "Ascending",
    Descending = "Descending"
}
export declare class SortInfo implements ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;
    constructor(data?: ISortInfo);
    init(_data?: any): void;
    static fromJS(data: any): SortInfo;
    toJSON(data?: any): any;
}
export interface ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;
}
export declare enum SortInfoSortDirection {
    Ascending = "Ascending",
    Descending = "Descending"
}
export declare class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: {
        [key: string]: any;
    };
    result: any;
    constructor(message: string, status: number, response: string, headers: {
        [key: string]: any;
    }, result: any);
    protected isApiException: boolean;
    static isApiException(obj: any): obj is ApiException;
}
//# sourceMappingURL=virtocommerce.pushmessages.d.ts.map