using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class DealerAndDealerContractContainer
    {
        public bmw_dealer Dealer { get; set; }
        public string dealerName { get; set; }
        public string additionalName { get; set; }
        public string nickname { get; set; }
        public string dealerId { get; set; }
        public string nationalDealerId { get; set; }
        public List<bmw_dealercontracttype> dealercontracttypeList { get; set; }
    }
}
