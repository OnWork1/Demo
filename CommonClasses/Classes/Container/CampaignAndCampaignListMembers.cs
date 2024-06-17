using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
   public class CampaignAndCampaignListMembers
    {
        public Campaign Campaign { get; set; }
        public List<CampaignListDetails> CampaignListDetails { get; set; }
    }

    public class CampaignListDetails
    {
        public Contact CampaigListMembers { get; set; }
        public Lead leadRequest { get; set; }
        public Guid campaignListMemberGuid { get; set; }
        public string Vin_Number { get; set; }
        public string ServiceCenter { get; set; }
        public string ServiceCaseId { get; set; }
    }
}
