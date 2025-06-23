export declare class AuthApiBase {
    authToken: string;
    protected constructor();
    getBaseUrl(defaultUrl: string, baseUrl: string): string;
    setAuthToken(token: string): void;
    protected transformOptions(options: any): Promise<any>;
}
export declare class CustomerModuleClient extends AuthApiBase {
    private http;
    private baseUrl;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined;
    constructor(baseUrl?: string, http?: {
        fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
    });
    /**
     * Get organizations
     * @return Success
     */
    listOrganizations(): Promise<Organization[]>;
    protected processListOrganizations(response: Response): Promise<Organization[]>;
    /**
     * Get members
     * @param body (optional) concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter
     * @return Success
     */
    searchMember(body?: MembersSearchCriteria | undefined): Promise<MemberSearchResult>;
    protected processSearchMember(response: Response): Promise<MemberSearchResult>;
    /**
     * Get member
     * @param id member id
     * @param responseGroup (optional) response group
     * @param memberType (optional) member type
     * @return Success
     */
    getMemberById(id: string, responseGroup?: string | undefined, memberType?: string | undefined): Promise<Member>;
    protected processGetMemberById(response: Response): Promise<Member>;
    /**
     * @param responseGroup (optional)
     * @param memberType (optional)
     * @return Success
     */
    getMemberByUserId(userId: string, responseGroup?: string | undefined, memberType?: string | undefined): Promise<Member>;
    protected processGetMemberByUserId(response: Response): Promise<Member>;
    /**
     * @param ids (optional)
     * @param responseGroup (optional)
     * @param memberTypes (optional)
     * @return Success
     */
    getMembersByIds(ids?: string[] | undefined, responseGroup?: string | undefined, memberTypes?: string[] | undefined): Promise<Member[]>;
    protected processGetMembersByIds(response: Response): Promise<Member[]>;
    /**
     * Create new member (can be any object inherited from Member type)
     * @param body (optional) concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter
     * @return Success
     */
    createMember(body?: Member | undefined): Promise<Member>;
    protected processCreateMember(response: Response): Promise<Member>;
    /**
     * Update member
     * @param body (optional) concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter
     * @return No Content
     */
    updateMember(body?: Member | undefined): Promise<void>;
    protected processUpdateMember(response: Response): Promise<void>;
    /**
     * Delete members
     * @param ids (optional) An array of members ids
     * @return No Content
     */
    deleteMembers(ids?: string[] | undefined): Promise<void>;
    protected processDeleteMembers(response: Response): Promise<void>;
    /**
     * Bulk create new members (can be any objects inherited from Member type)
     * @param body (optional) Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter
     * @return Success
     */
    bulkCreateMembers(body?: Member[] | undefined): Promise<Member[]>;
    protected processBulkCreateMembers(response: Response): Promise<Member[]>;
    /**
     * Bulk update members
     * @param body (optional) Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter
     * @return No Content
     */
    bulkUpdateMembers(body?: Member[] | undefined): Promise<void>;
    protected processBulkUpdateMembers(response: Response): Promise<void>;
    /**
     * Bulk delete members
     * @param body (optional) concrete instance of SearchCriteria type will be created by using PolymorphicMemberSearchCriteriaJsonConverter
     * @return No Content
     */
    bulkDeleteMembersBySearchCriteria(body?: MembersSearchCriteria | undefined): Promise<void>;
    protected processBulkDeleteMembersBySearchCriteria(response: Response): Promise<void>;
    /**
     * Create contact
     * @param body (optional)
     * @return Success
     */
    createContact(body?: Contact | undefined): Promise<Contact>;
    protected processCreateContact(response: Response): Promise<Contact>;
    /**
     * Update contact
     * @param body (optional)
     * @return No Content
     */
    updateContact(body?: Contact | undefined): Promise<void>;
    protected processUpdateContact(response: Response): Promise<void>;
    /**
     * Delete contacts
     * @param ids (optional) An array of contacts ids
     * @return No Content
     */
    deleteContacts(ids?: string[] | undefined): Promise<void>;
    protected processDeleteContacts(response: Response): Promise<void>;
    /**
     * Get plenty contacts
     * @param ids (optional) contact IDs
     * @return Success
     */
    getContactsByIds(ids?: string[] | undefined): Promise<Contact[]>;
    protected processGetContactsByIds(response: Response): Promise<Contact[]>;
    /**
     * Bulk create contacts
     * @param body (optional)
     * @return Success
     */
    bulkCreateContacts(body?: Contact[] | undefined): Promise<Contact[]>;
    protected processBulkCreateContacts(response: Response): Promise<Contact[]>;
    /**
     * Bulk update contact
     * @param body (optional)
     * @return No Content
     */
    bulkUpdateContacts(body?: Contact[] | undefined): Promise<void>;
    protected processBulkUpdateContacts(response: Response): Promise<void>;
    /**
     * Create organization
     * @param body (optional)
     * @return Success
     */
    createOrganization(body?: Organization | undefined): Promise<Organization>;
    protected processCreateOrganization(response: Response): Promise<Organization>;
    /**
     * Update organization
     * @param body (optional)
     * @return No Content
     */
    updateOrganization(body?: Organization | undefined): Promise<void>;
    protected processUpdateOrganization(response: Response): Promise<void>;
    /**
     * Delete organizations
     * @param ids (optional) An array of organizations ids
     * @return No Content
     */
    deleteOrganizations(ids?: string[] | undefined): Promise<void>;
    protected processDeleteOrganizations(response: Response): Promise<void>;
    /**
     * Get plenty organizations
     * @param ids (optional) Organization ids
     * @return Success
     */
    getOrganizationsByIds(ids?: string[] | undefined): Promise<Organization[]>;
    protected processGetOrganizationsByIds(response: Response): Promise<Organization[]>;
    /**
     * Bulk create organizations
     * @param body (optional)
     * @return No Content
     */
    bulkCreateOrganizations(body?: Organization[] | undefined): Promise<void>;
    protected processBulkCreateOrganizations(response: Response): Promise<void>;
    /**
     * Bulk update organization
     * @param body (optional)
     * @return No Content
     */
    bulkUpdateOrganizations(body?: Organization[] | undefined): Promise<void>;
    protected processBulkUpdateOrganizations(response: Response): Promise<void>;
    /**
     * Get organization
     * @param id Organization id
     * @return Success
     */
    getOrganizationById(id: string): Promise<Organization>;
    protected processGetOrganizationById(response: Response): Promise<Organization>;
    /**
     * Search organizations
     * @param body (optional) concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter
     * @return Success
     */
    searchOrganizations(body?: MembersSearchCriteria | undefined): Promise<OrganizationSearchResult>;
    protected processSearchOrganizations(response: Response): Promise<OrganizationSearchResult>;
    /**
     * Get contact
     * @param id Contact ID
     * @return Success
     */
    getContactById(id: string): Promise<Contact>;
    protected processGetContactById(response: Response): Promise<Contact>;
    /**
     * Search contacts
     * @param body (optional) concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter
     * @return Success
     */
    searchContacts(body?: MembersSearchCriteria | undefined): Promise<ContactSearchResult>;
    protected processSearchContacts(response: Response): Promise<ContactSearchResult>;
    /**
     * Get vendor
     * @param id Vendor ID
     * @return Success
     */
    getVendorById(id: string): Promise<Vendor>;
    protected processGetVendorById(response: Response): Promise<Vendor>;
    /**
     * Get plenty vendors
     * @param ids (optional) Vendors IDs
     * @return Success
     */
    getVendorsByIds(ids?: string[] | undefined): Promise<Vendor[]>;
    protected processGetVendorsByIds(response: Response): Promise<Vendor[]>;
    /**
     * Search vendors
     * @param body (optional) concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter
     * @return Success
     */
    searchVendors(body?: MembersSearchCriteria | undefined): Promise<VendorSearchResult>;
    protected processSearchVendors(response: Response): Promise<VendorSearchResult>;
    /**
     * @param memberId (optional)
     * @param body (optional)
     * @return No Content
     */
    updateAddesses(memberId?: string | undefined, body?: CustomerAddress[] | undefined): Promise<void>;
    protected processUpdateAddesses(response: Response): Promise<void>;
    /**
     * Create employee
     * @param body (optional)
     * @return Success
     */
    createEmployee(body?: Employee | undefined): Promise<Employee>;
    protected processCreateEmployee(response: Response): Promise<Employee>;
    /**
     * Get plenty employees
     * @param ids (optional) contact IDs
     * @return Success
     */
    getEmployeesByIds(ids?: string[] | undefined): Promise<Employee[]>;
    protected processGetEmployeesByIds(response: Response): Promise<Employee[]>;
    /**
     * Create employee
     * @param body (optional)
     * @return Success
     */
    bulkCreateEmployees(body?: Employee[] | undefined): Promise<Employee[]>;
    protected processBulkCreateEmployees(response: Response): Promise<Employee[]>;
    /**
     * Get all member organizations
     * @param id member Id
     * @return Success
     */
    getMemberOrganizations(id: string): Promise<Organization[]>;
    protected processGetMemberOrganizations(response: Response): Promise<Organization[]>;
}
/** Obsolete. Left due to compatibility issues. Will be removed. Instead of, use: ApplicationUser.EmailConfirmed, ApplicationUser.LockoutEnd. */
export declare enum AccountState {
    PendingApproval = "PendingApproval",
    Approved = "Approved",
    Rejected = "Rejected"
}
export declare enum AddressType {
    Undefined = "Undefined",
    Billing = "Billing",
    Shipping = "Shipping",
    BillingAndShipping = "BillingAndShipping",
    Pickup = "Pickup"
}
export declare class ApplicationUser implements IApplicationUser {
    /** Tenant id */
    storeId?: string | undefined;
    memberId?: string | undefined;
    isAdministrator?: boolean;
    photoUrl?: string | undefined;
    userType?: string | undefined;
    status?: string | undefined;
    password?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    roles?: Role[] | undefined;
    /** Obsolete. Use LockoutEnd. DateTime in UTC when lockout ends, any time in the past is considered not locked out. */
    lockoutEndDateUtc?: Date | undefined;
    /** Obsolete. Left due to compatibility issues. Will be removed. Instead of, use: ApplicationUser.EmailConfirmed, ApplicationUser.LockoutEnd. */
    userState?: ApplicationUserUserState;
    /** Obsolete. All permissions from assigned roles. */
    permissions?: string[] | undefined;
    /** External provider logins. */
    logins?: ApplicationUserLogin[] | undefined;
    /** Indicates that the password for this user is expired and must be changed. */
    passwordExpired?: boolean;
    /** The last date when the password was changed */
    lastPasswordChangedDate?: Date | undefined;
    /** The last date when the requested password change. */
    lastPasswordChangeRequestDate?: Date | undefined;
    /** The last login date */
    lastLoginDate?: Date | undefined;
    id?: string | undefined;
    userName?: string | undefined;
    normalizedUserName?: string | undefined;
    email?: string | undefined;
    normalizedEmail?: string | undefined;
    emailConfirmed?: boolean;
    passwordHash?: string | undefined;
    securityStamp?: string | undefined;
    concurrencyStamp?: string | undefined;
    phoneNumber?: string | undefined;
    phoneNumberConfirmed?: boolean;
    twoFactorEnabled?: boolean;
    lockoutEnd?: Date | undefined;
    lockoutEnabled?: boolean;
    accessFailedCount?: number;
    constructor(data?: IApplicationUser);
    init(_data?: any): void;
    static fromJS(data: any): ApplicationUser;
    toJSON(data?: any): any;
}
export interface IApplicationUser {
    /** Tenant id */
    storeId?: string | undefined;
    memberId?: string | undefined;
    isAdministrator?: boolean;
    photoUrl?: string | undefined;
    userType?: string | undefined;
    status?: string | undefined;
    password?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    roles?: Role[] | undefined;
    /** Obsolete. Use LockoutEnd. DateTime in UTC when lockout ends, any time in the past is considered not locked out. */
    lockoutEndDateUtc?: Date | undefined;
    /** Obsolete. Left due to compatibility issues. Will be removed. Instead of, use: ApplicationUser.EmailConfirmed, ApplicationUser.LockoutEnd. */
    userState?: ApplicationUserUserState;
    /** Obsolete. All permissions from assigned roles. */
    permissions?: string[] | undefined;
    /** External provider logins. */
    logins?: ApplicationUserLogin[] | undefined;
    /** Indicates that the password for this user is expired and must be changed. */
    passwordExpired?: boolean;
    /** The last date when the password was changed */
    lastPasswordChangedDate?: Date | undefined;
    /** The last date when the requested password change. */
    lastPasswordChangeRequestDate?: Date | undefined;
    /** The last login date */
    lastLoginDate?: Date | undefined;
    id?: string | undefined;
    userName?: string | undefined;
    normalizedUserName?: string | undefined;
    email?: string | undefined;
    normalizedEmail?: string | undefined;
    emailConfirmed?: boolean;
    passwordHash?: string | undefined;
    securityStamp?: string | undefined;
    concurrencyStamp?: string | undefined;
    phoneNumber?: string | undefined;
    phoneNumberConfirmed?: boolean;
    twoFactorEnabled?: boolean;
    lockoutEnd?: Date | undefined;
    lockoutEnabled?: boolean;
    accessFailedCount?: number;
}
export declare class ApplicationUserLogin implements IApplicationUserLogin {
    loginProvider?: string | undefined;
    providerKey?: string | undefined;
    constructor(data?: IApplicationUserLogin);
    init(_data?: any): void;
    static fromJS(data: any): ApplicationUserLogin;
    toJSON(data?: any): any;
}
export interface IApplicationUserLogin {
    loginProvider?: string | undefined;
    providerKey?: string | undefined;
}
export declare class Contact implements IContact {
    salutation?: string | undefined;
    fullName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    birthDate?: Date | undefined;
    defaultLanguage?: string | undefined;
    timeZone?: string | undefined;
    organizations?: string[] | undefined;
    associatedOrganizations?: string[] | undefined;
    taxPayerId?: string | undefined;
    preferredDelivery?: string | undefined;
    preferredCommunication?: string | undefined;
    defaultShippingAddressId?: string | undefined;
    defaultBillingAddressId?: string | undefined;
    photoUrl?: string | undefined;
    isAnonymized?: boolean;
    about?: string | undefined;
    readonly objectType?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    readonly seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IContact);
    init(_data?: any): void;
    static fromJS(data: any): Contact;
    toJSON(data?: any): any;
}
export interface IContact {
    salutation?: string | undefined;
    fullName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    birthDate?: Date | undefined;
    defaultLanguage?: string | undefined;
    timeZone?: string | undefined;
    organizations?: string[] | undefined;
    associatedOrganizations?: string[] | undefined;
    taxPayerId?: string | undefined;
    preferredDelivery?: string | undefined;
    preferredCommunication?: string | undefined;
    defaultShippingAddressId?: string | undefined;
    defaultBillingAddressId?: string | undefined;
    photoUrl?: string | undefined;
    isAnonymized?: boolean;
    about?: string | undefined;
    objectType?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class ContactSearchResult implements IContactSearchResult {
    totalCount?: number;
    results?: Contact[] | undefined;
    constructor(data?: IContactSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): ContactSearchResult;
    toJSON(data?: any): any;
}
export interface IContactSearchResult {
    totalCount?: number;
    results?: Contact[] | undefined;
}
export declare class CustomerAddress implements ICustomerAddress {
    addressType?: CustomerAddressAddressType;
    key?: string | undefined;
    name?: string | undefined;
    organization?: string | undefined;
    countryCode?: string | undefined;
    countryName?: string | undefined;
    city?: string | undefined;
    postalCode?: string | undefined;
    zip?: string | undefined;
    line1?: string | undefined;
    line2?: string | undefined;
    regionId?: string | undefined;
    regionName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    phone?: string | undefined;
    email?: string | undefined;
    outerId?: string | undefined;
    isDefault?: boolean;
    description?: string | undefined;
    constructor(data?: ICustomerAddress);
    init(_data?: any): void;
    static fromJS(data: any): CustomerAddress;
    toJSON(data?: any): any;
}
export interface ICustomerAddress {
    addressType?: CustomerAddressAddressType;
    key?: string | undefined;
    name?: string | undefined;
    organization?: string | undefined;
    countryCode?: string | undefined;
    countryName?: string | undefined;
    city?: string | undefined;
    postalCode?: string | undefined;
    zip?: string | undefined;
    line1?: string | undefined;
    line2?: string | undefined;
    regionId?: string | undefined;
    regionName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    phone?: string | undefined;
    email?: string | undefined;
    outerId?: string | undefined;
    isDefault?: boolean;
    description?: string | undefined;
}
export declare class DynamicObjectProperty implements IDynamicObjectProperty {
    objectId?: string | undefined;
    values?: DynamicPropertyObjectValue[] | undefined;
    name?: string | undefined;
    /** dynamic property description */
    description?: string | undefined;
    objectType?: string | undefined;
    /** Defines whether a property supports multiple values. */
    isArray?: boolean;
    /** Dictionary has a predefined set of values. User can select one or more of them and cannot enter arbitrary values. */
    isDictionary?: boolean;
    /** For multilingual properties user can enter different values for each of registered languages. */
    isMultilingual?: boolean;
    isRequired?: boolean;
    displayOrder?: number | undefined;
    /** The storage property type */
    valueType?: DynamicObjectPropertyValueType;
    /** Property names for different languages. */
    displayNames?: DynamicPropertyName[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IDynamicObjectProperty);
    init(_data?: any): void;
    static fromJS(data: any): DynamicObjectProperty;
    toJSON(data?: any): any;
}
export interface IDynamicObjectProperty {
    objectId?: string | undefined;
    values?: DynamicPropertyObjectValue[] | undefined;
    name?: string | undefined;
    /** dynamic property description */
    description?: string | undefined;
    objectType?: string | undefined;
    /** Defines whether a property supports multiple values. */
    isArray?: boolean;
    /** Dictionary has a predefined set of values. User can select one or more of them and cannot enter arbitrary values. */
    isDictionary?: boolean;
    /** For multilingual properties user can enter different values for each of registered languages. */
    isMultilingual?: boolean;
    isRequired?: boolean;
    displayOrder?: number | undefined;
    /** The storage property type */
    valueType?: DynamicObjectPropertyValueType;
    /** Property names for different languages. */
    displayNames?: DynamicPropertyName[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class DynamicPropertyName implements IDynamicPropertyName {
    /** Language ID, e.g. en-US. */
    locale?: string | undefined;
    name?: string | undefined;
    constructor(data?: IDynamicPropertyName);
    init(_data?: any): void;
    static fromJS(data: any): DynamicPropertyName;
    toJSON(data?: any): any;
}
export interface IDynamicPropertyName {
    /** Language ID, e.g. en-US. */
    locale?: string | undefined;
    name?: string | undefined;
}
export declare class DynamicPropertyObjectValue implements IDynamicPropertyObjectValue {
    objectType?: string | undefined;
    objectId?: string | undefined;
    locale?: string | undefined;
    value?: any | undefined;
    valueId?: string | undefined;
    valueType?: DynamicPropertyObjectValueValueType;
    propertyId?: string | undefined;
    propertyName?: string | undefined;
    constructor(data?: IDynamicPropertyObjectValue);
    init(_data?: any): void;
    static fromJS(data: any): DynamicPropertyObjectValue;
    toJSON(data?: any): any;
}
export interface IDynamicPropertyObjectValue {
    objectType?: string | undefined;
    objectId?: string | undefined;
    locale?: string | undefined;
    value?: any | undefined;
    valueId?: string | undefined;
    valueType?: DynamicPropertyObjectValueValueType;
    propertyId?: string | undefined;
    propertyName?: string | undefined;
}
export declare enum DynamicPropertyValueType {
    Undefined = "Undefined",
    ShortText = "ShortText",
    LongText = "LongText",
    Integer = "Integer",
    Decimal = "Decimal",
    DateTime = "DateTime",
    Boolean = "Boolean",
    Html = "Html",
    Image = "Image"
}
export declare class Employee implements IEmployee {
    salutation?: string | undefined;
    fullName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    birthDate?: Date | undefined;
    defaultLanguage?: string | undefined;
    timeZone?: string | undefined;
    organizations?: string[] | undefined;
    employeeType?: string | undefined;
    isActive?: boolean;
    photoUrl?: string | undefined;
    readonly objectType?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    readonly seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IEmployee);
    init(_data?: any): void;
    static fromJS(data: any): Employee;
    toJSON(data?: any): any;
}
export interface IEmployee {
    salutation?: string | undefined;
    fullName?: string | undefined;
    firstName?: string | undefined;
    middleName?: string | undefined;
    lastName?: string | undefined;
    birthDate?: Date | undefined;
    defaultLanguage?: string | undefined;
    timeZone?: string | undefined;
    organizations?: string[] | undefined;
    employeeType?: string | undefined;
    isActive?: boolean;
    photoUrl?: string | undefined;
    objectType?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class Member implements IMember {
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    readonly objectType?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    readonly seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IMember);
    init(_data?: any): void;
    static fromJS(data: any): Member;
    toJSON(data?: any): any;
}
export interface IMember {
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    objectType?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class MemberSearchResult implements IMemberSearchResult {
    totalCount?: number;
    results?: Member[] | undefined;
    constructor(data?: IMemberSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): MemberSearchResult;
    toJSON(data?: any): any;
}
export interface IMemberSearchResult {
    totalCount?: number;
    results?: Member[] | undefined;
}
export declare class MembersSearchCriteria implements IMembersSearchCriteria {
    memberType?: string | undefined;
    memberTypes?: string[] | undefined;
    group?: string | undefined;
    groups?: string[] | undefined;
    memberId?: string | undefined;
    deepSearch?: boolean;
    outerIds?: string[] | undefined;
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
    constructor(data?: IMembersSearchCriteria);
    init(_data?: any): void;
    static fromJS(data: any): MembersSearchCriteria;
    toJSON(data?: any): any;
}
export interface IMembersSearchCriteria {
    memberType?: string | undefined;
    memberTypes?: string[] | undefined;
    group?: string | undefined;
    groups?: string[] | undefined;
    memberId?: string | undefined;
    deepSearch?: boolean;
    outerIds?: string[] | undefined;
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
export declare class Note implements INote {
    title?: string | undefined;
    body?: string | undefined;
    outerId?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: INote);
    init(_data?: any): void;
    static fromJS(data: any): Note;
    toJSON(data?: any): any;
}
export interface INote {
    title?: string | undefined;
    body?: string | undefined;
    outerId?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class Organization implements IOrganization {
    description?: string | undefined;
    businessCategory?: string | undefined;
    ownerId?: string | undefined;
    parentId?: string | undefined;
    readonly objectType?: string | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    readonly seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IOrganization);
    init(_data?: any): void;
    static fromJS(data: any): Organization;
    toJSON(data?: any): any;
}
export interface IOrganization {
    description?: string | undefined;
    businessCategory?: string | undefined;
    ownerId?: string | undefined;
    parentId?: string | undefined;
    objectType?: string | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class OrganizationSearchResult implements IOrganizationSearchResult {
    totalCount?: number;
    results?: Organization[] | undefined;
    constructor(data?: IOrganizationSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): OrganizationSearchResult;
    toJSON(data?: any): any;
}
export interface IOrganizationSearchResult {
    totalCount?: number;
    results?: Organization[] | undefined;
}
export declare class Permission implements IPermission {
    id?: string | undefined;
    name?: string | undefined;
    /** Id of the module which has registered this permission. */
    moduleId?: string | undefined;
    /** Display name of the group to which this permission belongs. The '|' character is used to separate Child and parent groups. */
    groupName?: string | undefined;
    assignedScopes?: PermissionScope[] | undefined;
    readonly availableScopes?: PermissionScope[] | undefined;
    constructor(data?: IPermission);
    init(_data?: any): void;
    static fromJS(data: any): Permission;
    toJSON(data?: any): any;
}
export interface IPermission {
    id?: string | undefined;
    name?: string | undefined;
    /** Id of the module which has registered this permission. */
    moduleId?: string | undefined;
    /** Display name of the group to which this permission belongs. The '|' character is used to separate Child and parent groups. */
    groupName?: string | undefined;
    assignedScopes?: PermissionScope[] | undefined;
    availableScopes?: PermissionScope[] | undefined;
}
export declare class PermissionScope implements IPermissionScope {
    /** Scope type name */
    type?: string | undefined;
    /** Display label for particular scope value used only for  representation */
    label?: string | undefined;
    /** Represent string scope value */
    scope?: string | undefined;
    constructor(data?: IPermissionScope);
    init(_data?: any): void;
    static fromJS(data: any): PermissionScope;
    toJSON(data?: any): any;
}
export interface IPermissionScope {
    /** Scope type name */
    type?: string | undefined;
    /** Display label for particular scope value used only for  representation */
    label?: string | undefined;
    /** Represent string scope value */
    scope?: string | undefined;
}
export declare class Role implements IRole {
    description?: string | undefined;
    permissions?: Permission[] | undefined;
    id?: string | undefined;
    name?: string | undefined;
    normalizedName?: string | undefined;
    concurrencyStamp?: string | undefined;
    constructor(data?: IRole);
    init(_data?: any): void;
    static fromJS(data: any): Role;
    toJSON(data?: any): any;
}
export interface IRole {
    description?: string | undefined;
    permissions?: Permission[] | undefined;
    id?: string | undefined;
    name?: string | undefined;
    normalizedName?: string | undefined;
    concurrencyStamp?: string | undefined;
}
export declare class SeoInfo implements ISeoInfo {
    name?: string | undefined;
    /** Slug */
    semanticUrl?: string | undefined;
    /** head title tag content */
    pageTitle?: string | undefined;
    /** <meta name="description" /> */
    metaDescription?: string | undefined;
    imageAltDescription?: string | undefined;
    /** <meta name="keywords" /> */
    metaKeywords?: string | undefined;
    /** Tenant StoreId which SEO defined */
    storeId?: string | undefined;
    /** SEO related object id */
    objectId?: string | undefined;
    /** SEO related object type name */
    objectType?: string | undefined;
    /** Active/Inactive */
    isActive?: boolean;
    languageCode?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: ISeoInfo);
    init(_data?: any): void;
    static fromJS(data: any): SeoInfo;
    toJSON(data?: any): any;
}
export interface ISeoInfo {
    name?: string | undefined;
    /** Slug */
    semanticUrl?: string | undefined;
    /** head title tag content */
    pageTitle?: string | undefined;
    /** <meta name="description" /> */
    metaDescription?: string | undefined;
    imageAltDescription?: string | undefined;
    /** <meta name="keywords" /> */
    metaKeywords?: string | undefined;
    /** Tenant StoreId which SEO defined */
    storeId?: string | undefined;
    /** SEO related object id */
    objectId?: string | undefined;
    /** SEO related object type name */
    objectType?: string | undefined;
    /** Active/Inactive */
    isActive?: boolean;
    languageCode?: string | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
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
export declare class StringIdentityUserRole implements IStringIdentityUserRole {
    userId?: string | undefined;
    roleId?: string | undefined;
    constructor(data?: IStringIdentityUserRole);
    init(_data?: any): void;
    static fromJS(data: any): StringIdentityUserRole;
    toJSON(data?: any): any;
}
export interface IStringIdentityUserRole {
    userId?: string | undefined;
    roleId?: string | undefined;
}
export declare class Vendor implements IVendor {
    description?: string | undefined;
    siteUrl?: string | undefined;
    logoUrl?: string | undefined;
    groupName?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    readonly objectType?: string | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    readonly seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IVendor);
    init(_data?: any): void;
    static fromJS(data: any): Vendor;
    toJSON(data?: any): any;
}
export interface IVendor {
    description?: string | undefined;
    siteUrl?: string | undefined;
    logoUrl?: string | undefined;
    groupName?: string | undefined;
    securityAccounts?: ApplicationUser[] | undefined;
    objectType?: string | undefined;
    name?: string | undefined;
    memberType?: string | undefined;
    outerId?: string | undefined;
    status?: string | undefined;
    addresses?: CustomerAddress[] | undefined;
    phones?: string[] | undefined;
    emails?: string[] | undefined;
    notes?: Note[] | undefined;
    groups?: string[] | undefined;
    iconUrl?: string | undefined;
    dynamicProperties?: DynamicObjectProperty[] | undefined;
    seoObjectType?: string | undefined;
    seoInfos?: SeoInfo[] | undefined;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class VendorSearchResult implements IVendorSearchResult {
    readonly vendors?: Vendor[] | undefined;
    totalCount?: number;
    results?: Vendor[] | undefined;
    constructor(data?: IVendorSearchResult);
    init(_data?: any): void;
    static fromJS(data: any): VendorSearchResult;
    toJSON(data?: any): any;
}
export interface IVendorSearchResult {
    vendors?: Vendor[] | undefined;
    totalCount?: number;
    results?: Vendor[] | undefined;
}
export declare enum ApplicationUserUserState {
    PendingApproval = "PendingApproval",
    Approved = "Approved",
    Rejected = "Rejected"
}
export declare enum CustomerAddressAddressType {
    Undefined = "Undefined",
    Billing = "Billing",
    Shipping = "Shipping",
    BillingAndShipping = "BillingAndShipping",
    Pickup = "Pickup"
}
export declare enum DynamicObjectPropertyValueType {
    Undefined = "Undefined",
    ShortText = "ShortText",
    LongText = "LongText",
    Integer = "Integer",
    Decimal = "Decimal",
    DateTime = "DateTime",
    Boolean = "Boolean",
    Html = "Html",
    Image = "Image"
}
export declare enum DynamicPropertyObjectValueValueType {
    Undefined = "Undefined",
    ShortText = "ShortText",
    LongText = "LongText",
    Integer = "Integer",
    Decimal = "Decimal",
    DateTime = "DateTime",
    Boolean = "Boolean",
    Html = "Html",
    Image = "Image"
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
//# sourceMappingURL=virtocommerce.customer.d.ts.map