using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class FilteredDealerContainer
    {
        public bmw_dealer Dealer { get; set; }
        public XElement AssociatedDealerElement { get; set; }

        public FilteredDealerContainer(bmw_dealer dealer, XElement associatedDealerElement)
        {
            this.Dealer = dealer;
            this.AssociatedDealerElement = associatedDealerElement;
        }
    }
}
