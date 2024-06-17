using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class ACSDataImportContainer
    {

        public string TypeOfAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string HouseNumber { get; set; }
        public string StreetAddress { get; set; }
        public string StreetAddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string CompanyName { get; set; }
        public string NoForwardToDealerFlag { get; set; }
        public string Deceased { get; set; }
        public string NoContactFlag { get; set; }
        public string NoCampaignFlag { get; set; }
        public string JobTitle { get; set; }
        public string NoSMSFlag { get; set; }
        public string NoSurveyFlag { get; set; }
        public string Title { get; set; }
        public string NoEmailFlag { get; set; }
        public string NoFaxFlag { get; set; }
        public string NoPhoneFlag { get; set; }
        public string ContactID { get; set; }


        public string NoMailFlag { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public string MobilePhone { get; set; }
        public string Phone { get; set; }
        public string Salutation { get; set; }
        public string SourceChannel { get; set; }
        public string LegalContext { get; set; }
        public string Email2 { get; set; }
        public string Email3 { get; set; }
        public string BusinessMobile { get; set; }
        public string BusinessPhone { get; set; }
        public string HomePhone { get; set; }
        public string Birthday { get; set; }


    }
    public class ACSLeadImportContainer
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string mobilephone { get; set; }
        public string emailaddress1 { get; set; }
        public string sourcechanneldetail { get; set; }
        public string Type { get; set; }
        public string SalesChannel { get; set; }
        public string LegalContext { get; set; }
        public string SourceChannel { get; set; }
        public string BusinessLine { get; set; }
        public string Brand { get; set; }
        public string Series { get; set; }
        public string Model { get; set; }
        public string VehicleofInterest { get; set; }
        public string Dealer { get; set; }
        public string MarketingMaterial { get; set; }
        public string StopPostal { get; set; }
        public string StopPhone { get; set; }
        public string StopFax { get; set; }
        public string StopEmail { get; set; }
        public string DoNotBulkEmail { get; set; }
        public string StopSMS { get; set; }
        public string StopMobilePhone { get; set; }
        public string StopHomePhone { get; set; }
        public string StopBusinessPhone { get; set; }
        public string MigrationID { get; set; }
        public string Validity { get; set; }
        public string Temperature { get; set; }
        public string SendToDealer { get; set; }
        public string VGModelCode { get; set; }
        public string AG_Code { get; set; }
        public string Preferred_TD_Date { get; set; }
        public string Preferred_TD_Time { get; set; }
        public string PreferredContact { get; set; }    

    }

    public class ACSConsentImportContainer
    {
        public string Brand { get; set; }
        public string GeneralContactAccepted { get; set; }
        public string BusinessPhoneAccepted { get; set; }
       
        public string FaxAccepted { get; set; }
        public string HomePhoneAccepted { get; set; }
        public string MobileEmailAccepted { get; set; }
        public string MobilePhoneAccepted { get; set; }
        public string ParentAccount { get; set; }
        public string ParentContact {get;set ;}
        public string PrimaryEmailAccepted { get; set; }
        public string CommunicationConsentId { get; set; }
        public string IncludeinCampaignsAccepted { get; set; }
        public string EmailAccepted { get; set; }
        public string PhoneAccepted { get; set; }
        public string PostalMailAccepted { get; set; }
        public string SMSAccepted { get; set; }
        public string SurveyAccepted { get; set; }
        public string MandatoryPrivacySalesAccepted { get; set; }
        public string MandatoryPrivacyNonSalesAccepted { get; set; }
        public string ThirdPartyAgreementAccepted { get; set; }
        
        public string OASAgreementAccepted { get; set; }
        public string VINAgreementAccepted { get; set; }
        public string NewsletterAccepted { get; set; }
        public string ContactfromDealerAccepted { get; set; }
       
    }
}
