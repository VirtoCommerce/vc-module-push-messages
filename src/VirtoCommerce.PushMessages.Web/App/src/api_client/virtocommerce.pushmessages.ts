//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming
// @ts-nocheck

export class AuthApiBase {
  authToken = "";
  protected constructor() {}

  // Enforce always return empty string as baseUrl
  getBaseUrl(defaultUrl: string, baseUrl: string) {
    return "";
  }

  setAuthToken(token: string) {
    this.authToken = token;
  }

  protected transformOptions(options: any): Promise<any> {
    if (this.authToken) {
      options.headers["authorization"] = `Bearer ${this.authToken}`;
    }
    return Promise.resolve(options);
  }
}

export class PushMessageClient extends AuthApiBase {
    private http: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> };
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(baseUrl?: string, http?: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> }) {
        super();
        this.http = http ? http : window as any;
        this.baseUrl = this.getBaseUrl("", baseUrl);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    searchRecipients(body?: PushMessageRecipientSearchCriteria | undefined): Promise<PushMessageRecipientSearchResult> {
        let url_ = this.baseUrl + "/api/push-message/search-recipients";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_: RequestInit = {
            body: content_,
            method: "POST",
            headers: {
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processSearchRecipients(_response);
        });
    }

    protected processSearchRecipients(response: Response): Promise<PushMessageRecipientSearchResult> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessageRecipientSearchResult.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessageRecipientSearchResult>(null as any);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    search(body?: PushMessageSearchCriteria | undefined): Promise<PushMessageSearchResult> {
        let url_ = this.baseUrl + "/api/push-message/search";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_: RequestInit = {
            body: content_,
            method: "POST",
            headers: {
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processSearch(_response);
        });
    }

    protected processSearch(response: Response): Promise<PushMessageSearchResult> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessageSearchResult.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessageSearchResult>(null as any);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    create(body?: PushMessage | undefined): Promise<PushMessage> {
        let url_ = this.baseUrl + "/api/push-message";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_: RequestInit = {
            body: content_,
            method: "POST",
            headers: {
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processCreate(_response);
        });
    }

    protected processCreate(response: Response): Promise<PushMessage> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessage.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessage>(null as any);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    update(body?: PushMessage | undefined): Promise<PushMessage> {
        let url_ = this.baseUrl + "/api/push-message";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_: RequestInit = {
            body: content_,
            method: "PUT",
            headers: {
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processUpdate(_response);
        });
    }

    protected processUpdate(response: Response): Promise<PushMessage> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessage.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessage>(null as any);
    }

    /**
     * @param ids (optional) 
     * @return No Content
     */
    delete(ids?: string[] | undefined): Promise<void> {
        let url_ = this.baseUrl + "/api/push-message?";
        if (ids === null)
            throw new Error("The parameter 'ids' cannot be null.");
        else if (ids !== undefined)
            ids && ids.forEach(item => { url_ += "ids=" + encodeURIComponent("" + item) + "&"; });
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "DELETE",
            headers: {
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processDelete(_response);
        });
    }

    protected processDelete(response: Response): Promise<void> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 204) {
            return response.text().then((_responseText) => {
            return;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<void>(null as any);
    }

    /**
     * @return Success
     */
    changeTracking(id: string, value: boolean): Promise<PushMessage> {
        let url_ = this.baseUrl + "/api/push-message/{id}/tracking/{value}";
        if (id === undefined || id === null)
            throw new Error("The parameter 'id' must be defined.");
        url_ = url_.replace("{id}", encodeURIComponent("" + id));
        if (value === undefined || value === null)
            throw new Error("The parameter 'value' must be defined.");
        url_ = url_.replace("{value}", encodeURIComponent("" + value));
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "PUT",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processChangeTracking(_response);
        });
    }

    protected processChangeTracking(response: Response): Promise<PushMessage> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessage.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessage>(null as any);
    }

    /**
     * @param responseGroup (optional) 
     * @return Success
     */
    get(id: string, responseGroup?: string | undefined): Promise<PushMessage> {
        let url_ = this.baseUrl + "/api/push-message/{id}?";
        if (id === undefined || id === null)
            throw new Error("The parameter 'id' must be defined.");
        url_ = url_.replace("{id}", encodeURIComponent("" + id));
        if (responseGroup === null)
            throw new Error("The parameter 'responseGroup' cannot be null.");
        else if (responseGroup !== undefined)
            url_ += "responseGroup=" + encodeURIComponent("" + responseGroup) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "text/plain"
            }
        };

        return this.transformOptions(options_).then(transformedOptions_ => {
            return this.http.fetch(url_, transformedOptions_);
        }).then((_response: Response) => {
            return this.processGet(_response);
        });
    }

    protected processGet(response: Response): Promise<PushMessage> {
        const status = response.status;
        let _headers: any = {}; if (response.headers && response.headers.forEach) { response.headers.forEach((v: any, k: any) => _headers[k] = v); };
        if (status === 200) {
            return response.text().then((_responseText) => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = PushMessage.fromJS(resultData200);
            return result200;
            });
        } else if (status === 401) {
            return response.text().then((_responseText) => {
            return throwException("Unauthorized", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return response.text().then((_responseText) => {
            return throwException("Forbidden", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve<PushMessage>(null as any);
    }
}

export class PushMessage implements IPushMessage {
    topic?: string | undefined;
    shortMessage?: string | undefined;
    startDate?: Date | undefined;
    status?: string | undefined;
    trackNewRecipients?: boolean;
    memberQuery?: string | undefined;
    memberIds?: string[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;

    constructor(data?: IPushMessage) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.topic = _data["topic"];
            this.shortMessage = _data["shortMessage"];
            this.startDate = _data["startDate"] ? new Date(_data["startDate"].toString()) : <any>undefined;
            this.status = _data["status"];
            this.trackNewRecipients = _data["trackNewRecipients"];
            this.memberQuery = _data["memberQuery"];
            if (Array.isArray(_data["memberIds"])) {
                this.memberIds = [] as any;
                for (let item of _data["memberIds"])
                    this.memberIds!.push(item);
            }
            this.createdDate = _data["createdDate"] ? new Date(_data["createdDate"].toString()) : <any>undefined;
            this.modifiedDate = _data["modifiedDate"] ? new Date(_data["modifiedDate"].toString()) : <any>undefined;
            this.createdBy = _data["createdBy"];
            this.modifiedBy = _data["modifiedBy"];
            this.id = _data["id"];
        }
    }

    static fromJS(data: any): PushMessage {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessage();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["topic"] = this.topic;
        data["shortMessage"] = this.shortMessage;
        data["startDate"] = this.startDate ? this.startDate.toISOString() : <any>undefined;
        data["status"] = this.status;
        data["trackNewRecipients"] = this.trackNewRecipients;
        data["memberQuery"] = this.memberQuery;
        if (Array.isArray(this.memberIds)) {
            data["memberIds"] = [];
            for (let item of this.memberIds)
                data["memberIds"].push(item);
        }
        data["createdDate"] = this.createdDate ? this.createdDate.toISOString() : <any>undefined;
        data["modifiedDate"] = this.modifiedDate ? this.modifiedDate.toISOString() : <any>undefined;
        data["createdBy"] = this.createdBy;
        data["modifiedBy"] = this.modifiedBy;
        data["id"] = this.id;
        return data;
    }
}

export interface IPushMessage {
    topic?: string | undefined;
    shortMessage?: string | undefined;
    startDate?: Date | undefined;
    status?: string | undefined;
    trackNewRecipients?: boolean;
    memberQuery?: string | undefined;
    memberIds?: string[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}

export class PushMessageRecipient implements IPushMessageRecipient {
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

    constructor(data?: IPushMessageRecipient) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.messageId = _data["messageId"];
            this.memberId = _data["memberId"];
            this.memberName = _data["memberName"];
            this.userId = _data["userId"];
            this.userName = _data["userName"];
            this.isRead = _data["isRead"];
            this.isHidden = _data["isHidden"];
            this.message = _data["message"] ? PushMessage.fromJS(_data["message"]) : <any>undefined;
            this.createdDate = _data["createdDate"] ? new Date(_data["createdDate"].toString()) : <any>undefined;
            this.modifiedDate = _data["modifiedDate"] ? new Date(_data["modifiedDate"].toString()) : <any>undefined;
            this.createdBy = _data["createdBy"];
            this.modifiedBy = _data["modifiedBy"];
            this.id = _data["id"];
        }
    }

    static fromJS(data: any): PushMessageRecipient {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessageRecipient();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["messageId"] = this.messageId;
        data["memberId"] = this.memberId;
        data["memberName"] = this.memberName;
        data["userId"] = this.userId;
        data["userName"] = this.userName;
        data["isRead"] = this.isRead;
        data["isHidden"] = this.isHidden;
        data["message"] = this.message ? this.message.toJSON() : <any>undefined;
        data["createdDate"] = this.createdDate ? this.createdDate.toISOString() : <any>undefined;
        data["modifiedDate"] = this.modifiedDate ? this.modifiedDate.toISOString() : <any>undefined;
        data["createdBy"] = this.createdBy;
        data["modifiedBy"] = this.modifiedBy;
        data["id"] = this.id;
        return data;
    }
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

export class PushMessageRecipientSearchCriteria implements IPushMessageRecipientSearchCriteria {
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

    constructor(data?: IPushMessageRecipientSearchCriteria) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.messageId = _data["messageId"];
            this.userId = _data["userId"];
            this.isRead = _data["isRead"];
            this.withHidden = _data["withHidden"];
            this.responseGroup = _data["responseGroup"];
            this.objectType = _data["objectType"];
            if (Array.isArray(_data["objectTypes"])) {
                this.objectTypes = [] as any;
                for (let item of _data["objectTypes"])
                    this.objectTypes!.push(item);
            }
            if (Array.isArray(_data["objectIds"])) {
                this.objectIds = [] as any;
                for (let item of _data["objectIds"])
                    this.objectIds!.push(item);
            }
            this.keyword = _data["keyword"];
            this.searchPhrase = _data["searchPhrase"];
            this.languageCode = _data["languageCode"];
            this.sort = _data["sort"];
            if (Array.isArray(_data["sortInfos"])) {
                (<any>this).sortInfos = [] as any;
                for (let item of _data["sortInfos"])
                    (<any>this).sortInfos!.push(SortInfo.fromJS(item));
            }
            this.skip = _data["skip"];
            this.take = _data["take"];
        }
    }

    static fromJS(data: any): PushMessageRecipientSearchCriteria {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessageRecipientSearchCriteria();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["messageId"] = this.messageId;
        data["userId"] = this.userId;
        data["isRead"] = this.isRead;
        data["withHidden"] = this.withHidden;
        data["responseGroup"] = this.responseGroup;
        data["objectType"] = this.objectType;
        if (Array.isArray(this.objectTypes)) {
            data["objectTypes"] = [];
            for (let item of this.objectTypes)
                data["objectTypes"].push(item);
        }
        if (Array.isArray(this.objectIds)) {
            data["objectIds"] = [];
            for (let item of this.objectIds)
                data["objectIds"].push(item);
        }
        data["keyword"] = this.keyword;
        data["searchPhrase"] = this.searchPhrase;
        data["languageCode"] = this.languageCode;
        data["sort"] = this.sort;
        if (Array.isArray(this.sortInfos)) {
            data["sortInfos"] = [];
            for (let item of this.sortInfos)
                data["sortInfos"].push(item.toJSON());
        }
        data["skip"] = this.skip;
        data["take"] = this.take;
        return data;
    }
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

export class PushMessageRecipientSearchResult implements IPushMessageRecipientSearchResult {
    totalCount?: number;
    results?: PushMessageRecipient[] | undefined;

    constructor(data?: IPushMessageRecipientSearchResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.totalCount = _data["totalCount"];
            if (Array.isArray(_data["results"])) {
                this.results = [] as any;
                for (let item of _data["results"])
                    this.results!.push(PushMessageRecipient.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PushMessageRecipientSearchResult {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessageRecipientSearchResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalCount"] = this.totalCount;
        if (Array.isArray(this.results)) {
            data["results"] = [];
            for (let item of this.results)
                data["results"].push(item.toJSON());
        }
        return data;
    }
}

export interface IPushMessageRecipientSearchResult {
    totalCount?: number;
    results?: PushMessageRecipient[] | undefined;
}

export class PushMessageSearchCriteria implements IPushMessageSearchCriteria {
    isDraft?: boolean | undefined;
    trackNewRecipients?: boolean | undefined;
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

    constructor(data?: IPushMessageSearchCriteria) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.isDraft = _data["isDraft"];
            this.trackNewRecipients = _data["trackNewRecipients"];
            this.startDateBefore = _data["startDateBefore"] ? new Date(_data["startDateBefore"].toString()) : <any>undefined;
            if (Array.isArray(_data["statuses"])) {
                this.statuses = [] as any;
                for (let item of _data["statuses"])
                    this.statuses!.push(item);
            }
            this.responseGroup = _data["responseGroup"];
            this.objectType = _data["objectType"];
            if (Array.isArray(_data["objectTypes"])) {
                this.objectTypes = [] as any;
                for (let item of _data["objectTypes"])
                    this.objectTypes!.push(item);
            }
            if (Array.isArray(_data["objectIds"])) {
                this.objectIds = [] as any;
                for (let item of _data["objectIds"])
                    this.objectIds!.push(item);
            }
            this.keyword = _data["keyword"];
            this.searchPhrase = _data["searchPhrase"];
            this.languageCode = _data["languageCode"];
            this.sort = _data["sort"];
            if (Array.isArray(_data["sortInfos"])) {
                (<any>this).sortInfos = [] as any;
                for (let item of _data["sortInfos"])
                    (<any>this).sortInfos!.push(SortInfo.fromJS(item));
            }
            this.skip = _data["skip"];
            this.take = _data["take"];
        }
    }

    static fromJS(data: any): PushMessageSearchCriteria {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessageSearchCriteria();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isDraft"] = this.isDraft;
        data["trackNewRecipients"] = this.trackNewRecipients;
        data["startDateBefore"] = this.startDateBefore ? this.startDateBefore.toISOString() : <any>undefined;
        if (Array.isArray(this.statuses)) {
            data["statuses"] = [];
            for (let item of this.statuses)
                data["statuses"].push(item);
        }
        data["responseGroup"] = this.responseGroup;
        data["objectType"] = this.objectType;
        if (Array.isArray(this.objectTypes)) {
            data["objectTypes"] = [];
            for (let item of this.objectTypes)
                data["objectTypes"].push(item);
        }
        if (Array.isArray(this.objectIds)) {
            data["objectIds"] = [];
            for (let item of this.objectIds)
                data["objectIds"].push(item);
        }
        data["keyword"] = this.keyword;
        data["searchPhrase"] = this.searchPhrase;
        data["languageCode"] = this.languageCode;
        data["sort"] = this.sort;
        if (Array.isArray(this.sortInfos)) {
            data["sortInfos"] = [];
            for (let item of this.sortInfos)
                data["sortInfos"].push(item.toJSON());
        }
        data["skip"] = this.skip;
        data["take"] = this.take;
        return data;
    }
}

export interface IPushMessageSearchCriteria {
    isDraft?: boolean | undefined;
    trackNewRecipients?: boolean | undefined;
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

export class PushMessageSearchResult implements IPushMessageSearchResult {
    totalCount?: number;
    results?: PushMessage[] | undefined;

    constructor(data?: IPushMessageSearchResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.totalCount = _data["totalCount"];
            if (Array.isArray(_data["results"])) {
                this.results = [] as any;
                for (let item of _data["results"])
                    this.results!.push(PushMessage.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PushMessageSearchResult {
        data = typeof data === 'object' ? data : {};
        let result = new PushMessageSearchResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalCount"] = this.totalCount;
        if (Array.isArray(this.results)) {
            data["results"] = [];
            for (let item of this.results)
                data["results"].push(item.toJSON());
        }
        return data;
    }
}

export interface IPushMessageSearchResult {
    totalCount?: number;
    results?: PushMessage[] | undefined;
}

export enum SortDirection {
    Ascending = "Ascending",
    Descending = "Descending",
}

export class SortInfo implements ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;

    constructor(data?: ISortInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.sortColumn = _data["sortColumn"];
            this.sortDirection = _data["sortDirection"];
        }
    }

    static fromJS(data: any): SortInfo {
        data = typeof data === 'object' ? data : {};
        let result = new SortInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sortColumn"] = this.sortColumn;
        data["sortDirection"] = this.sortDirection;
        return data;
    }
}

export interface ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;
}

export enum SortInfoSortDirection {
    Ascending = "Ascending",
    Descending = "Descending",
}

export class ApiException extends Error {
    override message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();

        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }

    protected isApiException = true;

    static isApiException(obj: any): obj is ApiException {
        return obj.isApiException === true;
    }
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): any {
    if (result !== null && result !== undefined)
        throw result;
    else
        throw new ApiException(message, status, response, headers, null);
}

/* eslint-disable */