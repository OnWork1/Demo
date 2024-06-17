using BMW.IntegrationService.CrmGenerated;
using System.Collections.Generic;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Containers
{
    public class additionalContactExportContainer
    {
        public bmw_dealer Dealer { get; set; }
        public Contact CurrentContact { get; set; }
        public bmw_foreignid CurrentAdditionalContact { get; set; }
        public Account CurrentAccount { get; set; }
        public bmw_foreignid CurrentAdditionalAccount { get; set; }
        public ICollection<bmw_communicationconsent> Consents { get; set; }

        //JP
        public bmw_dealer notifiedDealer { get; set; }

        public bmw_preferreddealer CurrentPDealer { get; set; }
        public bmw_preferreddealer[] OldPDealer { get; set; }

        public List<DealerClass> DealerClassList { get; set; }

        public List<CustomerInfoJP> customerInfo { get; set; }

    }

    public class DealerClass
    {
        public bmw_dealer NewDealer { get; set; }
        public List<bmw_dealer> oldDealerList { get; set; }
    }


    public class CustomerInfoJP
    {
        public Contact customer { get; set; }
        public List<bmw_dealer> preferDealer { get; set; }
    }
    //public class PreferredDealersExportContainerJP {

    //    public bmw_preferreddealer Dealer { get; set; }
    //    public bmw_preferreddealer OldDealer { get; set; }
    //    public Contact CurrentContact { get; set; } 
    //}
}
