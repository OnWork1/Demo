using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;
//using Webcom.IntegrationService.CommonClassesAndEnums.Classes.Logging;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
    public class CrmLicsManager
    {
        private static CrmLicsManager CurrentInstance;
        private static readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private const string LicSkip = "#SKIP#";

        private readonly Dictionary<string, Dictionary<string, List<bmw_lic>>> organizationLics;

        // Constructors

        #region CrmLicsManager()
        private CrmLicsManager()
        {
            this.organizationLics = new Dictionary<string, Dictionary<string, List<bmw_lic>>>();
        }
        #endregion

        // Public properties

        #region Instance
        public static CrmLicsManager Instance
        {
            get
            {
                if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
                    readerWriterLock.EnterUpgradeableReadLock();

                if (CrmLicsManager.CurrentInstance == null)
                {
                    if (!readerWriterLock.IsWriteLockHeld)
                        readerWriterLock.EnterWriteLock();
                    try
                    {
                        if (CrmLicsManager.CurrentInstance == null)
                        {
                            CrmLicsManager.CurrentInstance = new CrmLicsManager();
                        }
                    }
                    finally
                    {
                        if (readerWriterLock.IsWriteLockHeld)
                            readerWriterLock.ExitWriteLock();
                    }
                }
                if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
                return CrmLicsManager.CurrentInstance;
            }
        }
        #endregion

        // Private methods

        #region CacheCrmLics(string url)
        private void CacheCrmLics(string url, bmw_interfacerun interfaceRun, CrmServiceContext CrmServiceContext)
        {
            if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
                readerWriterLock.EnterUpgradeableReadLock();

            var lovDictionary = new Dictionary<string, List<bmw_lic>>();

            foreach (var lov in CrmServiceContext.bmw_lovSet)
            {
                try
                {
                    var lics = CrmServiceContext.bmw_licSet.Where(p => p.bmw_lov.Id == lov.Id).ToList();
                    if (lovDictionary.ContainsKey(lov.bmw_name))
                    {
                        continue;
                    }
                    if (lics.Count > 0)
                        lovDictionary.Add(lov.bmw_name, lics);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            this.AddLicSet(url, lovDictionary, interfaceRun);

            CrmServiceContext.Dispose();

            if (readerWriterLock.IsUpgradeableReadLockHeld)
                readerWriterLock.ExitUpgradeableReadLock();
        }
        #endregion

        #region AddLicSet(string organizationCrmUrlWillBeUsedAsKey, Dictionary<string, List<bmw_lic>> lic)
        private void AddLicSet(string organizationCrmUrlWillBeUsedAsKey, Dictionary<string, List<bmw_lic>> lic,
            bmw_interfacerun interfaceRun)
        {
            if (!readerWriterLock.IsWriteLockHeld)
                readerWriterLock.EnterWriteLock();

            try
            {
                if (this.organizationLics.ContainsKey(organizationCrmUrlWillBeUsedAsKey))
                    return;

                this.organizationLics.Add(organizationCrmUrlWillBeUsedAsKey, lic);

            }
            finally
            {
                if (readerWriterLock.IsWriteLockHeld)
                    readerWriterLock.ExitWriteLock();
            }
        }
        #endregion

        #region LicsAlreadyDownloaded(string url)
        private bool LicsAlreadyDownloaded(string url)
        {
            return this.organizationLics.ContainsKey(url);
        }
        #endregion

        #region GetLicCollection(string crmOrganizationServiceUrl, string lovName)
        private IEnumerable<bmw_lic> GetLicCollection(string crmOrganizationServiceUrl, string lovName,
            bmw_interfacerun interfaceRun)
        {
            if (!this.organizationLics.ContainsKey(crmOrganizationServiceUrl))
                return null;

            Dictionary<string, List<bmw_lic>> lovs = this.organizationLics[crmOrganizationServiceUrl];

            return lovs.ContainsKey(lovName) ? lovs[lovName] : null;
        }
        #endregion

        // Public methods

        #region RefreshLics(string crmUrl)
        public void RefreshLics(string crmUrl)
        {
            if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
                readerWriterLock.EnterUpgradeableReadLock();

            if (this.organizationLics.ContainsKey(crmUrl))
            {
                if (!readerWriterLock.IsWriteLockHeld)
                    readerWriterLock.EnterWriteLock();

                try
                {
                    this.organizationLics.Remove(crmUrl);
                }
                finally
                {
                    if (readerWriterLock.IsWriteLockHeld)
                        readerWriterLock.ExitWriteLock();
                }

            }
            if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
        }
        #endregion

        // Public methods

        #region GetLicValue(string crmOrganizationServiceUrl, string lovName, string licName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crmOrganizationServiceUrl"></param>
        /// <param name="lovName"></param>
        /// <param name="licName"></param>
        /// <param name="interfaceRun"></param>
        /// <returns></returns>
        public string GetLicValue(string crmOrganizationServiceUrl, string lovName, string licName, bmw_interfacerun interfaceRun, CrmServiceContext CrmServiceContext)
        {
            licName = String.IsNullOrEmpty(licName) ? licName : licName.Trim();

            if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
                readerWriterLock.EnterUpgradeableReadLock();

            if (!this.LicsAlreadyDownloaded(crmOrganizationServiceUrl))
            {
                if (!readerWriterLock.IsWriteLockHeld)
                    readerWriterLock.EnterWriteLock();

                if (!this.LicsAlreadyDownloaded(crmOrganizationServiceUrl))
                {
                    try
                    {
                        this.CacheCrmLics(crmOrganizationServiceUrl, interfaceRun, CrmServiceContext);
                    }
                    finally
                    {
                        if (readerWriterLock.IsWriteLockHeld)
                            readerWriterLock.ExitWriteLock();
                    }
                }
                else
                {
                    if (readerWriterLock.IsWriteLockHeld)
                        readerWriterLock.ExitWriteLock();
                }
            }

            IEnumerable<bmw_lic> licCollection = this.GetLicCollection(crmOrganizationServiceUrl, lovName, interfaceRun);

            if (licCollection == null)
            {
                if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
                return String.Empty;
            }

            string lic = licCollection.Where(p => !String.IsNullOrEmpty(p.bmw_name) && p.bmw_name.Equals(licName, StringComparison.InvariantCultureIgnoreCase)).Select(p => p.bmw_value).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(lic) || lic == LicSkip)
            {
                if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
                return String.Empty;
            }
            if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
            return lic;
        }
        #endregion

        #region GetLicName(string crmOrganizationServiceUrl, string lovName, string licValue)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crmOrganizationServiceUrl"></param>
        /// <param name="lovName"></param>		
        /// <param name="licValue"> </param>
        /// <returns></returns>
        public string GetLicName(string crmOrganizationServiceUrl, string lovName, string licValue, bmw_interfacerun interfaceRun, CrmServiceContext CrmServiceContext)
        {
            if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
                readerWriterLock.EnterUpgradeableReadLock();

            if (!this.LicsAlreadyDownloaded(crmOrganizationServiceUrl))
            {
                if (!readerWriterLock.IsWriteLockHeld)
                    readerWriterLock.EnterWriteLock();
                try
                {
                    this.CacheCrmLics(crmOrganizationServiceUrl, interfaceRun, CrmServiceContext);
                }
                finally
                {
                    if (readerWriterLock.IsWriteLockHeld)
                        readerWriterLock.ExitWriteLock();
                }
            }

            IEnumerable<bmw_lic> licCollection = this.GetLicCollection(crmOrganizationServiceUrl, lovName, interfaceRun);

            if (licCollection == null)
            {
                if (readerWriterLock.IsUpgradeableReadLockHeld)
                    readerWriterLock.ExitUpgradeableReadLock();
                return String.Empty;
            }
            string lic = licCollection.Where(p => p.bmw_value == licValue).Select(p => p.bmw_name).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(lic) || lic == LicSkip)
            {
                if (readerWriterLock.IsUpgradeableReadLockHeld)
                    readerWriterLock.ExitUpgradeableReadLock();
                return String.Empty;
            }
            if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
            return lic;
        }
        #endregion
    }
}
