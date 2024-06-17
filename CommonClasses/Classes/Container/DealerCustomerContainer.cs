using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class DealerCustomerContainer
    {
        public bmw_dealer Dealer { get; set; }
        public bmw_preferreddealer preferreddealer { get; set; }
        public List<ContactDetails> ContactDetails { get; set; }
    }

    public class ContactDetails
    {
        public Contact Contacts { get; set; }
        public List<bmw_hobby> hobbies { get; set; }
        public List<bmw_vehicle> vehicle { get; set; }
    }

    public class DealerCustomer
    {
        public Contact contact { get; set; }
        public bmw_dealer dealer { get; set; }
        public bmw_preferreddealer preferredDealer { get; set; }
    }
}
