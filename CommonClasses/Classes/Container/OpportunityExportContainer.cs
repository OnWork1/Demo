using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
   public class OpportunityExportContainer
    {
        public bmw_dealer Dealer { get; set; }
        public Opportunity CurrentOpportunity { get; set; }
        public Lead[] LeadsList { get; set; }
        public bmw_leadactivity[] LeadActivities { get; set; }
        public Contact CurrentContact { get; set; }
        public Product[] OpportunityVehicles { get; set; }
        public Annotation ConfigurationAttachment { get; set; }
        public ICollection<Annotation> OpportunityAttachments { get; set; }
        public ICollection<OwnedVehicleContainer> OwnedVehicles { get; set; }
        public ICollection<bmw_action> LeadActivityActions { get; set; }
        public ICollection<bmw_validresponse> LeadActivityResponses { get; set; }
        public ICollection<bmw_vehicleoption>[] VehicleOptions { get; set; }
        public bmw_appraisal TradeInVehicle { get; set; }
        public ICollection<bmw_communicationconsent> CommunicationConsents { get; set; }

        public ICollection<bmw_consent> Consents { get; set; }
    }
}
