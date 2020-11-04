/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.MessagingSystem
{
    public enum MessageAction
    {
        None = -1,

        #region Login

        LoginSuccess = 1000,
        LoginSuccessViaSocialAccount = 1001,
        LoginSuccessViaSms = 1007,
        LoginSuccessViaApi = 1010,
        LoginSuccessViaSocialApp = 1011,
        LoginSuccessViaApiSms = 1012,
        LoginSuccessViaApiTfa = 1024,
        LoginSuccessViaApiSocialAccount = 1019,
        LoginSuccessViaSSO = 1015,
        LoginSuccesViaTfaApp = 1021,
        LoginFailViaSSO = 1018,
        LoginFailInvalidCombination = 1002,
        LoginFailSocialAccountNotFound = 1003,
        LoginFailDisabledProfile = 1004,
        LoginFail = 1005,
        LoginFailViaSms = 1008,
        LoginFailViaApi = 1013,
        LoginFailViaApiSms = 1014,
        LoginFailViaApiTfa = 1025,
        LoginFailViaApiSocialAccount = 1020,
        LoginFailViaTfaApp = 1022,
        LoginFailIpSecurity = 1009,
        LoginFailBruteForce = 1023,
        LoginFailRecaptcha = 1026,  // last login
        Logout = 1006,

        SessionStarted = 1016,
        SessionCompleted = 1017,

        #endregion

        #region Projects

        ProjectCreated = 2000,
        ProjectCreatedFromTemplate = 2001,
        ProjectUpdated = 2002,
        ProjectUpdatedStatus = 2003,
        ProjectFollowed = 2004,
        ProjectUnfollowed = 2005,
        ProjectDeleted = 2006,

        ProjectDeletedMember = 2007,
        ProjectUpdatedTeam = 2008,
        ProjectUpdatedMemberRights = 2009,

        ProjectLinkedCompany = 2010,
        ProjectUnlinkedCompany = 2011,
        ProjectLinkedPerson = 2012,
        ProjectUnlinkedPerson = 2013,
        ProjectLinkedContacts = 2014,

        MilestoneCreated = 2015,
        MilestoneUpdated = 2016,
        MilestoneUpdatedStatus = 2017,
        MilestoneDeleted = 2018,

        TaskCreated = 2019,
        TaskCreatedFromDiscussion = 2020,
        TaskUpdated = 2021,
        TaskUpdatedStatus = 2022,
        TaskMovedToMilestone = 2023,
        TaskUnlinkedMilestone = 2024,
        TaskUpdatedFollowing = 2025,
        TaskAttachedFiles = 2026,
        TaskDetachedFile = 2027,
        TasksLinked = 2028,
        TasksUnlinked = 2029,
        TaskDeleted = 2030,

        TaskCommentCreated = 2031,
        TaskCommentUpdated = 2032,
        TaskCommentDeleted = 2033,

        SubtaskCreated = 2034,
        SubtaskUpdated = 2035,
        SubtaskUpdatedStatus = 2036,
        SubtaskDeleted = 2037,

        DiscussionCreated = 2038,
        DiscussionUpdated = 2039,
        DiscussionUpdatedFollowing = 2040,
        DiscussionAttachedFiles = 2041,
        DiscussionDetachedFile = 2042,
        DiscussionDeleted = 2043,

        DiscussionCommentCreated = 2044,
        DiscussionCommentUpdated = 2045,
        DiscussionCommentDeleted = 2046,

        TaskTimeCreated = 2047,
        TaskTimeUpdated = 2048,
        TaskTimesUpdatedStatus = 2049,
        TaskTimesDeleted = 2050,

        ReportTemplateCreated = 2051,
        ReportTemplateUpdated = 2052,
        ReportTemplateDeleted = 2053,

        ProjectTemplateCreated = 2054,
        ProjectTemplateUpdated = 2055,
        ProjectTemplateDeleted = 2056,

        ProjectsImportedFromBasecamp = 2057,

        #endregion

        #region CRM

        CompanyCreated = 3000,
        CompanyCreatedWithWebForm = 3157,
        CompanyUpdated = 3001,
        CompanyUpdatedPrincipalInfo = 3002,
        CompanyUpdatedPhoto = 3003,
        CompanyUpdatedTemperatureLevel = 3004,
        CompanyUpdatedPersonsTemperatureLevel = 3005,
        CompanyCreatedTag = 3006,
        CompanyCreatedPersonsTag = 3007,
        CompanyDeletedTag = 3008,
        CompanyCreatedHistoryEvent = 3009,
        CompanyDeletedHistoryEvent = 3010,
        CompanyLinkedPerson = 3011,
        CompanyUnlinkedPerson = 3012,
        CompanyLinkedProject = 3013,
        CompanyUnlinkedProject = 3014,
        CompanyAttachedFiles = 3015,
        CompanyDetachedFile = 3159,
        CompaniesMerged = 3016,
        CompanyDeleted = 3017,

        PersonCreated = 3018,
        PersonCreatedWithWebForm = 3158,
        PersonsCreated = 3019,
        PersonUpdated = 3020,
        PersonUpdatedPrincipalInfo = 3021,
        PersonUpdatedPhoto = 3022,
        PersonUpdatedTemperatureLevel = 3023,
        PersonUpdatedCompanyTemperatureLevel = 3024,
        PersonCreatedTag = 3025,
        PersonCreatedCompanyTag = 3026,
        PersonDeletedTag = 3027,
        PersonCreatedHistoryEvent = 3028,
        PersonDeletedHistoryEvent = 3029,
        PersonLinkedProject = 3030,
        PersonUnlinkedProject = 3031,
        PersonAttachedFiles = 3032,
        PersonDetachedFile = 3160,
        PersonsMerged = 3033,
        PersonDeleted = 3034,

        ContactsDeleted = 3035,

        CrmTaskCreated = 3036,
        ContactsCreatedCrmTasks = 3037,
        CrmTaskUpdated = 3038,
        CrmTaskOpened = 3039,
        CrmTaskClosed = 3040,
        CrmTaskDeleted = 3041,

        OpportunityCreated = 3042,
        OpportunityUpdated = 3043,
        OpportunityUpdatedStage = 3044,
        OpportunityCreatedTag = 3045,
        OpportunityDeletedTag = 3046,
        OpportunityCreatedHistoryEvent = 3047,
        OpportunityDeletedHistoryEvent = 3048,
        OpportunityLinkedCompany = 3049,
        OpportunityUnlinkedCompany = 3050,
        OpportunityLinkedPerson = 3051,
        OpportunityUnlinkedPerson = 3052,
        OpportunityAttachedFiles = 3053,
        OpportunityDetachedFile = 3161,
        OpportunityOpenedAccess = 3054,
        OpportunityRestrictedAccess = 3055,
        OpportunityDeleted = 3056,
        OpportunitiesDeleted = 3057,

        InvoiceCreated = 3058,
        InvoiceUpdated = 3059,
        InvoicesUpdatedStatus = 3060,
        InvoiceDeleted = 3061,
        InvoicesDeleted = 3062,

        CaseCreated = 3063,
        CaseUpdated = 3064,
        CaseOpened = 3065,
        CaseClosed = 3066,
        CaseCreatedTag = 3067,
        CaseDeletedTag = 3068,
        CaseCreatedHistoryEvent = 3069,
        CaseDeletedHistoryEvent = 3070,
        CaseLinkedCompany = 3071,
        CaseUnlinkedCompany = 3072,
        CaseLinkedPerson = 3073,
        CaseUnlinkedPerson = 3074,
        CaseAttachedFiles = 3075,
        CaseDetachedFile = 3162,
        CaseOpenedAccess = 3076,
        CaseRestrictedAccess = 3077,
        CaseDeleted = 3078,
        CasesDeleted = 3079,

        CrmSmtpSettingsUpdated = 3080,
        CrmTestMailSent = 3081,
        CrmDefaultCurrencyUpdated = 3082,
        CrmAllDataExported = 3083,

        ContactTemperatureLevelCreated = 3084,
        ContactTemperatureLevelUpdated = 3085,
        ContactTemperatureLevelUpdatedColor = 3086,
        ContactTemperatureLevelsUpdatedOrder = 3087,
        ContactTemperatureLevelDeleted = 3088,
        ContactTemperatureLevelSettingsUpdated = 3089,

        ContactTypeCreated = 3090,
        ContactTypeUpdated = 3091,
        ContactTypesUpdatedOrder = 3092,
        ContactTypeDeleted = 3093,

        InvoiceItemCreated = 3094,
        InvoiceItemUpdated = 3095,
        InvoiceItemDeleted = 3096,
        InvoiceItemsDeleted = 3097,

        InvoiceTaxCreated = 3098,
        InvoiceTaxUpdated = 3099,
        InvoiceTaxDeleted = 3100,

        CurrencyRateUpdated = 3163,
        InvoiceDefaultTermsUpdated = 3164,
        InvoiceDownloaded = 3165,
        CrmSmtpMailSent = 3166,

        OrganizationProfileUpdatedCompanyName = 3101,
        OrganizationProfileUpdatedInvoiceLogo = 3102,
        OrganizationProfileUpdatedAddress = 3103,

        InvoiceNumberFormatUpdated = 3104,

        ContactUserFieldCreated = 3105,
        ContactUserFieldUpdated = 3106,
        ContactUserFieldsUpdatedOrder = 3107,
        ContactUserFieldDeleted = 3108,
        CompanyUserFieldCreated = 3109,
        CompanyUserFieldUpdated = 3110,
        CompanyUserFieldsUpdatedOrder = 3111,
        CompanyUserFieldDeleted = 3112,
        PersonUserFieldCreated = 3113,
        PersonUserFieldUpdated = 3114,
        PersonUserFieldsUpdatedOrder = 3115,
        PersonUserFieldDeleted = 3116,
        OpportunityUserFieldCreated = 3117,
        OpportunityUserFieldUpdated = 3118,
        OpportunityUserFieldsUpdatedOrder = 3119,
        OpportunityUserFieldDeleted = 3120,
        CaseUserFieldCreated = 3121,
        CaseUserFieldUpdated = 3122,
        CaseUserFieldsUpdatedOrder = 3123,
        CaseUserFieldDeleted = 3124,

        HistoryEventCategoryCreated = 3125,
        HistoryEventCategoryUpdated = 3126,
        HistoryEventCategoryUpdatedIcon = 3127,
        HistoryEventCategoriesUpdatedOrder = 3128,
        HistoryEventCategoryDeleted = 3129,

        CrmTaskCategoryCreated = 3130,
        CrmTaskCategoryUpdated = 3131,
        CrmTaskCategoryUpdatedIcon = 3132,
        CrmTaskCategoriesUpdatedOrder = 3133,
        CrmTaskCategoryDeleted = 3134,

        OpportunityStageCreated = 3135,
        OpportunityStageUpdated = 3136,
        OpportunityStageUpdatedColor = 3137,
        OpportunityStagesUpdatedOrder = 3138,
        OpportunityStageDeleted = 3139,

        ContactsCreatedTag = 3140,
        ContactsDeletedTag = 3141,
        OpportunitiesCreatedTag = 3142,
        OpportunitiesDeletedTag = 3143,
        CasesCreatedTag = 3144,
        CasesDeletedTag = 3145,
        ContactsTagSettingsUpdated = 3146,

        WebsiteContactFormUpdatedKey = 3147,

        ContactsImportedFromCSV = 3148,
        CrmTasksImportedFromCSV = 3149,
        OpportunitiesImportedFromCSV = 3150,
        CasesImportedFromCSV = 3151,

        ContactsExportedToCsv = 3152,
        CrmTasksExportedToCsv = 3153,
        OpportunitiesExportedToCsv = 3154,
        CasesExportedToCsv = 3155,

        #endregion

        #region People

        UserCreated = 4000,
        GuestCreated = 4001,
        UserCreatedViaInvite = 4002,
        GuestCreatedViaInvite = 4003,

        UserActivated = 4004,
        GuestActivated = 4005,

        UserUpdated = 4006,
        UserUpdatedMobileNumber = 4029,
        UserUpdatedLanguage = 4007,
        UserAddedAvatar = 4008,
        UserDeletedAvatar = 4009,
        UserUpdatedAvatarThumbnails = 4010,

        UserLinkedSocialAccount = 4011,
        UserUnlinkedSocialAccount = 4012,

        UserConnectedTfaApp = 4032,
        UserDisconnectedTfaApp = 4033,

        UserSentActivationInstructions = 4013,
        UserSentEmailChangeInstructions = 4014,
        UserSentPasswordChangeInstructions = 4015,
        UserSentDeleteInstructions = 4016,

        UserUpdatedEmail = 5047,
        UserUpdatedPassword = 4017,
        UserDeleted = 4018,

        UsersUpdatedType = 4019,
        UsersUpdatedStatus = 4020,
        UsersSentActivationInstructions = 4021,
        UsersDeleted = 4022,
        SentInviteInstructions = 4023,

        UserImported = 4024,
        GuestImported = 4025,

        GroupCreated = 4026,
        GroupUpdated = 4027,
        GroupDeleted = 4028,

        UserDataReassigns = 4030,
        UserDataRemoving = 4031,

        #endregion

        #region Documents

        FileCreated = 5000,
        FileRenamed = 5001,
        FileUpdated = 5002,
        UserFileUpdated = 5034,
        FileCreatedVersion = 5003,
        FileDeletedVersion = 5004,
        FileRestoreVersion = 5044,
        FileUpdatedRevisionComment = 5005,
        FileLocked = 5006,
        FileUnlocked = 5007,
        FileUpdatedAccess = 5008,
        FileSendAccessLink = 5036, // not used

        FileDownloaded = 5009,
        FileDownloadedAs = 5010,

        FileUploaded = 5011,
        FileImported = 5012,

        FileCopied = 5013,
        FileCopiedWithOverwriting = 5014,
        FileMoved = 5015,
        FileMovedWithOverwriting = 5016,
        FileMovedToTrash = 5017,
        FileDeleted = 5018, // not used

        FolderCreated = 5019,
        FolderRenamed = 5020,
        FolderUpdatedAccess = 5021,

        FolderCopied = 5022,
        FolderCopiedWithOverwriting = 5023,
        FolderMoved = 5024,
        FolderMovedWithOverwriting = 5025,
        FolderMovedToTrash = 5026,
        FolderDeleted = 5027, // not used

        ThirdPartyCreated = 5028,
        ThirdPartyUpdated = 5029,
        ThirdPartyDeleted = 5030,

        DocumentsThirdPartySettingsUpdated = 5031,
        DocumentsOverwritingSettingsUpdated = 5032,
        DocumentsForcesave = 5049,
        DocumentsStoreForcesave = 5048,
        DocumentsUploadingFormatsSettingsUpdated = 5033,

        FileConverted = 5035,

        FileChangeOwner = 5043,

        DocumentSignComplete = 5046,
        DocumentSendToSign = 5045,

        #endregion

        #region Settings

        LanguageSettingsUpdated = 6000,
        TimeZoneSettingsUpdated = 6001,
        DnsSettingsUpdated = 6002,
        TrustedMailDomainSettingsUpdated = 6003,
        PasswordStrengthSettingsUpdated = 6004,
        TwoFactorAuthenticationSettingsUpdated = 6005, // deprecated - use 6036-6038 instead
        AdministratorMessageSettingsUpdated = 6006,
        DefaultStartPageSettingsUpdated = 6007,

        ProductsListUpdated = 6008,

        AdministratorAdded = 6009,
        AdministratorOpenedFullAccess = 6010,
        AdministratorDeleted = 6011,

        UsersOpenedProductAccess = 6012,
        GroupsOpenedProductAccess = 6013,

        ProductAccessOpened = 6014,
        ProductAccessRestricted = 6015, // not used

        ProductAddedAdministrator = 6016,
        ProductDeletedAdministrator = 6017,

        GreetingSettingsUpdated = 6018,
        TeamTemplateChanged = 6019,
        ColorThemeChanged = 6020,

        OwnerSentChangeOwnerInstructions = 6021,
        OwnerUpdated = 6022,

        OwnerSentPortalDeactivationInstructions = 6023,
        OwnerSentPortalDeleteInstructions = 6024,

        PortalDeactivated = 6025,
        PortalDeleted = 6026,

        LoginHistoryReportDownloaded = 6027,
        AuditTrailReportDownloaded = 6028,

        SSOEnabled = 6029,
        SSODisabled = 6030,

        PortalAccessSettingsUpdated = 6031,

        CookieSettingsUpdated = 6032,
        MailServiceSettingsUpdated = 6033,

        CustomNavigationSettingsUpdated = 6034,

        AuditSettingsUpdated = 6035,

        TwoFactorAuthenticationDisabled = 6036,
        TwoFactorAuthenticationEnabledBySms = 6037,
        TwoFactorAuthenticationEnabledByTfaApp = 6038,

        DocumentServiceLocationSetting = 5037,
        AuthorizationKeysSetting = 5038,
        FullTextSearchSetting = 5039,

        StartTransferSetting = 5040,
        StartBackupSetting = 5041,

        LicenseKeyUploaded = 5042,

        StartStorageEncryption = 5050,

        PrivacyRoomEnable = 5051,
        PrivacyRoomDisable = 5052,

        StartStorageDecryption = 5053, // last

        #endregion

        #region others

        ContactAdminMailSent = 7000,

        #endregion

        #region Partners

        AcceptRequest = 8000,
        RejectRequest = 8001,
        BlockPartner = 8002,
        UnblockPartner = 8003,
        DeletePartner = 8004,
        ChangePartner = 8005,
        ConfirmPortal = 8006,
        MarkInvoicePaid = 8007,
        MarkInvoiceUnpaid = 8008,
        AddHostedPartner = 8009,
        RemoveHostedPartner = 8010,
        MarkPartnerAuthorized = 8011,
        MarkPartnerNotAuthorized = 8012,
        ChangePartnerLevel = 8013,
        ChangeHostedPartnerQuotas = 8014,
        ChangeHostedPartner = 8015,
        BillLumpSumInvoice = 8016,

        #endregion

    }
}