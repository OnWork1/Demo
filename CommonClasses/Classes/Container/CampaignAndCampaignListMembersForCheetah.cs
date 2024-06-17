using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class CampaignAndCampaignListMembersForCheetah
    {
        public Campaign Campaign { get; set; }
        public List<Contact> listMemberContacts { get; set; }
        public bool? Status { get; set; } = true;
        public string Reason { get; set; }
    }
}
