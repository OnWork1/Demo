using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Sdk;
//using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;
using BMW.IntegrationService.CrmGenerated;


namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
    public class CrmDealerHelper
    {
        private const char DealerNumberSeparator = '_';

        private readonly CrmServiceContext CrmServiceContext;
        private readonly Dictionary<string, bmw_dealer> dealerCache;

        // Constructors

        #region CrmDealerHelper(CrmServiceContext CrmServiceContext/*, OperationResult operationResult*/)
        public CrmDealerHelper(CrmServiceContext CrmServiceContext/*, OperationResult operationResult*/)
        {
            if (CrmServiceContext == null)
            {
                throw new ArgumentNullException("CrmServiceContext");
            }

            this.CrmServiceContext = CrmServiceContext;
            this.dealerCache = new Dictionary<string, bmw_dealer>();
        }
        #endregion

        // Private methods
        #region GetDealerFromCache(string dealerNumber)
        private bmw_dealer GetDealerFromCache(string dealerNumber)
        {
            if (String.IsNullOrEmpty(dealerNumber))
                return null;

            if (this.dealerCache == null)
                return null;

            return this.dealerCache.ContainsKey(dealerNumber) ? this.dealerCache[dealerNumber] : null;
        }
        #endregion

        #region AddDealerToCache(string dealerNumber, bmw_dealer dealer)
        private void AddDealerToCache(string dealerNumber, bmw_dealer dealer)
        {
            if (String.IsNullOrEmpty(dealerNumber) || dealer == null || this.dealerCache == null)
                return;

            if (this.dealerCache.ContainsKey(dealerNumber))
                return;

            this.dealerCache.Add(dealerNumber, dealer);
        }
        #endregion

        #region GetWsnscCDealer(string opportunityIds/*, OperationResult result*/)
        private bmw_dealer GetWsnscCDealer(string opportunityIds, IOrganizationService CrmService)
        {
            string dealerNumber = CommonClass.GetCrmParameterValue<String>("WSNSCDealerPIXID");

            if (String.IsNullOrEmpty(dealerNumber))
            {
              
                return null;
            }

            return this.GetDealerByDealerNumber(dealerNumber, opportunityIds/*, result*/);
        }
        #endregion

        #region GetDealerByCentralId(XElement opportunity, string opportunityIds/*, OperationResult result*/)
        private bmw_dealer GetDealerByCentralId(XElement opportunity, string opportunityIds)
        {
            if (opportunity == null || opportunity.Parent == null)
                return null;

            XElement xBuno = opportunity.Parent.XPathSelectElement("CentralDealerID");
            if (xBuno == null)
            {
               
                return null;
            }

            return this.GetDealerByDealerNumber(xBuno.Value, opportunityIds/*, result*/);
        }

        #endregion

        #region GetDealerByLocalId(XElement opportunity, string opportunityIds/*, OperationResult result*/)
        private bmw_dealer GetDealerByLocalId(XElement opportunity, string opportunityIds)
        {
            if (opportunity == null || opportunity.Parent == null)
                return null;

            XElement xBuno = opportunity.Parent.XPathSelectElement("LocalDealerID");
            if (xBuno == null)
            {               
                return null;
            }

            return this.GetDealerByDealerNumber(xBuno.Value, opportunityIds/*, result*/);
        }
        #endregion

        #region GetDealerByDealerNumber(string dealerNumber, string opportunityIds/*, OperationResult result*/)
        private bmw_dealer GetDealerByDealerNumber(string dealerNumber, string opportunityIds)
        {
            bmw_dealer dealer =
                (from d in this.CrmServiceContext.bmw_dealerSet where d.bmw_dealernumber == dealerNumber select d).FirstOrDefault();

            if (dealer == null)
            {
                return null;
            }
            return dealer;
        }
        #endregion

        #region GetDealerByNationalId(XElement opportunity, string opportunityIds, string dealerIdPath/*, OperationResult result*/)
        private bmw_dealer GetDealerByNationalId(XElement opportunity, string opportunityIds, string dealerIdPath)
        {
            if (opportunity == null || opportunity.Parent == null)
                return null;

            XElement xDealerId = opportunity.Parent.XPathSelectElement(dealerIdPath);
            if (xDealerId == null)
            {
                return null;
            }

            bmw_dealer dealer = (from d in this.CrmServiceContext.bmw_dealerSet where d.bmw_nationaldealerid == xDealerId.Value select d).FirstOrDefault();
            if (dealer == null)
            {
                return null;
            }
            return dealer;
        }
        #endregion

        #region GetDealerByDistributionPartnerAndOutletId(string dealerNumber/*, OperationResult result*/)
        private bmw_dealer GetDealerByDistributionPartnerAndOutletId(string dealerNumber/*, OperationResult result*/)
        {
            string[] splitted = dealerNumber.Split(CrmDealerHelper.DealerNumberSeparator);

            if (splitted.Length != 2)
                return null;

            string distributionPartnerId = splitted[0];
            string outletId = splitted[1];

            if (String.IsNullOrEmpty(distributionPartnerId) || String.IsNullOrEmpty(outletId))
            {                
                return null;
            }

            return (from d in this.CrmServiceContext.bmw_dealerSet where d.bmw_distributionpartnerid == distributionPartnerId && d.bmw_outletid == outletId select d).FirstOrDefault();
        }
        #endregion

        // Public methods
        #region GetDealer(string dealerNumber, bool findByNationalId = false)
        public bmw_dealer GetDealer(string dealerNumber, bool findByNationalId = false, bool findByOutletId = false)
        {
            if (String.IsNullOrWhiteSpace(dealerNumber))
                return null;

            bmw_dealer dealer = this.GetDealerFromCache(dealerNumber);

            if (dealer != null)
                return dealer;

            dealer = findByNationalId ?
                this.CrmServiceContext.bmw_dealerSet.FirstOrDefault(d => d.bmw_nationaldealerid == dealerNumber) :
                this.CrmServiceContext.bmw_dealerSet.FirstOrDefault(d => d.bmw_dealernumber == dealerNumber);

            dealer = findByOutletId ?
                this.CrmServiceContext.bmw_dealerSet.FirstOrDefault(d => d.bmw_outletid == dealerNumber) : dealer;

            if (dealer == null)
                return null;

            this.AddDealerToCache(dealerNumber, dealer);
            return dealer;
        }
        #endregion

        
        #region GetDealerFromOpportunity(XElement opportunity, string dealerIdPath, string opportunityIds, OperationResult result, bool useWsnscDealerIfEmpty = true, bool useNationalDealerIds = false)
        public bmw_dealer GetDealerFromOpportunity(XElement opportunity, string dealerIdPath, string opportunityIds, IOrganizationService CrmService/*, OperationResult result*/, bool useWsnscDealerIfEmpty = true, bool useNationalDealerIds = false)
        {
            bmw_dealer dealer = null;

            if (dealerIdPath == null && !useWsnscDealerIfEmpty)
                dealer = this.GetDealerByCentralId(opportunity, opportunityIds/*, result*/);

            if (String.IsNullOrEmpty(dealerIdPath) && useWsnscDealerIfEmpty)
                dealer = this.GetWsnscCDealer(opportunityIds/*, result*/,CrmService);

            if ("CentralDealerID".Equals(dealerIdPath, StringComparison.InvariantCultureIgnoreCase))
                dealer = useNationalDealerIds ? this.GetDealerByNationalId(opportunity, opportunityIds, dealerIdPath/*, result*/) : this.GetDealerByCentralId(opportunity, opportunityIds/*, result*/);

            if ("LocalDealerID".Equals(dealerIdPath, StringComparison.InvariantCultureIgnoreCase))
                dealer = useNationalDealerIds ? this.GetDealerByNationalId(opportunity, opportunityIds, dealerIdPath/*, result*/) : this.GetDealerByLocalId(opportunity, opportunityIds/*, result*/);

            return dealer;
        }
        #endregion


        #region GetDealerForEcom(string dealerNumber, OperationResult result)
        public bmw_dealer GetDealerForEcom(string dealerNumber/*, OperationResult result*/)
        {
            if (String.IsNullOrEmpty(dealerNumber))
                return null;

            if (dealerNumber.Contains(CrmDealerHelper.DealerNumberSeparator))
                return this.GetDealerByDistributionPartnerAndOutletId(dealerNumber/*, result*/);

            //get only first digits sequesnce from dealer number (trimming whitespaces, letters, etc.)
            Regex numberRegex = new Regex("\\d+");
            Match numberMatch = numberRegex.Match(dealerNumber);
            dealerNumber = numberMatch.Value;
            return this.GetDealer(dealerNumber);
        }
        #endregion
    }
}
