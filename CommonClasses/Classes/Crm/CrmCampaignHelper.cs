using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
    public static class CrmCampaignHelper
    {
        #region GetCampaignByBmwIdOrName(IOrganizationService service, string sourceCampaign)
        public static Guid? GetCampaignByBmwIdOrName(IOrganizationService service, string sourceCampaign)
        {
            if (String.IsNullOrEmpty(sourceCampaign))
                return null;

            ConditionExpression stateCondition = new ConditionExpression("statecode", ConditionOperator.Equal, (int)CampaignState.Active);

            QueryExpression query = new QueryExpression(Campaign.EntityLogicalName)
            {
                NoLock = true,
                PageInfo = new PagingInfo { Count = 1, PageNumber = 1 },
                ColumnSet = new ColumnSet()
            };

            Guid guid = Guid.Empty;
            query.Criteria.AddCondition(stateCondition);
            int campaignId;

            EntityCollection entityCollection;

            if (Int32.TryParse(sourceCampaign, out campaignId))
            {
                query.Criteria.AddCondition("bmw_campaignid", ConditionOperator.Equal, campaignId);
                entityCollection = service.RetrieveMultiple(query);
                if (entityCollection != null && entityCollection.Entities != null)
                {
                    Entity entity = entityCollection.Entities.FirstOrDefault();
                    if (entity != null) return entity.Id;
                }
            }
            else if (Guid.TryParse(sourceCampaign, out guid))
            {
                return new Guid(sourceCampaign);
            }
            else
            {
                query.Criteria.AddCondition("bmw_migrationid", ConditionOperator.Equal, sourceCampaign);
                entityCollection = service.RetrieveMultiple(query);
                if (entityCollection != null && entityCollection.Entities != null)
                {
                    Entity entity = entityCollection.Entities.FirstOrDefault();
                    if (entity != null) return entity.Id;
                }
            }

            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(stateCondition);
            query.Criteria.AddCondition("name", ConditionOperator.Equal, sourceCampaign);

            entityCollection = service.RetrieveMultiple(query);
            if (entityCollection != null && entityCollection.Entities != null)
            {
                Entity entity = entityCollection.Entities.FirstOrDefault();
                if (entity != null) return entity.Id;
            }
            return null;
        }
        #endregion

    }
}
