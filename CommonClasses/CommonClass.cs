using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Exceptions;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.XmlXsdValidation;
using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Microsoft.WindowsAzure.Storage.File;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging;
using System.Security.Cryptography;
using System.Globalization;
using Renci.SshNet;
using Microsoft.Xrm.Sdk.Metadata;
using Renci.SshNet.Sftp;
using System.Net.Http;
using System.Collections.Concurrent;
using Microsoft.Xrm.Tooling.Connector;
using System.Text.RegularExpressions;

namespace BMW.IntegrationService
{
    public class CommonClass
    {
        private static string encrptkeyPrivate;
        private static string Key64Private;
        private static string Value64Private;
        private static string ServiceBusName;
        private static string crmOrgUrl;
        private static string azureStorageAccountConnectionString;
        private static string outputFileShare;
        private static string queueConnectionString;
        private static string clientSecret;
        private static string XMLValidationLogicApp;
        private static ConcurrentDictionary<Guid, string> SecretKeyConDictionary = new ConcurrentDictionary<Guid, string>();

        public static string encrptkey
        {
            get
            {
                if (string.IsNullOrEmpty(encrptkeyPrivate))
                {
                    encrptkeyPrivate = ConfigurationManager.AppSettings["ClientId"];
                }
                return encrptkeyPrivate;
            }
        }
        public static string Key64
        {
            get
            {
                if (string.IsNullOrEmpty(Key64Private))
                {
                    Key64Private = ConfigurationManager.AppSettings["AESKey"];
                }
                return Key64Private;
            }
        }
        public static string Value64
        {
            get
            {
                if (string.IsNullOrEmpty(Value64Private))
                {
                    Value64Private = ConfigurationManager.AppSettings["AESIV"];
                }
                return Value64Private;
            }
        }

        public static string ServiceBus
        {
            get
            {
                if (string.IsNullOrEmpty(ServiceBusName))
                {
                    ServiceBusName = ConfigurationManager.AppSettings["ServiceBusNameSpaceName"];
                }
                return ServiceBusName;
            }
        }
        public static string CrmOrgUrl
        {
            get
            {
                if (string.IsNullOrEmpty(crmOrgUrl))
                {
                    crmOrgUrl = ConfigurationManager.AppSettings["CRMOrgurl"];
                }
                return crmOrgUrl;
            }
        }

        public static string ClientSecret
        {
            get
            {
                if (string.IsNullOrEmpty(clientSecret))
                {
                    clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                }
                return clientSecret;
            }
        }
        public static string XMLValidationLogicAppUri
        {
            get
            {
                if (string.IsNullOrEmpty(XMLValidationLogicApp))
                {
                    XMLValidationLogicApp = ConfigurationManager.AppSettings["XMLValidationLogicAppUri"];
                }
                return XMLValidationLogicApp;
            }
        }

        public static string AzureStorageAccountConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(azureStorageAccountConnectionString))
                {
                    azureStorageAccountConnectionString = ConfigurationManager.AppSettings["AzureStorageAccountConnectionString"];
                }
                return azureStorageAccountConnectionString;
            }
        }

        public static string OutputFileShare
        {
            get
            {
                if (string.IsNullOrEmpty(outputFileShare))
                {
                    outputFileShare = ConfigurationManager.AppSettings["outputFileShare"];
                }
                return outputFileShare;
            }
        }

        public static string QueueConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(queueConnectionString))
                {
                    queueConnectionString = ConfigurationManager.AppSettings["SBNSSEM2CRMConnectionString"];
                }
                return queueConnectionString;
            }
        }

        public static DateTimeOffset tokenExpiresON;
        protected static CrmServiceContext CommonCrmServiceContext;
        private readonly string CrmUrl = string.Empty;
        protected static IOrganizationService CommoncrmService;
        private static HttpClient httpClient = new HttpClient();
        private static readonly string logicAppUri = XMLValidationLogicAppUri;
        protected static Logger Commonlogger = null;
        protected static TraceWriter CommonTraceLog;
        public static OrganizationWebProxyClient CRMConnect(TraceWriter TraceLog, Guid? owner)
        {
            try
            {
                CommonTraceLog = TraceLog;
                string resourceURL = CrmOrgUrl + "/api/data/v9.1/";
                string clientId = string.Empty;

                if (!string.IsNullOrEmpty(encrptkey) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
                {
                    byte[] encrypted = Convert.FromBase64String(encrptkey);
                    byte[] encryptedKey = Convert.FromBase64String(Key64);
                    byte[] encryptedValue = Convert.FromBase64String(Value64);

                    // Decrypt the bytes to a string.
                    clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
                }
                else
                {
                    CommonTraceLog.Error("encrptkey/Key64/Value64 is Empty");
                    return null;
                }

                string appKey = ClientSecret;

                //Create the Client credentials to pass for authentication
                ClientCredential clientcred = new ClientCredential(clientId, appKey);

                //get the authentication parameters
                AuthenticationParameters authParam = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(resourceURL)).Result;

                //Generate the authentication context - this is the azure login url specific to the tenant
                string authority = authParam.Authority;
                var authContext = new AuthenticationContext(authority);
                authContext.TokenCache.Clear(); // to restrict time out/connection issue.
                //request token
                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult = authContext.AcquireTokenAsync(CrmOrgUrl, clientcred).Result;
                //Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult = new AuthenticationContext(authority,null).AcquireTokenAsync(CrmOrgUrl, clientcred).Result;

                tokenExpiresON = authenticationResult.ExpiresOn;

                //get the token              
                string token = authenticationResult.AccessToken;


                OrganizationWebProxyClient sdkService = null;
                Uri serviceUrl = new Uri(CrmOrgUrl + @"/XRMServices/2011/Organization.svc/web?SdkClientVersion=9.1");
                if (!owner.HasValue)
                {
                    sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        CommoncrmService = sdkService;

                    }
                }
                else
                {
                    sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        sdkService.CallerId = owner.Value;
                        ///CommoncrmService = sdkService;
                    }

                }
                CommonCrmServiceContext = new CrmServiceContext(sdkService);
                Commonlogger = Logger.GetLoggerWithCrmLogging(sdkService, CommonTraceLog);

                return sdkService;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in connection:" + ex.Message + " Stack_Trace: " + ex.StackTrace + " Inner Exception: " + ex.InnerException?.Message);
                return null;
            }
        }
        public static string decryptSMSCredentials(string encryptedCredential)
        {
            string decryptedCredential=string.Empty;
            if (!string.IsNullOrEmpty(encryptedCredential) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
            {
                byte[] encrypted = Convert.FromBase64String(encryptedCredential);
                byte[] encryptedKey = Convert.FromBase64String(Key64);
                byte[] encryptedValue = Convert.FromBase64String(Value64);

                // Decrypt the bytes to a string.
                decryptedCredential = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
            }
            return decryptedCredential;
        }

        public static IOrganizationService CRMConnect(IOrganizationService crmService, TraceWriter traceLog, Guid? owner, int duration = 1)
        {
            try
            {
                CommonTraceLog = traceLog;
                string clientId = string.Empty;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                if (!string.IsNullOrEmpty(encrptkey) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
                {
                    byte[] encrypted = Convert.FromBase64String(encrptkey);
                    byte[] encryptedKey = Convert.FromBase64String(Key64);
                    byte[] encryptedValue = Convert.FromBase64String(Value64);

                    // Decrypt the bytes to a string.
                    clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
                }
                else
                {
                    CommonTraceLog.Error("encrptkey/Key64/Value64 is Empty");
                    return null;
                }

                object connectionLock = new object();
                if (crmService == null)
                {
                    lock (connectionLock)
                    {
                        if (crmService == null)
                        {
                            try
                            {
                                CrmServiceClient.MaxConnectionTimeout = new TimeSpan(duration, 0, 0);
                                var conn = new CrmServiceClient($@"AuthType=ClientSecret;url={CrmOrgUrl};ClientId={clientId};ClientSecret={ClientSecret}");
                                CommoncrmService = conn;
                                if (owner.HasValue)
                                {
                                    conn.CallerId = owner.Value;
                                }
                                CommonCrmServiceContext = new CrmServiceContext(conn);
                                Commonlogger = Logger.GetLoggerWithCrmLogging(conn, CommonTraceLog);
                                return conn;
                            }
                            catch (Exception ex)
                            {
                                traceLog.Error("Exception in creating connection with CRM!! " + ex.Message);
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    CommonCrmServiceContext = new CrmServiceContext(crmService);
                    Commonlogger = Logger.GetLoggerWithCrmLogging(crmService, CommonTraceLog);
                    return crmService;
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in connection:" + ex.Message + " Stack_Trace: " + ex.StackTrace + " Inner Exception: " + ex.InnerException?.Message);
                return null;
            }
            return null;
        }

        public static IOrganizationService CRMConnect(IOrganizationService crmService, TraceWriter traceLog, string CRMurl, Guid? owner, int duration = 1)
        {
            try
            {
                CommonTraceLog = traceLog;
                string clientId = string.Empty;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                if (!string.IsNullOrEmpty(encrptkey) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
                {
                    byte[] encrypted = Convert.FromBase64String(encrptkey);
                    byte[] encryptedKey = Convert.FromBase64String(Key64);
                    byte[] encryptedValue = Convert.FromBase64String(Value64);

                    // Decrypt the bytes to a string.
                    clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
                }
                else
                {
                    CommonTraceLog.Error("encrptkey/Key64/Value64 is Empty");
                    return null;
                }

                object connectionLock = new object();
                if (crmService == null)
                {
                    lock (connectionLock)
                    {
                        if (crmService == null)
                        {
                            try
                            {
                                CrmServiceClient.MaxConnectionTimeout = new TimeSpan(duration, 0, 0);
                                var conn = new CrmServiceClient($@"AuthType=ClientSecret;url={CRMurl};ClientId={clientId};ClientSecret={ClientSecret}");
                                CommoncrmService = conn;
                                if (owner.HasValue)
                                {
                                    conn.CallerId = owner.Value;
                                }
                                CommonCrmServiceContext = new CrmServiceContext(conn);
                                Commonlogger = Logger.GetLoggerWithCrmLogging(conn, CommonTraceLog);
                                return conn;
                            }
                            catch (Exception ex)
                            {
                                traceLog.Error("Exception in creating connection with CRM!! " + ex.Message);
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    CommonCrmServiceContext = new CrmServiceContext(crmService);
                    Commonlogger = Logger.GetLoggerWithCrmLogging(crmService, CommonTraceLog);
                    return crmService;
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in connection:" + ex.Message + " Stack_Trace: " + ex.StackTrace + " Inner Exception: " + ex.InnerException?.Message);
                return null;
            }
            return null;
        }

        public static IOrganizationService CRMConnect(IOrganizationService crmService, string ekey, string ClientSecrate, string orgurl, TraceWriter traceLog, Guid? owner, int duration = 1)
        {
            try
            {
                CommonTraceLog = traceLog;
                string clientId = string.Empty;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                if (!string.IsNullOrEmpty(ekey) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
                {
                    byte[] encrypted = Convert.FromBase64String(ekey);
                    byte[] encryptedKey = Convert.FromBase64String(Key64);
                    byte[] encryptedValue = Convert.FromBase64String(Value64);

                    // Decrypt the bytes to a string.
                    clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
                }
                else
                {
                    CommonTraceLog.Error("encrptkey/Key64/Value64 is Empty");
                    return null;
                }

                object connectionLock = new object();
                if (crmService == null)
                {
                    lock (connectionLock)
                    {
                        if (crmService == null)
                        {
                            try
                            {
                                CrmServiceClient.MaxConnectionTimeout = new TimeSpan(duration, 0, 0);
                                var conn = new CrmServiceClient($@"AuthType=ClientSecret;url={orgurl};ClientId={clientId};ClientSecret={ClientSecrate}");
                                CommoncrmService = conn;
                                if (owner.HasValue)
                                {
                                    conn.CallerId = owner.Value;
                                }
                                CommonCrmServiceContext = new CrmServiceContext(conn);
                                Commonlogger = Logger.GetLoggerWithCrmLogging(conn, CommonTraceLog);
                                return conn;
                            }
                            catch (Exception ex)
                            {
                                traceLog.Error("Exception in creating connection with CRM!! " + ex.Message);
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    CommonCrmServiceContext = new CrmServiceContext(crmService);
                    Commonlogger = Logger.GetLoggerWithCrmLogging(crmService, CommonTraceLog);
                    return crmService;
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in connection:" + ex.Message + " Stack_Trace: " + ex.StackTrace + " Inner Exception: " + ex.InnerException?.Message);
                return null;
            }
            return null;
        }


        #region GetSecretKeyForInterface(Guid credentialId, bmw_datalocation dataLocation, bmw_credentials bmwCredentials, string optionSetText)
        private string GetSecretKeyForInterface(Guid credentialId, bmw_datalocation dataLocation, bmw_credentials bmwCredentials, string optionSetText, EnDc.EnDcHelper endcHelper)
        {
            string secretData = string.Empty;

            if (!SecretKeyConDictionary.ContainsKey(credentialId))
            {
                var secretkey = endcHelper.KeyVault().GetAwaiter().GetResult();
                SecretKeyConDictionary.TryAdd(credentialId, secretkey);
            }

            return SecretKeyConDictionary[credentialId];

        }
        #endregion

        public static OrganizationWebProxyClient CRMConnect(Guid? owner)
        {
            try
            {
                string organizationUrl = ConfigurationManager.AppSettings["CRMOrgurl"];
                string resourceURL = ConfigurationManager.AppSettings["CRMOrgurl"] + "/api/data/v9.1/";
                string clientId = string.Empty;

                if (!string.IsNullOrEmpty(encrptkey) && !string.IsNullOrEmpty(Key64) && !string.IsNullOrEmpty(Value64))
                {
                    byte[] encrypted = Convert.FromBase64String(encrptkey);
                    byte[] encryptedKey = Convert.FromBase64String(Key64);
                    byte[] encryptedValue = Convert.FromBase64String(Value64);

                    // Decrypt the bytes to a string.
                    clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);
                }
                else
                {
                    return null;
                }

                string appKey = ConfigurationManager.AppSettings["ClientSecret"];

                //Create the Client credentials to pass for authentication
                ClientCredential clientcred = new ClientCredential(clientId, appKey);

                //get the authentication parameters
                AuthenticationParameters authParam = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(resourceURL)).Result;

                //Generate the authentication context - this is the azure login url specific to the tenant
                string authority = authParam.Authority;

                //request token
                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult = new AuthenticationContext(authority).AcquireTokenAsync(organizationUrl, clientcred).Result;

                //get the token              
                string token = authenticationResult.AccessToken;

                OrganizationWebProxyClient sdkService = null;
                Uri serviceUrl = new Uri(organizationUrl + @"/XRMServices/2011/Organization.svc/web?SdkClientVersion=9.1");
                if (!owner.HasValue)
                {
                    sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        CommoncrmService = sdkService;
                    }
                }
                else
                {
                    sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        sdkService.CallerId = owner.Value;
                    }

                }
                CommonCrmServiceContext = new CrmServiceContext(sdkService);

                return sdkService;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IOrganizationService CRMConnect(TraceWriter TraceLog, Guid? owner, string orgUrl)
        {
            try
            {
                CommonTraceLog = TraceLog;

                string organizationUrl = orgUrl;
                string resourceURL = orgUrl + "/api/data/v9.1/";

                string encrptkey = ConfigurationManager.AppSettings["ClientId"];
                string Key64 = ConfigurationManager.AppSettings["AESKey"];
                string Value64 = ConfigurationManager.AppSettings["AESIV"];

                byte[] encrypted = Convert.FromBase64String(encrptkey);
                byte[] encryptedKey = Convert.FromBase64String(Key64);
                byte[] encryptedValue = Convert.FromBase64String(Value64);

                string clientId = string.Empty;

                // Decrypt the bytes to a string.
                clientId = DecryptStringFromBytes_Aes(encrypted, encryptedKey, encryptedValue);

                string appKey = ConfigurationManager.AppSettings["ClientSecret"];

                //Create the Client credentials to pass for authentication
                ClientCredential clientcred = new ClientCredential(clientId, appKey);

                //get the authentication parameters
                AuthenticationParameters authParam = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(resourceURL)).Result;

                //Generate the authentication context - this is the azure login url specific to the tenant
                string authority = authParam.Authority;

                //request token
                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult = new AuthenticationContext(authority).AcquireTokenAsync(organizationUrl, clientcred).Result;

                //get the token              
                string token = authenticationResult.AccessToken;


                Uri serviceUrl = new Uri(organizationUrl + @"/XRMServices/2011/Organization.svc/web?SdkClientVersion=9.1");
                if (!owner.HasValue)
                {
                    OrganizationWebProxyClient sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        CommoncrmService = sdkService;
                    }
                }
                else
                {
                    OrganizationWebProxyClient sdkService = new OrganizationWebProxyClient(serviceUrl, true);
                    {
                        sdkService.HeaderToken = token;
                        CommoncrmService = sdkService;
                        sdkService.CallerId = owner.Value;
                    }

                }
                CommonCrmServiceContext = new CrmServiceContext(CommoncrmService);
                Commonlogger = Logger.GetLoggerWithCrmLogging(CommoncrmService, CommonTraceLog);

                return CommoncrmService;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in connection:" + ex.Message);
                return null;
            }
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            //// Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            //// Declare the string used to hold
            //// the decrypted text.
            string plaintext = null;

            //// Create an Aes object
            //// with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        public EntityCollection ExecuteQuery(string entityName, ColumnSet columnSet, FilterExpression filterExpression, LinkEntity linkentity, IOrganizationService service)
        {
            var Query = new QueryExpression()
            {
                EntityName = entityName,
                ColumnSet = columnSet,
                Criteria = filterExpression
            };

            if (linkentity != null)
                Query.LinkEntities.Add(linkentity);

            return service.RetrieveMultiple(Query);
        }

        #region GetLov(string lovName, string lic)
        public string GetLov(string lovName, string lic)
        {
            return this.GetCrmLicValue(lovName, lic);
        }
        #endregion

        #region ExistsParameter(string parameterName)
        public bool ExistsParameter(string parameterName)
        {
            var QEbmw_parameter = new QueryExpression(bmw_parameter.EntityLogicalName);
            QEbmw_parameter.Criteria.AddCondition(bmw_parameter.Fields.bmw_name, ConditionOperator.Equal, parameterName);
            QEbmw_parameter.Criteria.AddCondition(bmw_parameter.Fields.StateCode, ConditionOperator.Equal, 0);
            EntityCollection parameterColl = CommoncrmService.RetrieveMultiple(QEbmw_parameter);
            if (parameterColl.Entities.Count > 0)
            {
                return true;
            }
            else
                return false;
        }
        #endregion

        public static T GetCrmParameterValue<T>(string parameterName, bmw_parameter_bmw_category? bmwCategory = null)
        {
            Type type = typeof(T);

            string columnName = string.Empty;
            var QEbmw_parameter = new QueryExpression(bmw_parameter.EntityLogicalName);
            if (type == typeof(string))
                columnName = bmw_parameter.Fields.bmw_textvalue;
            if (type == typeof(bool))
                columnName = bmw_parameter.Fields.bmw_booleanvalue;
            if (type == typeof(decimal?))
                columnName = bmw_parameter.Fields.bmw_numbervalue;
            if (type == typeof(DateTime?))
                columnName = bmw_parameter.Fields.bmw_datevalue;
            if (type == typeof(double?))
                columnName = bmw_parameter.Fields.bmw_methodtype;
            if (type == typeof(int?))
                columnName = bmw_parameter.Fields.bmw_end;
            QEbmw_parameter.ColumnSet.AddColumn(columnName);
            QEbmw_parameter.Criteria.AddCondition(bmw_parameter.Fields.bmw_name, ConditionOperator.Equal, parameterName);
            QEbmw_parameter.Criteria.AddCondition(bmw_parameter.Fields.StateCode, ConditionOperator.Equal, 0);
            EntityCollection parameterColl = CommoncrmService.RetrieveMultiple(QEbmw_parameter);
            if (parameterColl.Entities.Count > 0 && parameterColl.Entities[0].Contains(columnName))
            {
                if (type == typeof(bool))
                    return (T)Convert.ChangeType(parameterColl.Entities[0].GetAttributeValue<OptionSetValue>(columnName).Value == 1, typeof(bool));
                else
                    return (parameterColl.Entities[0].GetAttributeValue<T>(columnName));
            }
            return default(T);
        }

        public static List<bmw_parameter> GetParamters(string parameterNames, IOrganizationService organizationService)
        {
            QueryExpression queryParameter = new QueryExpression(bmw_parameter.EntityLogicalName);
            queryParameter.ColumnSet.AddColumns(bmw_parameter.Fields.bmw_name, bmw_parameter.Fields.bmw_booleanvalue, bmw_parameter.Fields.bmw_description, bmw_parameter.Fields.bmw_datevalue, bmw_parameter.Fields.bmw_numbervalue, bmw_parameter.Fields.bmw_textvalue, bmw_parameter.Fields.bmw_textvalue2, bmw_parameter.Fields.bmw_parameterId);
            queryParameter.Criteria = new FilterExpression(LogicalOperator.Or);

            foreach (string paramtername in parameterNames.Split(','))
            {
                queryParameter.Criteria.AddCondition(bmw_parameter.Fields.bmw_name, ConditionOperator.Equal, paramtername);
            }

            EntityCollection entityCollection = organizationService.RetrieveMultiple(queryParameter);

            List<Entity> paramterList = new List<Entity>();
            paramterList.AddRange(entityCollection.Entities);
            return paramterList.Cast<bmw_parameter>().ToList();
        }

        #region GetCrmLicValue(string lovName, string licName,IOrganizationService crmService)
        public string GetCrmLicValue(string lovName, string licName)
        {
            bmw_lov lov = (from l in CommonCrmServiceContext.bmw_lovSet where l.bmw_name == lovName select l).FirstOrDefault();
            if (lov != null)
            {
                List<bmw_lic> lics = CommonCrmServiceContext.bmw_licSet.Where(p => p.bmw_lov.Id == lov.Id).ToList();

                return lics.Where(p => !String.IsNullOrEmpty(p.bmw_name) && p.bmw_name.Equals(licName, StringComparison.InvariantCultureIgnoreCase)).Select(p => p.bmw_value).FirstOrDefault();
            }
            return string.Empty;
        }
        #endregion

        #region GetCrmLicName(string lovName, string licName,IOrganizationService crmService)
        public string GetCrmLicName(string lovName, string licValue)
        {
            string returnValue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(lovName) && !string.IsNullOrEmpty(licValue))
                {
                    var QEbmw_lic = new QueryExpression("bmw_lic");
                    QEbmw_lic.ColumnSet.AddColumns("bmw_name");
                    QEbmw_lic.Criteria.AddCondition("bmw_value", ConditionOperator.Equal, licValue);
                    QEbmw_lic.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                    var QEbmw_lic_bmw_lov = QEbmw_lic.AddLink("bmw_lov", "bmw_lov", "bmw_lovid");
                    QEbmw_lic_bmw_lov.LinkCriteria.AddCondition("bmw_name", ConditionOperator.Equal, lovName);
                    EntityCollection collLic = CommoncrmService.RetrieveMultiple(QEbmw_lic);
                    if (collLic.Entities.Count > 0 && collLic.Entities[0].Contains("bmw_name"))
                    {
                        returnValue = collLic.Entities[0].GetAttributeValue<string>("bmw_name");
                    }
                }
            }
            catch (Exception)
            {
                returnValue = string.Empty;
            }
            return returnValue;
        }
        #endregion

        #region InterfaceRun
        public bmw_interfacerun InterfaceRun
        { get; set; }
        #endregion

        #region GetTestVehicleByQuery(string vehicleBrand, string vehicleModelName, IEnumerable<ConditionExpression> specialConditions, int? count)
        public IEnumerable<bmw_vehicle> GetTestVehicleByQuery(string vehicleBrand, string vehicleModelName, IEnumerable<ConditionExpression> specialConditions, int? count, IOrganizationService CrmService)
        {
            if (string.IsNullOrEmpty(vehicleBrand) && string.IsNullOrEmpty(vehicleModelName))
            {
                return null;
            }

            string[] searchString = (vehicleBrand + " " + vehicleModelName).Trim().Split(' ');
            string actual = String.Empty;

            QueryExpression query = new QueryExpression(bmw_vehicle.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            };

            if (count.HasValue)
            {
                query.PageInfo = new PagingInfo
                {
                    Count = count.Value,
                    PageNumber = 1
                };
            }

            FilterExpression brandCondition = new FilterExpression(LogicalOperator.Or);
            foreach (string str in searchString)
            {
                actual += " " + str;
                actual = actual.Trim();
                brandCondition.AddCondition("bmw_brand", ConditionOperator.Equal, actual);
            }
            bool auSpecific = false;
            auSpecific = GetCrmParameterValue<bool>("AU_SHOW_SPECIFICS");

            FilterExpression modelCondition = new FilterExpression(LogicalOperator.Or);
            if (auSpecific)
            {
                actual = String.Empty;

                foreach (string str in searchString.Reverse())
                {
                    actual = actual.Insert(0, str + " ");
                    actual = actual.Trim();
                    modelCondition.AddCondition("bmw_vgmodelcode", ConditionOperator.Equal, actual);
                }
            }
            else
            {
                actual = String.Empty;
                foreach (string str in searchString.Reverse())
                {
                    actual = actual.Insert(0, str + " ");
                    actual = actual.Trim();
                    modelCondition.AddCondition("bmw_model", ConditionOperator.Equal, actual);
                }
            }


            FilterExpression vinCondition = new FilterExpression(LogicalOperator.Or);
            vinCondition.AddCondition("bmw_vin", ConditionOperator.Null);
            vinCondition.AddCondition("bmw_vin", ConditionOperator.Equal, String.Empty);

            query.Criteria.AddFilter(brandCondition);
            query.Criteria.AddFilter(modelCondition);
            query.Criteria.AddFilter(vinCondition);
            query.Criteria.AddCondition("bmw_vehicleclassification", ConditionOperator.Equal, (int)bmw_VehicleClassifications.ModelTreeVehicle);

            if (specialConditions != null)
            {
                query.Criteria.Conditions.AddRange(specialConditions);
            }

            CrmService.Execute(new QueryExpressionToFetchXmlRequest { Query = query });

            EntityCollection entityCollection = CrmService.RetrieveMultiple(query);

            return entityCollection.Entities.Cast<bmw_vehicle>();
        }
        #endregion

        #region ResolveActivityType(bmw_leadrequestleadactivitytype activityType)
        protected static bmw_leadrequestleadactivitytype ResolveActivityType(string activityType)
        {
            switch (activityType)
            {
                case "TDA_GENERAL":
                    return bmw_leadrequestleadactivitytype.TDR;
                case "RFC_GENERAL":
                    return bmw_leadrequestleadactivitytype.RFC;
                case "RFO_GENERAL":
                    return bmw_leadrequestleadactivitytype.RFO;
                default:
                    return bmw_leadrequestleadactivitytype.RFI;
            }
        }

        #endregion

        #region FindReasonCode(string reasonCodeName)
        /// <summary>
        /// Find reason code according to its name
        /// </summary>
        /// <param name="reasonCodeName">Reason code name</param>
        /// <returns>Reference to reason code. Null if reason code is not found.</returns>
        public EntityReference FindReasonCode(string reasonCodeName, IOrganizationService CrmService)
        {
            QueryByAttribute query = new QueryByAttribute(bmw_reasoncode.EntityLogicalName);
            query.AddAttributeValue("bmw_name", reasonCodeName);
            query.PageInfo = new PagingInfo { Count = 1, PageNumber = 1 };
            query.ColumnSet = new ColumnSet();

            EntityCollection reasonCodes = CrmService.RetrieveMultiple(query);
            if (reasonCodes != null && reasonCodes.Entities.Count > 0)
            {
                return reasonCodes.Entities.First().ToEntityReference();
            }
            return null;
        }
        #endregion

        #region SetState(Entity entity, int statuscode, int? state = null)
        protected bool SetState(Entity entity, int statuscode, IOrganizationService crmService, int? state = null)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity.Contains("statuscode"))
            {
                OptionSetValue currentStatus = entity.GetAttributeValue<OptionSetValue>("statuscode");

                if (currentStatus != null && currentStatus.Value == statuscode)
                {
                    return true;
                }
            }

            int? entityState;

            if (state.HasValue)
            {
                entityState = state;
            }
            else
            {
                int? entityTypeCode = CrmMetadataHelper.GetObjectTypeCode(crmService, entity.LogicalName);

                if (!entityTypeCode.HasValue)
                {
                    return false;
                }

                entityState = this.GetStateForStatusCode(entityTypeCode.Value, statuscode, crmService);

                if (!entityState.HasValue)
                {
                    return false;
                }
            }

            if (entity.LogicalName.Equals(Opportunity.EntityLogicalName))
            {
                if (statuscode.Equals((int)Opportunity_StatusCode.SaleWon))
                {
                    return this.ProcessWinOpportunity(entity, statuscode, crmService);

                }
                if (statuscode.Equals((int)Opportunity_StatusCode.SaleLost) || statuscode.Equals((int)Opportunity_StatusCode.Cancelled))
                {

                    return this.ProcessLoseOpportunity(entity, statuscode, crmService);
                }

                return this.ProcessSetStateRequest(entity, entityState.Value, statuscode, crmService);
            }

            if (entity.LogicalName.Equals(Incident.EntityLogicalName) && statuscode.Equals((int)Incident_StatusCode.Closed))
            {
                return this.CloseIncident(entity, statuscode, crmService);
            }

            return this.ProcessSetStateRequest(entity, entityState.Value, statuscode, crmService);
        }
        #endregion

        #region ProcessSetStateRequest(Entity entity, int state, int statusCode)
        /// <summary>
        /// Tries to set state of entity using <see cref="IOrganizationService.Execute"/> method.
        /// </summary>
        /// <param name="entity">Entity to update state</param>		
        /// <param name="state">State value</param>
        /// <param name="statusCode">Status code value</param>
        /// <returns>True if state set successfully, otherwise false</returns>
        protected bool ProcessSetStateRequest(Entity entity, int state, int statusCode, IOrganizationService crmService)
        {
            SetStateRequest setStateRequest = new SetStateRequest
            {
                State = new OptionSetValue(state),
                Status =
                    new OptionSetValue(statusCode),
                EntityMoniker = entity.ToEntityReference()
            };

            // Execute the Request
            try
            {
                crmService.Execute(setStateRequest);
                return true;
            }
            catch (Exception ex)
            {
                string message = "Error when setting state of entity '" + entity.LogicalName + "', id '" + entity.Id + "', state '" + state +
                                 "', statuscode '" + statusCode + "'" + "\n " + ex.Message;
                CommonTraceLog.Error(message);
                return false;
            }
        }
        #endregion


        #region ProcessWinOpportunity(Entity entity, int statusCode)
        protected bool ProcessWinOpportunity(Entity entity, int statusCode, IOrganizationService crmService)
        {
            WinOpportunityRequest request = new WinOpportunityRequest
            {
                OpportunityClose = new OpportunityClose
                {
                    OpportunityId = new EntityReference
                        (Opportunity.EntityLogicalName, entity.Id)
                },
                Status = new OptionSetValue(statusCode)
            };

            try
            {
                crmService.Execute(request);
                return true;
            }
            catch (Exception ex)
            {
                string message = "Error when setting opportunity to SalesWon '" + entity.LogicalName + "', statuscode '" + statusCode + "'" + "\n " + ex.Message;
                CommonTraceLog.Error(message);
                return false;
            }

        }
        #endregion

        #region ProcessLoseOpportunity(Entity entity, int statusCode)
        protected bool ProcessLoseOpportunity(Entity entity, int statusCode, IOrganizationService CrmService)
        {
            LoseOpportunityRequest request = new LoseOpportunityRequest
            {
                OpportunityClose = new OpportunityClose
                {
                    OpportunityId = new EntityReference
                        (Opportunity.EntityLogicalName, entity.Id)
                },
                Status = new OptionSetValue(statusCode)
            };
            try
            {
                CrmService.Execute(request);
                return true;
            }
            catch (Exception ex)
            {
                string message = "Error when setting opportunity to SaleLost '" + entity.LogicalName + "', statuscode '" + statusCode + "'" + "\n" + ex.Message;
                CommonTraceLog.Error(message);
                return false;
            }

        }
        #endregion

        #region CloseIncident(Entity entity, int statusCode)
        protected bool CloseIncident(Entity entity, int statusCode, IOrganizationService CrmService)
        {
            IncidentResolution incidentResolution = new IncidentResolution
            {
                Subject = "Case Resolved",
                IncidentId = new EntityReference(Incident.EntityLogicalName, entity.Id)
            };

            CloseIncidentRequest request = new CloseIncidentRequest
            {
                Status = new OptionSetValue(statusCode),
                IncidentResolution = incidentResolution
            };

            try
            {
                CrmService.Execute(request);
                return true;
            }
            catch (Exception ex)
            {
                string message = "Error when closing incident id '" + entity.Id + "', statuscode '" + statusCode + "'" + "\n" + ex.Message;
                CommonTraceLog.Error(message);
                return false;
            }
        }
        #endregion


        #region GetStateForStatusCode(int objectTypeCode, int statusCode)
        protected int? GetStateForStatusCode(int objectTypeCode, int statusCode, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("statusmap") { ColumnSet = new ColumnSet(true) };

            query.Criteria.AddFilter(LogicalOperator.And);

            ConditionExpression condition1 = new ConditionExpression("objecttypecode",
                                                                     ConditionOperator.
                                                                        Equal,
                                                                     new object[] { objectTypeCode });

            query.Criteria.AddCondition(condition1);
            query.Criteria.AddCondition(new ConditionExpression("status",
                                                                ConditionOperator.Equal,
                                                                statusCode));

            query.PageInfo = new PagingInfo { Count = 1, PageNumber = 1, ReturnTotalRecordCount = true };

            EntityCollection entities = crmService.RetrieveMultiple(query);

            if (entities == null || entities.Entities == null ||
                entities.TotalRecordCount == 0)
            {
                return null;
            }

            Entity entity = entities.Entities.FirstOrDefault();

            if (entity == null || !entity.Attributes.Contains("state"))
            {
                return null;
            }

            int stateCode = (int)entity.Attributes["state"];
            return stateCode;
        }
        #endregion

        #region SaveContextChanges()
        /// <summary>
        /// Save changes from crm context
        /// </summary>
        /// <returns>False if ContinueOnError is not set and error occured. In all other cases returns True</returns>
        public bool SaveContextChanges()
        {
            return this.SaveContextChanges(false);
        }
        #endregion

        protected bool SaveContextChanges(bool alwaysReturnFalseOnErrors)
        {
            try
            {
                SaveChangesResultCollection results = CommonCrmServiceContext.SaveChanges();

                // Errors by saving 
                if (results.Any(result => result.Error != null))
                {
                    string errorsDump = "Errors occured during saving data to Crm:" + Environment.NewLine;
                    errorsDump = results.Where(result => result.Error != null).Aggregate(errorsDump,
                                                                                         (current, result) =>
                                                                                         current +
                                                                                         (result.Error.DumpException() +
                                                                                          Environment.NewLine));
                    CommonTraceLog.Error(errorsDump);
                    return this.ContextSaveChangesOption == SaveChangesOptions.ContinueOnError && !alwaysReturnFalseOnErrors;

                }
                return true;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("an exception occured in SaveContextChanges" + ex.Message);
            }

            return false;
        }

        #region ContextSaveChangesOption
        protected virtual SaveChangesOptions ContextSaveChangesOption
        {
            get
            {
                return SaveChangesOptions.None;
            }
        }
        #endregion

        #region GetInterfaceRun(Guid? interfaceRunId, ref OperationResult operationResult)
        public bmw_interfacerun GetInterfaceRun(Guid? interfaceRunId/*, ref OperationResult operationResult*/, string taskName)
        {
            if (CommoncrmService == null)
            {
                return null;
            }

            bmw_interfacerun interfaceRun = null;

            if (interfaceRunId.HasValue && interfaceRunId.Value != Guid.Empty)
            {
                Entity entity = CommoncrmService.Retrieve(bmw_interfacerun.EntityLogicalName, interfaceRunId.Value, new ColumnSet(true));

                if (entity != null)
                {
                    interfaceRun = entity.ToEntity<bmw_interfacerun>();
                }

                if (interfaceRun != null)
                {
                    interfaceRun.bmw_started = new OptionSetValue((int)bmw_interfacerun_bmw_started.Manually);
                }
            }

            if (interfaceRunId == null || interfaceRunId == Guid.Empty || interfaceRun == null)
            {
                interfaceRun = new bmw_interfacerun
                {
                    bmw_started = new OptionSetValue((int)bmw_interfacerun_bmw_started.Automatically),
                    bmw_interfacerunstatus = new OptionSetValue((int)bmw_interfacerun_bmw_interfacerunstatus.Running),
                    bmw_name = taskName + " " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss").Replace(".", "/")
                };

                if (Enum.TryParse(taskName, out bmw_Interfaces bmwInterface))
                {
                    interfaceRun.bmw_interface = new OptionSetValue((int)bmwInterface);
                }
                try
                {
                    interfaceRun = CommoncrmService.Retrieve(bmw_interfacerun.EntityLogicalName, CommoncrmService.Create(interfaceRun), new ColumnSet(true)) as bmw_interfacerun;

                    if (interfaceRun == null)
                    {
                        throw new ApplicationException("strange crm bug - crm did not create interfacerun entity");
                    }
                }
                catch (Exception ex)
                {
                    CommonTraceLog.Error("Error in Get interface Run " + ex.Message);
                    return null;
                }
            }

            return interfaceRun;
        }
        #endregion

        #region
        ////Check parameters for encryption/decryption from data location
        ////Parameters- 1:Doc-  Actual Data 2: encryption (True- Encryption,False-Decrption)
        public Stream CheckDataLocationForEncryptionDecryption(string doc, bool encryption, int ineterfaceValue)
        {
            Stream stream = null;
            bmw_datalocation datalocation = (from j in CommonCrmServiceContext.bmw_datalocationSet
                                             where j.bmw_interface.Value.Equals(Convert.ToInt32(ineterfaceValue))
                                             select j).FirstOrDefault();
            if (datalocation.bmw_end.HasValue && Convert.ToBoolean(datalocation.bmw_end.Value))
            {
                return DataEncryptionDecryption(doc, datalocation, encryption);
            }
            else
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(doc);
                stream = new MemoryStream(byteArray);
                return stream;
            }
            return null;
        }
        #endregion

        #region 
        public Stream DataEncryptionDecryption(string Doc, bmw_datalocation datalocation, bool encryption)
        {
            Stream stream = null;
            try
            {
                if (datalocation != null && datalocation.bmw_clientid != null && datalocation.bmw_clientsecret != null && datalocation.bmw_credentials != null)
                {
                    bmw_credentials credentials = (from j in CommonCrmServiceContext.bmw_credentialsSet
                                                   where j.Id.Equals(datalocation.bmw_credentials.Id)
                                                   select j).FirstOrDefault();

                    if (credentials.bmw_keyencryptionkey != null && credentials.bmw_baseurl != null && credentials.bmw_cek != null && credentials.bmw_algorithmtype != null)
                    {
                        try
                        {
                            int optionsetValue = Convert.ToInt32(credentials.bmw_algorithmtype.Value);
                            string optionSetText = string.Empty;
                            CommonTraceLog.Info($"Before GetOptionSetTextFromValue:{optionsetValue}");
                            optionSetText = GetLov("ALGORITHM_TYPE", optionsetValue.ToString()); //created LOV for this

                            if (!String.IsNullOrEmpty(optionSetText))
                            {
                                string obj_endcText = string.Empty;
                                //string AKVReferenceKey = string.Format("AKVReference_{0}", datalocation.bmw_textvalue);
                                //string AKVReferenceValue = ConfigurationManager.AppSettings[AKVReferenceKey];

                                //if (!string.IsNullOrEmpty(AKVReferenceValue))
                                //{
                                //    CommonTraceLog.Info("AKVReferenceValue found");
                                //}
                                ////Pass KeyVault details to EnDC
                                // EnDc.EnDcHelper obj_endc = new EnDc.EnDcHelper(datalocation.bmw_clientid, datalocation.bmw_clientsecret, credentials.bmw_baseurl, credentials.bmw_keyencryptionkey, credentials.bmw_cek, optionSetText, AKVReferenceValue);

                                ////Data converted successfully
                                //obj_endcText = obj_endc.ProtectData(Doc, encryption);

                                EnDc.EnDcHelper obj_endc = new EnDc.EnDcHelper(datalocation.bmw_clientid, datalocation.bmw_clientsecret, credentials.bmw_baseurl, credentials.bmw_keyencryptionkey, credentials.bmw_cek, optionSetText);
                                var secretKeyForCred = GetSecretKeyForInterface(credentials.Id, datalocation, credentials, optionSetText, obj_endc);

                                obj_endcText = obj_endc.ProtectData(Doc, encryption, secretKeyForCred);
                                byte[] byteArray = Encoding.UTF8.GetBytes(obj_endcText);

                                stream = new MemoryStream(byteArray);
                            }
                            else
                            {
                                CommonTraceLog.Info("Optionset Text not found");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!string.IsNullOrEmpty(datalocation.bmw_xsdname))
                            {
                                HttpResponseMessage response = CommonClass.ValidateXMLWithXSD(Doc, Convert.ToString(datalocation.bmw_xsdname));
                                if (response.StatusCode.ToString() != "OK")
                                {
                                    CommonTraceLog.Error($"Exce Error:{ex.Message} Stack Trace:{ex.StackTrace} Inner Exception:{ex.InnerException}");
                                    throw;
                                }
                                else
                                {
                                    byte[] byteArray = Encoding.UTF8.GetBytes(Doc);
                                    stream = new MemoryStream(byteArray);
                                }
                            }
                            else
                            {
                                CommonTraceLog.Error($"Exce Error:{ex.Message} Stack Trace:{ex.StackTrace} Inner Exception:{ex.InnerException}");
                                throw;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception Error:" + ex.Message);
                throw;
            }
            return stream;
        }
        #endregion

        #region Get Data Location Settings
        /// <summary>
        /// Get Data Location details with Interface as parameter
        /// </summary>
        /// <returns>Return data location details</returns>
        public static bmw_datalocation GetDataLocationSettings(bmw_Interfaces _interface)
        {
            bmw_datalocation datalocation = (from c in CommonCrmServiceContext.bmw_datalocationSet
                                             where c.bmw_interface == new OptionSetValue(Convert.ToInt32(_interface))
                                             select c).FirstOrDefault();

            return datalocation;
        }
        #endregion

        #region
        public static HttpResponseMessage ValidateXMLWithXSD(string XMLData, string XsdSchemaName)
        {
            if (XMLValidationLogicAppUri != null)
            {
                return httpClient.PostAsync(string.Format(XMLValidationLogicAppUri, XsdSchemaName), new StringContent(XMLData, Encoding.UTF8, "application/xml")).Result;
            }
            else
                return null;
        }
        #endregion

        #region public static void PushDataIntoBlob(string Data, string fileName, string containerName, string storageAccountConnString, string contentType)
        public static bool PushDataIntoBlob(string Data, string fileName, string containerName, string storageAccountConnString, string contentType, out string blobURL)
        {
            blobURL = string.Empty;
            try
            {
                //// Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnString);

                //// Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //// Create a container for organizing blobs within the storage account.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                blob.Properties.ContentType = contentType;
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(Data));
                blob.UploadFromStreamAsync(stream).Wait();
                blobURL = blob.Uri.AbsoluteUri;
                return true;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Error While Pushing DAta into the Blob " + ex.Message);
                blobURL = string.Empty;
            }
            return false;
        }

        #endregion

        #region convert XML to CSV
        public static string ConvertXMLToCSV(string XmlFile)
        {
            string csvfile = string.Empty;
            char del = ',';
            using (XmlReader xmlr = XmlReader.Create(new StringReader(XmlFile)))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(xmlr);
                csvfile = CreateCSVFile(ds.Tables[ds.Tables.Count - 1], del);
            }
            return csvfile;
        }
        #endregion

        public static string ConvertXMLToCSVwithDel(string XmlFile, char del)
        {
            string csvfile = string.Empty;
            using (XmlReader xmlr = XmlReader.Create(new StringReader(XmlFile)))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(xmlr);
                csvfile = CreateCSVFile(ds.Tables[ds.Tables.Count - 1], del);
            }
            return csvfile;
        }

        #region Create CSV File
        public static string CreateCSVFile(DataTable dt, char del)
        {
            StringBuilder sb = new StringBuilder();

            int iColCount = dt.Columns.Count - 1;   // Pass counts of record accordingly

            for (int i = 0; i < iColCount; i++)
            {
                if (dt.Columns[i].ColumnName.Contains("-"))
                {
                    string[] headers = dt.Columns[i].ColumnName.Split('-');
                    string newHeader = String.Empty;
                    for (int j = 0; j < headers.Length; j++)
                    {
                        newHeader = newHeader + headers[j] + " ";
                    }
                    dt.Columns[i].ColumnName = newHeader.Trim();
                }
                sb.Append(dt.Columns[i]);
                if (i <= iColCount - 2)
                {
                    sb.Append(del);
                }
            }

            sb.Append("\n");

            //// Now write all the rows.                      
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string data = dr[i].ToString().Replace("\n", "  ");
                        if (i < iColCount - 1)
                        {
                            sb.Append(data + del);
                        }
                        else
                        {
                            sb.Append(data);
                        }
                    }
                    else
                    {
                        if (i < iColCount - 1)
                        {
                            sb.Append(del);
                        }
                    }
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }
        #endregion

        public static void PushMessageInQueue(string XMLMessage, string queueName)
        {
            BrokeredMessage message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(XMLMessage)), true);
            var _client = QueueClient.CreateFromConnectionString(QueueConnectionString, queueName);
            _client.Send(message);

            ////var sbNameSpace = ServiceBus;
            ////var sendEndpoint = "https://" + sbNameSpace + ".servicebus.windows.net/" + queueName + "/messages";

            ////SendMessageInServiceBusQueue(sendEndpoint, XMLMessage, sbNameSpace, queueName);
        }

        private static void SendMessageInServiceBusQueue(string endpoint, string postData, string sbNameSpace, string queueName)
        {
            string generateSASTokenForSend = GetTokenForSend(sbNameSpace, queueName);
            WebRequest request = WebRequest.Create(endpoint);
            request.Method = "POST";
            request.Headers.Add("Authorization", generateSASTokenForSend);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/xml; charset = UTF-8";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Console.WriteLine(responseFromServer);
                }
            }
            response.Close();
        }

        private static string GetTokenForSend(string sbNameSpace, string sbPath)
        {
            string generatesas;
            var sbPolicy = ConfigurationManager.AppSettings["SendSharedAccessKeyName"];
            var sbvKey = ConfigurationManager.AppSettings["SendSharedAccessKeyValue"];
            var expiry = new TimeSpan(100, 1, 1, 1);
            var serviceURI = ServiceBusEnvironment.CreateServiceUri("https", sbNameSpace, sbPath).ToString().Trim('/');
            generatesas = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(sbPolicy, sbvKey, serviceURI, expiry);
            return generatesas;
        }

        public static void JsonToXML(string jsonData, IOrganizationService crmService, TraceWriter log, string taskName , bmw_interfacerun interfaceRun = null)
        {
            Logger Smosslogger;
            string fileName = string.Empty;
            crmService = CommonClass.CRMConnect(log, null);
            Smosslogger = Logger.GetLoggerWithCrmLogging(crmService, log);
            CrmServiceContext serviceContext = new CrmServiceContext(crmService);

            CommonClass common = new CommonClass();
            bmw_interfacerun interfacerunstatus;
            if (taskName.Equals("AutogateAPI"))
            {
                interfacerunstatus = interfaceRun;
            }
            else
            {
                interfacerunstatus = common.GetInterfaceRun(null, taskName);                               
            }           
            interfacerunstatus.StatusCode = new OptionSetValue((int)bmw_interfacerun_StatusCode.ReadyForProcessing);

            crmService.Update(interfacerunstatus);

            Smosslogger.Debug($"JSON content : {jsonData}", bmw_log_bmw_reasonstate.OK, interfacerunstatus, null);

            log.Info("under common class", null);

            string convertedJson = PrepareJson(jsonData, crmService, log);

            var Xmlconversion = JsonConvert.DeserializeXmlNode("{\"item\":" + convertedJson + "}", "root");

            XDocument xmlData = XDocument.Parse(Xmlconversion.InnerXml);

            Smosslogger.Debug($"XML : {xmlData.ToString()}", bmw_log_bmw_reasonstate.OK, interfacerunstatus, null);

            XDocument Leaddata = GetRetailDataDocument("DMSOpportunity", xmlData, log, taskName);

            bmw_datalocation datalocation = null;

            if (taskName.Equals("Oglivy_Cardekho_Carwale"))
            {
                datalocation = (from j in serviceContext.bmw_datalocationSet
                                where j.bmw_interface.Value.Equals(Convert.ToInt32(bmw_Interfaces.Oglivy_Cardekho_Carwale))
                                select j).FirstOrDefault();
            }
            else if (taskName.Equals("SMOSSIntegration"))
            {
                datalocation = (from j in serviceContext.bmw_datalocationSet
                                where j.bmw_interface.Value.Equals(Convert.ToInt32(bmw_Interfaces.SMOSSIntegration))
                                select j).FirstOrDefault();
            }
            else if (taskName.Equals("AutogateAPI"))
            {
                datalocation = (from j in serviceContext.bmw_datalocationSet
                                where j.bmw_interface.Value.Equals(Convert.ToInt32(bmw_Interfaces.AutogateAPI))
                                select j).FirstOrDefault();                
                dynamic Senddataobject = JObject.Parse(jsonData);
                if(Senddataobject.Identifier !=null)
                {
                    fileName = Senddataobject.Identifier + "_" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                }
            }

            if (datalocation != null)
            {
                string XsdSchemaName = Convert.ToString(datalocation.bmw_xsdname);

                log.Info("XsdSchemaName :: " + XsdSchemaName, null);

                try
                {
                    var response = httpClient.PostAsync(string.Format(logicAppUri, XsdSchemaName), new StringContent(Leaddata.ToString(), Encoding.UTF8, "application/xml")).Result;

                    string QueueName = Convert.ToString(datalocation.bmw_outgoingqueue);

                    if (response.StatusCode.ToString() == "OK")
                    {
                        // Create Sender  
                        PushMessageInQueue(Leaddata.ToString(), QueueName);
                        interfacerunstatus.bmw_interfacerunstatus = new OptionSetValue((int)bmw_interfacerun_bmw_interfacerunstatus.Completed);
                        interfacerunstatus.StateCode = bmw_interfacerunState.Inactive;
                        interfacerunstatus.StatusCode = new OptionSetValue((int)bmw_interfacerun_StatusCode.Completed);

                        crmService.Update(interfacerunstatus);
                    }
                    else
                    {
                        Smosslogger.Error("XSD validation Failed " + response.StatusCode, interfacerunstatus);
                        interfacerunstatus.bmw_interfacerunstatus = new OptionSetValue((int)bmw_interfacerun_bmw_interfacerunstatus.Error);
                        interfacerunstatus.StateCode = bmw_interfacerunState.Inactive;
                        interfacerunstatus.StatusCode = new OptionSetValue((int)bmw_interfacerun_StatusCode.Completed);
                        crmService.Update(interfacerunstatus);
                        if (taskName.Equals("AutogateAPI"))
                        {                          
                           CommonClass.MoveFileToError(Leaddata.ToString(), interfacerunstatus, Commonlogger, crmService, fileName, Convert.ToString(datalocation.bmw_errorfolder));
                        }                                               
                    }
                }
                catch (Exception ex)
                {
                    Smosslogger.Info($"exception occured while pushing message in queue: {ex.Message}", bmw_log_bmw_reasonstate.Error, interfacerunstatus, null);
                    if (taskName.Equals("AutogateAPI"))
                    {                       
                        CommonClass.MoveFileToError(Leaddata.ToString(), interfacerunstatus, Commonlogger, crmService, fileName, Convert.ToString(datalocation.bmw_errorfolder));
                    }
                }
            }
            else
            {
                Smosslogger.Error("Data location is not present for SMOSS/Oglivy_Cardekho_Carwale", interfacerunstatus);
            }
        }

        private static string PrepareJson(string inputs, IOrganizationService orgService, TraceWriter log)
        {
            log.Info("under perpareJosn", null);

            Dictionary<string, string> collection = GetLovLic(orgService, log);

            string convertedJson = CreateJson(inputs, collection);

            return convertedJson;
        }

        private static Dictionary<string, string> GetLovLic(IOrganizationService orgService, TraceWriter log)
        {
            log.Info("In GetLovLic- JSONTOXML_LOV");

            Dictionary<string, string> collectionList = new Dictionary<string, string>();

            using (CrmServiceContext svcContext = new CrmServiceContext(orgService))
            {
                List<bmw_lic> collection = (from attributes in svcContext.bmw_licSet
                                            join lov in svcContext.bmw_lovSet on attributes.bmw_lov.Id equals lov.Id
                                            where lov.bmw_name.Equals("MAPPINGS_8.1.2")
                                            select attributes).ToList();

                log.Info("collection LIC:", collection.Count.ToString());

                foreach (var item in collection)
                {
                    collectionList.Add(item.bmw_name, item.bmw_value);
                }
            }
            return collectionList;
        }

        private static string CreateJson(string inputJson, Dictionary<string, string> collection)
        {
            foreach (var item in collection)
            {
                if (inputJson.Contains(item.Key))
                {
                    inputJson = inputJson.Replace(item.Key, item.Value);
                }
            }
            return inputJson;
        }

        public static XDocument GetRetailDataDocument(string header, XDocument GetxmlData, TraceWriter log, string taskName)
        {
            XDocument document = new XDocument();
            XElement dataContentRootElement = new XElement(header);
            document.Add(dataContentRootElement);
            CreateMessageHeader(dataContentRootElement, GetxmlData, log, taskName);
            return document;
        }

        public static void CreateMessageHeader(XElement parent, XDocument GetxmlData, TraceWriter log, string taskName)
        {                     
            XElement messageHeader = parent.CreateElement("MessageHeader");
            log.Info("Collecting headers", null);
            if (taskName.Equals("SMOSSIntegration") || taskName.Equals("Oglivy_Cardekho_Carwale"))
            {
                messageHeader.CreateElement("Sender", "SMOSS");
            }
            else if (taskName.Equals("AutogateAPI"))
            {
                messageHeader.CreateElement("Sender", "Autogate");
            }
            messageHeader.CreateElement("Receiver", "SEM");
            messageHeader.CreateElement("TimeStamp", DateTime.Now.ToString("s"));
            messageHeader.CreateElement("CountryCode");
            messageHeader.CreateElement("SchemaVersion", "3.5.0");
            if(taskName.Equals("AutogateAPI"))
            {                                
                string Request_Id = GetxmlData.GetXDocumentElementStringValue("root/item/RequestId", string.Empty);
                if(!string.IsNullOrEmpty(Request_Id))
                {
                    messageHeader.CreateElement("FileName", taskName + Request_Id + String.Format("-{0:yyyyMMdd_hhmmsstt}", DateTime.Now));
                }
                else
                {
                    messageHeader.CreateElement("FileName", taskName + String.Format("-{0:yyyyMMdd_hhmmsstt}", DateTime.Now));
                }                                
            }
            else
            {
                messageHeader.CreateElement("FileName", taskName + String.Format("-{0:yyyyMMdd_hhmmsstt}", DateTime.Now));
            }           
            CreateMessageBody(parent, GetxmlData, log, taskName);
        }

        public static void CreateMessageBody(XElement parent, XDocument GetxmlData, TraceWriter log, string taskName)
        {
            try
            {
                XElement dealerElementParent = parent.CreateElement("MessageBody");
                XElement dealerElement;

                IEnumerable<XElement> xItems = GetxmlData.XPathSelectElements("root/item");
                Dictionary<string, List<XElement>> dealerdata = new Dictionary<string, List<XElement>>();
                XElement prospect = null;
                XElement vehicle = null;
                XElement assignment = null;
                string channel = string.Empty;

                foreach (XElement item in xItems)
                {
                    string dealer = item.XPathSelectElement("Dealer").Value;

                    if (taskName.Equals("AutogateAPI"))
                    {
                        channel = item.XPathSelectElement("Channel").Value;
                        prospect = item.XPathSelectElement("Prospect");
                        vehicle = item.XPathSelectElement("Item");
                        assignment = item.XPathSelectElement("Assignment");
                    }

                    if (!dealerdata.Keys.Contains(dealer))
                    {
                        List<XElement> data = new List<XElement>();
                        data.Add(item);
                        dealerdata.Add(dealer, data);
                    }
                    else
                    {
                        List<XElement> existingData = dealerdata[dealer];
                        existingData.Add(item);
                        dealerdata[dealer] = existingData;
                    }
                }

                foreach (KeyValuePair<string, List<XElement>> dealerwise in dealerdata)
                {
                    dealerElement = dealerElementParent.CreateElement("Dealer");
                    if (!String.IsNullOrWhiteSpace(dealerwise.Key))
                    {
                        dealerElement.CreateElement("CentralDealerID", dealerwise.Key);
                    }
                    foreach (var dealerrecord in dealerwise.Value)
                    {
                        if (taskName.Equals("SMOSSIntegration") || taskName.Equals("Oglivy_Cardekho_Carwale"))
                        {
                            CreateContent(dealerrecord, dealerElement, log, taskName);
                        }
                        else if (taskName.Equals("AutogateAPI"))
                        {
                            CreateContentforAutogateAPI(dealerrecord, dealerElement, log, taskName, prospect, vehicle, assignment, channel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info("error occured While creating Xml form Json ", ex.Message);
            }
        }

        #region CreateContent(XElement parentNode, object[] whatToConvert)
        private static void CreateContent(XElement dealerdata, XElement dealerElement, TraceWriter log, string taskName)
        {
            try
            {
                XElement opportunityElement = dealerElement.CreateElement("Opportunity");
                XElement customerElement = opportunityElement.CreateElement("Customer");
                if (dealerdata.XPathSelectElement("ReceiptNumber") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ReceiptNumber").Value))
                {
                    customerElement.CreateElement("ReceiptNumber", dealerdata.XPathSelectElement("ReceiptNumber").Value);
                }
                if (dealerdata.XPathSelectElement("BackendCustomerId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BackendCustomerId").Value))
                {
                    customerElement.CreateElement("BackendCustomerId", dealerdata.XPathSelectElement("BackendCustomerId").Value);
                }
                if (dealerdata.XPathSelectElement("BackendSystemName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BackendSystemName").Value))
                {
                    customerElement.CreateElement("BackendSystemName", dealerdata.XPathSelectElement("BackendSystemName").Value);
                }
                if (taskName.Equals("Oglivy_Cardekho_Carwale"))
                {
                    if (dealerdata.XPathSelectElement("WebSource") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("WebSource").Value))
                    {
                        customerElement.CreateElement("SalesChannel", dealerdata.XPathSelectElement("WebSource").Value);
                    }
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    customerElement.CreateElement("SalesChannel", "Online_Sales");
                }
                if (dealerdata.XPathSelectElement("LastName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("LastName").Value))
                {
                    customerElement.CreateElement("LastName", dealerdata.XPathSelectElement("LastName").Value);
                }
                if (dealerdata.XPathSelectElement("LastNameKana") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("LastNameKana").Value))
                {
                    customerElement.CreateElement("LastNameKana", dealerdata.XPathSelectElement("LastNameKana").Value);
                }
                if (dealerdata.XPathSelectElement("FirstNameKana") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("FirstNameKana").Value))
                {
                    customerElement.CreateElement("FirstNameKana", dealerdata.XPathSelectElement("FirstNameKana").Value);
                }
                if (dealerdata.XPathSelectElement("FirstName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("FirstName").Value))
                {
                    customerElement.CreateElement("FirstName", dealerdata.XPathSelectElement("FirstName").Value);
                }
                if (dealerdata.XPathSelectElement("Salutation") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Salutation").Value))
                {
                    customerElement.CreateElement("Salutation", dealerdata.XPathSelectElement("Salutation").Value);
                }
                if (dealerdata.XPathSelectElement("Sex") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Sex").Value))
                {
                    customerElement.CreateElement("Sex", dealerdata.XPathSelectElement("Sex").Value);
                }

                customerElement.CreateElement("ContactType", "1");

                if (dealerdata.XPathSelectElement("Phone") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Phone").Value))
                {
                    customerElement.CreateElement("Phone", dealerdata.XPathSelectElement("Phone").Value);
                }
                if (dealerdata.XPathSelectElement("MobilePhone") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("MobilePhone").Value))
                {
                    customerElement.CreateElement("MobilePhone", dealerdata.XPathSelectElement("MobilePhone").Value);
                }
                if (dealerdata.XPathSelectElement("Email") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Email").Value))
                {
                    customerElement.CreateElement("Email", dealerdata.XPathSelectElement("Email").Value);
                }
                if (dealerdata.XPathSelectElement("BusinessPhone") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BusinessPhone").Value))
                {
                    customerElement.CreateElement("BusinessPhone", dealerdata.XPathSelectElement("BusinessPhone").Value);
                }
                if (dealerdata.XPathSelectElement("MailPhone") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("MailPhone").Value))
                {
                    customerElement.CreateElement("MailPhone", dealerdata.XPathSelectElement("MailPhone").Value);
                }
                if (dealerdata.XPathSelectElement("smossUniqueId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("smossUniqueId").Value))
                {
                    customerElement.CreateElement("smossUniqueId", dealerdata.XPathSelectElement("smossUniqueId").Value);
                }
                if (dealerdata.XPathSelectElement("bookingAmount") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("bookingAmount").Value))
                {
                    customerElement.CreateElement("bookingAmount", dealerdata.XPathSelectElement("bookingAmount").Value);
                }
                if (dealerdata.XPathSelectElement("occupation") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("occupation").Value))
                {
                    customerElement.CreateElement("occupation", dealerdata.XPathSelectElement("occupation").Value);
                }
                if (dealerdata.XPathSelectElement("PreferredContactChannel") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PreferredContactChannel").Value))
                {
                    customerElement.CreateElement("PreferredContactChannel", dealerdata.XPathSelectElement("PreferredContactChannel").Value);
                }
                if (dealerdata.XPathSelectElement("BirthDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BirthDate").Value))
                {
                    customerElement.CreateElement("BirthDate", dealerdata.XPathSelectElement("BirthDate").Value);
                }
                #region changes CR399 india
                if (dealerdata.XPathSelectElement("Profession") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Profession").Value))
                {
                    customerElement.CreateElement("Profession", dealerdata.XPathSelectElement("Profession").Value);
                }
                #endregion
                if (dealerdata.XPathSelectElement("donotemail") != null && dealerdata.XPathSelectElement("donotemail").Value == "true")
                {
                    customerElement.CreateElement("NoEmailFlag", "Y");
                }
                else
                {
                    customerElement.CreateElement("NoEmailFlag", "N");
                }
                if (dealerdata.XPathSelectElement("donotphone") != null && dealerdata.XPathSelectElement("donotphone").Value == "true")
                {
                    customerElement.CreateElement("NoPhoneFlag", "Y");
                }
                else
                {
                    customerElement.CreateElement("NoPhoneFlag", "N");
                }
                if (dealerdata.XPathSelectElement("JobTitle") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("JobTitle").Value))
                {
                    customerElement.CreateElement("JobTitle", dealerdata.XPathSelectElement("JobTitle").Value);
                }
                if (dealerdata.XPathSelectElement("StopNewsletterFlag") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("StopNewsletterFlag").Value))
                {
                    customerElement.CreateElement("StopNewsletterFlag", dealerdata.XPathSelectElement("StopNewsletterFlag").Value);
                }
                if (dealerdata.XPathSelectElement("Industry") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Industry").Value))
                {
                    customerElement.CreateElement("Industry", dealerdata.XPathSelectElement("Industry").Value);
                }
                if (dealerdata.XPathSelectElement("Comments") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Comments").Value))
                {
                    customerElement.CreateElement("Comments", dealerdata.XPathSelectElement("Comments").Value);
                }
                if (dealerdata.XPathSelectElement("AllowMarketingPromotionFlag") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("AllowMarketingPromotionFlag").Value))
                {
                    customerElement.CreateElement("AllowMarketingPromotionFlag", dealerdata.XPathSelectElement("AllowMarketingPromotionFlag").Value);
                }
                if (dealerdata.XPathSelectElement("AccessDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("AccessDate").Value))
                {
                    customerElement.CreateElement("AccessDate", dealerdata.XPathSelectElement("AccessDate").Value);
                }
                if (dealerdata.XPathSelectElement("AccessTime") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("AccessTime").Value))
                {
                    customerElement.CreateElement("AccessTime", dealerdata.XPathSelectElement("AccessTime").Value);
                }
                if (dealerdata.XPathSelectElement("SourceCampaign") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SourceCampaign").Value))
                {
                    customerElement.CreateElement("SourceCampaign", dealerdata.XPathSelectElement("SourceCampaign").Value);
                }
                if (dealerdata.XPathSelectElement("SourceName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SourceName").Value))
                {
                    customerElement.CreateElement("SourceName", dealerdata.XPathSelectElement("SourceName").Value);
                }
                if (dealerdata.XPathSelectElement("CampaignCode") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CampaignCode").Value))
                {
                    customerElement.CreateElement("CampaignCode", dealerdata.XPathSelectElement("CampaignCode").Value);
                }
                if (dealerdata.XPathSelectElement("CampaignName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CampaignName").Value))
                {
                    customerElement.CreateElement("CampaignName", dealerdata.XPathSelectElement("CampaignName").Value);
                }
                if (dealerdata.XPathSelectElement("PurchaseTimeFrame") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PurchaseTimeFrame").Value))
                {
                    customerElement.CreateElement("PurchaseTimeFrame", dealerdata.XPathSelectElement("PurchaseTimeFrame").Value);
                }
                if (dealerdata.XPathSelectElement("Temperature") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Temperature").Value))
                {
                    customerElement.CreateElement("Temperature", dealerdata.XPathSelectElement("Temperature").Value);
                }
                if (dealerdata.XPathSelectElement("City") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("City").Value))
                {
                    customerElement.CreateElement("City", dealerdata.XPathSelectElement("City").Value);
                }
                if (dealerdata.XPathSelectElement("Subject") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Subject").Value))
                {
                    customerElement.CreateElement("Subject", dealerdata.XPathSelectElement("Subject").Value);
                }
                if (dealerdata.XPathSelectElement("Platform") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Platform").Value))
                {
                    customerElement.CreateElement("Platform", dealerdata.XPathSelectElement("Platform").Value);
                }
                if (dealerdata.XPathSelectElement("ExperiencePeople") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ExperiencePeople").Value))
                {
                    customerElement.CreateElement("ExperiencePeople", dealerdata.XPathSelectElement("ExperiencePeople").Value);
                }
                if (dealerdata.XPathSelectElement("Media") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Media").Value))
                {
                    customerElement.CreateElement("Media", dealerdata.XPathSelectElement("Media").Value);
                }
                if (dealerdata.XPathSelectElement("PaymentMethod") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PaymentMethod").Value))
                {
                    customerElement.CreateElement("PaymentMethod", dealerdata.XPathSelectElement("PaymentMethod").Value);
                }
                if (dealerdata.XPathSelectElement("NegotiationEmail") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("NegotiationEmail").Value))
                {
                    customerElement.CreateElement("NegotiationEmail", dealerdata.XPathSelectElement("NegotiationEmail").Value);
                }
                if (dealerdata.XPathSelectElement("PresenceofTradeInCar") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PresenceofTradeInCar").Value))
                {
                    customerElement.CreateElement("PresenceofTradeInCar", dealerdata.XPathSelectElement("PresenceofTradeInCar").Value);
                }
                //new tags added on 27-09-2019
                if (dealerdata.XPathSelectElement("gcId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("gcId").Value))
                {
                    customerElement.CreateElement("gcId", dealerdata.XPathSelectElement("gcId").Value);
                }
                if (dealerdata.XPathSelectElement("ucId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ucId").Value))
                {
                    customerElement.CreateElement("ucId", dealerdata.XPathSelectElement("ucId").Value);
                }
                if (dealerdata.XPathSelectElement("bmwPreferredDealer") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("bmwPreferredDealer").Value))
                {
                    customerElement.CreateElement("bmwPreferredDealer", dealerdata.XPathSelectElement("bmwPreferredDealer").Value);
                }
                if (dealerdata.XPathSelectElement("lineID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("lineID").Value))
                {
                    customerElement.CreateElement("lineID", dealerdata.XPathSelectElement("lineID").Value);
                }
                if (dealerdata.XPathSelectElement("action") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("action").Value))
                {
                    customerElement.CreateElement("action", dealerdata.XPathSelectElement("action").Value);
                }

                XElement HobbiesElement = customerElement.CreateElement("Hobbies");
                if (dealerdata.XPathSelectElement("Hobby") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Hobby").Value))
                {
                    HobbiesElement.CreateElement("Hobby", dealerdata.XPathSelectElement("Hobby").Value);
                }

                XElement MaterialElement = customerElement.CreateElement("Material");
                if (dealerdata.XPathSelectElement("Material_code") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Material_code").Value))
                {
                    MaterialElement.CreateElement("Material_code", dealerdata.XPathSelectElement("Material_code").Value);
                }

                XElement CustomerProfileDataElement = customerElement.CreateElement("CustomerProfileData");

                if (dealerdata.XPathSelectElement("FreeComment10") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("FreeComment10").Value))
                {
                    CustomerProfileDataElement.CreateElement("FreeComment10", dealerdata.XPathSelectElement("FreeComment10").Value);
                }
                #region changes CR399 india
                if (dealerdata.XPathSelectElement("ExistingCarEvaluation") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ExistingCarEvaluation").Value))
                {
                    customerElement.CreateElement("ExistingCarEvaluation", dealerdata.XPathSelectElement("ExistingCarEvaluation").Value);
                }

                if (dealerdata.XPathSelectElement("BudgetRangeBudgetAmount") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BudgetRangeBudgetAmount").Value))
                {
                    customerElement.CreateElement("BudgetRangeBudgetAmount", dealerdata.XPathSelectElement("BudgetRangeBudgetAmount").Value);
                }

                if (dealerdata.XPathSelectElement("CarUsage") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CarUsage").Value))
                {
                    customerElement.CreateElement("CarUsage", dealerdata.XPathSelectElement("CarUsage").Value);
                }

                if (dealerdata.XPathSelectElement("PrimaryOwnedBrand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PrimaryOwnedBrand").Value))
                {
                    customerElement.CreateElement("PrimaryOwnedBrand", dealerdata.XPathSelectElement("PrimaryOwnedBrand").Value);
                }

                if (dealerdata.XPathSelectElement("PrimaryOwnedVehicle") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PrimaryOwnedVehicle").Value))
                {
                    customerElement.CreateElement("PrimaryOwnedVehicle", dealerdata.XPathSelectElement("PrimaryOwnedVehicle").Value);
                }

                if (dealerdata.XPathSelectElement("SecondaryOwnedVehicle") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SecondaryOwnedVehicle").Value))
                {
                    customerElement.CreateElement("SecondaryOwnedVehicle", dealerdata.XPathSelectElement("SecondaryOwnedVehicle").Value);
                }

                if (dealerdata.XPathSelectElement("SecondaryOwnerBrand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SecondaryOwnerBrand").Value))
                {
                    customerElement.CreateElement("SecondaryOwnerBrand", dealerdata.XPathSelectElement("SecondaryOwnerBrand").Value);
                }

                if (dealerdata.XPathSelectElement("FinancialServicesInterested") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("FinancialServicesInterested").Value))
                {
                    customerElement.CreateElement("FinancialServicesInterested", dealerdata.XPathSelectElement("FinancialServicesInterested").Value);
                }
                if (dealerdata.XPathSelectElement("marketingConsent") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("marketingConsent").Value))
                {
                    customerElement.CreateElement("marketingConsent", dealerdata.XPathSelectElement("marketingConsent").Value);
                }
                #endregion
                XElement AddressElement = customerElement.CreateElement("Address");
                if (dealerdata.XPathSelectElement("TypeOfAddress") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TypeOfAddress").Value))
                {
                    AddressElement.CreateElement("TypeOfAddress", dealerdata.XPathSelectElement("TypeOfAddress").Value);
                }
                if (dealerdata.XPathSelectElement("Prefecture") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Prefecture").Value))
                {
                    AddressElement.CreateElement("Prefecture", dealerdata.XPathSelectElement("Prefecture").Value);
                }
                if (dealerdata.XPathSelectElement("StreetAddress") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("StreetAddress").Value))
                {
                    AddressElement.CreateElement("StreetAddress", dealerdata.XPathSelectElement("StreetAddress").Value);
                }
                if (dealerdata.XPathSelectElement("Building") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Building").Value))
                {
                    AddressElement.CreateElement("Building", dealerdata.XPathSelectElement("Building").Value);
                }
                if (dealerdata.XPathSelectElement("Town") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Town").Value))
                {
                    AddressElement.CreateElement("Town", dealerdata.XPathSelectElement("Town").Value);
                }
                if (dealerdata.XPathSelectElement("ZipCode") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ZipCode").Value))
                {
                    AddressElement.CreateElement("ZipCode", dealerdata.XPathSelectElement("ZipCode").Value);
                }
                if (dealerdata.XPathSelectElement("StreetAddressLine2") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("StreetAddressLine2").Value))
                {
                    AddressElement.CreateElement("StreetAddressLine2", dealerdata.XPathSelectElement("StreetAddressLine2").Value);
                }
                if (dealerdata.XPathSelectElement("AddressLine3") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("AddressLine3").Value))
                {
                    AddressElement.CreateElement("AddressLine3", dealerdata.XPathSelectElement("AddressLine3").Value);
                }

                XElement ProductElement = customerElement.CreateElement("Product");

                if (dealerdata.XPathSelectElement("CurrentlyOwnedVehicle") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CurrentlyOwnedVehicle").Value))
                {
                    ProductElement.CreateElement("CurrentlyOwnedVehicle", dealerdata.XPathSelectElement("CurrentlyOwnedVehicle").Value);
                }

                ProductElement.CreateElement("VehicleOwned", "0");

                if (dealerdata.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Brand").Value))
                {
                    ProductElement.CreateElement("Brand", dealerdata.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    ProductElement.CreateElement("Brand", "BMW");
                }
                if (dealerdata.XPathSelectElement("Series") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Series").Value))
                {
                    ProductElement.CreateElement("Series", dealerdata.XPathSelectElement("Series").Value);
                }
                if (dealerdata.XPathSelectElement("YearOfConstruction") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("YearOfConstruction").Value))
                {
                    ProductElement.CreateElement("YearOfConstruction", dealerdata.XPathSelectElement("YearOfConstruction").Value);
                }
                if (dealerdata.XPathSelectElement("UpcomingInspectionDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("UpcomingInspectionDate").Value))
                {
                    ProductElement.CreateElement("UpcomingInspectionDate", dealerdata.XPathSelectElement("UpcomingInspectionDate").Value);
                }

                // CR 459 AND CR 458 India Market October Release

                if (dealerdata.XPathSelectElement("plan") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("plan").Value))
                {
                    customerElement.CreateElement("plan", dealerdata.XPathSelectElement("plan").Value);
                }
                if (dealerdata.XPathSelectElement("emi") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("emi").Value))
                {
                    customerElement.CreateElement("emi", dealerdata.XPathSelectElement("emi").Value);
                }
                if (dealerdata.XPathSelectElement("downPayment") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("downPayment").Value))
                {
                    customerElement.CreateElement("downPayment", dealerdata.XPathSelectElement("downPayment").Value);
                }
                if (dealerdata.XPathSelectElement("tenure") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("tenure").Value))
                {
                    customerElement.CreateElement("tenure", dealerdata.XPathSelectElement("tenure").Value);
                }
                if (dealerdata.XPathSelectElement("lastEmi") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("lastEmi").Value))
                {
                    customerElement.CreateElement("lastEmi", dealerdata.XPathSelectElement("lastEmi").Value);
                }

                if (dealerdata.XPathSelectElement("bookingDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("bookingDate").Value))
                {
                    customerElement.CreateElement("bookingDate", dealerdata.XPathSelectElement("bookingDate").Value);
                }

                //Central CR: SMOSS new mappings for AU, NZ, TH and MY market interface
                //Create Packages
                if (dealerdata.XPathSelectElement("Packages") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Packages").Value))
                {
                    IEnumerable<XElement> Packages = dealerdata.XPathSelectElements("Packages");

                    foreach (XElement Package in Packages)
                    {
                        XElement PackagesElement = customerElement.CreateElement("Packages");

                        //PackageType = SINGLE_PACKAGE or CATEGORY
                        if (Package.XPathSelectElement("PackageType") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageType").Value) && (Package.XPathSelectElement("PackageType").Value == "SINGLE_PACKAGE" || Package.XPathSelectElement("PackageType").Value == "CATEGORY"))
                        {
                            if (Package.XPathSelectElement("PackageType") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageType").Value))
                            {
                                PackagesElement.CreateElement("PackageType", Package.XPathSelectElement("PackageType").Value);
                            }
                            if (Package.XPathSelectElement("PackageName") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageName").Value))
                            {
                                PackagesElement.CreateElement("PackageName", Package.XPathSelectElement("PackageName").Value);
                            }
                            if (Package.XPathSelectElement("PackagePrice") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackagePrice").Value))
                            {
                                PackagesElement.CreateElement("PackagePrice", Package.XPathSelectElement("PackagePrice").Value);
                            }

                            //Create Products
                            if (Package.XPathSelectElement("Products") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("Products").Value))
                            {
                                IEnumerable<XElement> Products = Package.XPathSelectElements("Products");

                                foreach (XElement Product in Products)
                                {
                                    XElement ProductsElement = PackagesElement.CreateElement("Products");

                                    if (Product.XPathSelectElement("ProductName") != null && !String.IsNullOrEmpty(Product.XPathSelectElement("ProductName").Value))
                                    {
                                        ProductsElement.CreateElement("ProductName", Product.XPathSelectElement("ProductName").Value);
                                    }
                                    if (Product.XPathSelectElement("ProductPrice") != null && !String.IsNullOrEmpty(Product.XPathSelectElement("ProductPrice").Value))
                                    {
                                        ProductsElement.CreateElement("ProductPrice", Product.XPathSelectElement("ProductPrice").Value);
                                    }
                                }
                            }
                        }
                        //PackageType = COMBINATION
                        else if (Package.XPathSelectElement("PackageType") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageType").Value) && Package.XPathSelectElement("PackageType").Value == "COMBINATION")
                        {
                            if (Package.XPathSelectElement("PackageType") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageType").Value))
                            {
                                PackagesElement.CreateElement("PackageType", Package.XPathSelectElement("PackageType").Value);
                            }
                            if (Package.XPathSelectElement("PackageName") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackageName").Value))
                            {
                                PackagesElement.CreateElement("PackageName", Package.XPathSelectElement("PackageName").Value);
                            }
                            if (Package.XPathSelectElement("PackagePrice") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("PackagePrice").Value))
                            {
                                PackagesElement.CreateElement("PackagePrice", Package.XPathSelectElement("PackagePrice").Value);
                            }

                            //Combinations
                            if (Package.XPathSelectElement("Combinations") != null && !String.IsNullOrEmpty(Package.XPathSelectElement("Combinations").Value))
                            {
                                IEnumerable<XElement> Combinations = Package.XPathSelectElements("Combinations");

                                foreach (XElement Combination in Combinations)
                                {
                                    //Create Products
                                    if (Combination.XPathSelectElement("Products") != null && !String.IsNullOrEmpty(Combination.XPathSelectElement("Products").Value))
                                    {
                                        IEnumerable<XElement> Products = Combination.XPathSelectElements("Products");

                                        foreach (XElement Product in Products)
                                        {
                                            XElement ProductsElement = PackagesElement.CreateElement("Products");

                                            if (Product.XPathSelectElement("ProductName") != null && !String.IsNullOrEmpty(Product.XPathSelectElement("ProductName").Value))
                                            {
                                                ProductsElement.CreateElement("ProductName", Product.XPathSelectElement("ProductName").Value);
                                            }
                                            if (Product.XPathSelectElement("ProductPrice") != null && !String.IsNullOrEmpty(Product.XPathSelectElement("ProductPrice").Value))
                                            {
                                                ProductsElement.CreateElement("ProductPrice", Product.XPathSelectElement("ProductPrice").Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Exterior, Interior, LeadCreateddate, Finalcarprice
                if (dealerdata.XPathSelectElement("Exterior") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Exterior").Value))
                {
                    customerElement.CreateElement("Exterior", dealerdata.XPathSelectElement("Exterior").Value);
                }
                if (dealerdata.XPathSelectElement("Interior") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Interior").Value))
                {
                    customerElement.CreateElement("Interior", dealerdata.XPathSelectElement("Interior").Value);
                }
                if (dealerdata.XPathSelectElement("LeadCreatedDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("LeadCreatedDate").Value))
                {
                    customerElement.CreateElement("LeadCreatedDate", dealerdata.XPathSelectElement("LeadCreatedDate").Value);
                }
                if (dealerdata.XPathSelectElement("FinalCarPrice") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("FinalCarPrice").Value))
                {
                    customerElement.CreateElement("FinalCarPrice", dealerdata.XPathSelectElement("FinalCarPrice").Value);
                }

                if (dealerdata.XPathSelectElement("LeadScoring") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("LeadScoring").Value))
                {
                    customerElement.CreateElement("LeadScoring", dealerdata.XPathSelectElement("LeadScoring").Value);
                }

                if (dealerdata.XPathSelectElement("DataSegmentpoint") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("DataSegmentpoint").Value))
                {
                    customerElement.CreateElement("DataSegmentpoint", dealerdata.XPathSelectElement("DataSegmentpoint").Value);
                }

                if (dealerdata.XPathSelectElement("PID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("PID").Value))
                {
                    customerElement.CreateElement("PID", dealerdata.XPathSelectElement("PID").Value);
                }

                if (dealerdata.XPathSelectElement("CustomerDigitalFootprint") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CustomerDigitalFootprint").Value))
                {
                    customerElement.CreateElement("CustomerDigitalFootprint", dealerdata.XPathSelectElement("CustomerDigitalFootprint").Value);
                }

                if (dealerdata.XPathSelectElement("ConnectDriveURL") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ConnectDriveURL").Value))
                {
                    customerElement.CreateElement("ConnectDriveURL", dealerdata.XPathSelectElement("ConnectDriveURL").Value);
                }

                XElement ActivityElement = opportunityElement.CreateElement("Activity");

                if (dealerdata.XPathSelectElement("ActivityId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityId").Value))
                {
                    ActivityElement.CreateElement("ActivityId", dealerdata.XPathSelectElement("ActivityId").Value);
                }
                if (dealerdata.XPathSelectElement("ActivityType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityType").Value))
                {
                    ActivityElement.CreateElement("ActivityType", dealerdata.XPathSelectElement("ActivityType").Value);
                }
                if (dealerdata.XPathSelectElement("ActivityDueDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityDueDate").Value))
                {
                    ActivityElement.CreateElement("ActivityDueDate", dealerdata.XPathSelectElement("ActivityDueDate").Value);
                }
                if (dealerdata.XPathSelectElement("DOR") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("DOR").Value))
                {
                    String result = DateTime.ParseExact(dealerdata.XPathSelectElement("DOR").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    ActivityElement.CreateElement("DOR", result);
                }
                if (dealerdata.XPathSelectElement("RequestId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("RequestId").Value))
                {
                    ActivityElement.CreateElement("RequestId", dealerdata.XPathSelectElement("RequestId").Value);
                }
                if (dealerdata.XPathSelectElement("TestDriveDate1") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate1").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate1", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate1").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                if (dealerdata.XPathSelectElement("TestDriveDate2") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate2").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate2", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate2").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                if (dealerdata.XPathSelectElement("TestDriveDate3") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate3").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate3", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate3").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                // new tags
                if (dealerdata.XPathSelectElement("salesStage") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("salesStage").Value))
                {
                    ActivityElement.CreateElement("salesStage", dealerdata.XPathSelectElement("salesStage").Value);
                }

                if (dealerdata.XPathSelectElement("enquiryType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("enquiryType").Value))
                {
                    ActivityElement.CreateElement("enquiryType", dealerdata.XPathSelectElement("enquiryType").Value);
                }
                if (dealerdata.XPathSelectElement("customerType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("customerType").Value))
                {
                    ActivityElement.CreateElement("customerType", dealerdata.XPathSelectElement("customerType").Value);
                }
                if (dealerdata.XPathSelectElement("BPS") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BPS").Value))
                {
                    ActivityElement.CreateElement("BPS", dealerdata.XPathSelectElement("BPS").Value);
                }
                if (dealerdata.XPathSelectElement("CurrentCar") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CurrentCar").Value))
                {
                    ActivityElement.CreateElement("CurrentCar", dealerdata.XPathSelectElement("CurrentCar").Value);
                }
                if (dealerdata.XPathSelectElement("CarMonth") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CarMonth").Value))
                {
                    ActivityElement.CreateElement("CarMonth", dealerdata.XPathSelectElement("CarMonth").Value);
                }
                if (dealerdata.XPathSelectElement("CarYear") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CarYear").Value))
                {
                    ActivityElement.CreateElement("CarYear", dealerdata.XPathSelectElement("CarYear").Value);
                }
                if (dealerdata.XPathSelectElement("TotalKilometer") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TotalKilometer").Value))
                {
                    ActivityElement.CreateElement("TotalKilometer", dealerdata.XPathSelectElement("TotalKilometer").Value);
                }

                XElement ActivityProductElement = ActivityElement.CreateElement("Product");

                if (dealerdata.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Brand").Value))
                {
                    ActivityProductElement.CreateElement("Brand", dealerdata.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    ActivityProductElement.CreateElement("Brand", "BMW");
                }
                if (dealerdata.XPathSelectElement("Series") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Series").Value))
                {
                    ActivityProductElement.CreateElement("Series", dealerdata.XPathSelectElement("Series").Value);
                }

                ActivityProductElement.CreateElement("BodyType");

                if (dealerdata.XPathSelectElement("ModelCode") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ModelCode").Value))
                {
                    ActivityProductElement.CreateElement("ModelCode", dealerdata.XPathSelectElement("ModelCode").Value);
                }
                if (dealerdata.XPathSelectElement("Model") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Model").Value))
                {
                    ActivityProductElement.CreateElement("Model", dealerdata.XPathSelectElement("Model").Value);
                }
                if (dealerdata.XPathSelectElement("VGModelCode") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("VGModelCode").Value))
                {
                    ActivityProductElement.CreateElement("VGModelCode", dealerdata.XPathSelectElement("VGModelCode").Value);
                }
                if (dealerdata.XPathSelectElement("Transmission") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Transmission").Value))
                {
                    ActivityProductElement.CreateElement("Transmission", dealerdata.XPathSelectElement("Transmission").Value);
                }
                if (dealerdata.XPathSelectElement("Colour") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Colour").Value))
                {
                    ActivityProductElement.CreateElement("Colour", dealerdata.XPathSelectElement("Colour").Value);
                }
                if (dealerdata.XPathSelectElement("GradeID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("GradeID").Value))
                {
                    ActivityProductElement.CreateElement("GradeID", dealerdata.XPathSelectElement("GradeID").Value);
                }
                if (dealerdata.XPathSelectElement("ConfigurationID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ConfigurationID").Value))
                {
                    ActivityProductElement.CreateElement("ConfigurationID", dealerdata.XPathSelectElement("ConfigurationID").Value);
                }
                // Added new json tag conversion for IN, MY and TH (SMOSS)
                if (dealerdata.XPathSelectElement("vin17") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("vin17").Value))
                {
                    ActivityProductElement.CreateElement("Vin17", dealerdata.XPathSelectElement("vin17").Value);
                }

                XElement ActivityVehicleReferenceElement = ActivityElement.CreateElement("VehicleReference");
                if (dealerdata.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Brand").Value))
                {
                    ActivityVehicleReferenceElement.CreateElement("Brand", dealerdata.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    ActivityVehicleReferenceElement.CreateElement("Brand", "BMW");
                }
            }
            catch (Exception ex)
            {
                log.Info("error occured While creating the Xml ", ex.Message);
            }
        }
        #endregion

        #region GetSalesHistoryItem(EntityReference owner, bmw_vehicle vehicle)
        protected bmw_saleshistory GetSalesHistoryItem(EntityReference owner, EntityReference vehicle, string salesTypeValue, bmw_dealer dealer, EntityReference team)
        {
            if (owner == null || vehicle == null)
                return null;

            QueryExpression query = new QueryExpression(bmw_saleshistory.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                PageInfo = new PagingInfo
                {
                    Count = 1,
                    PageNumber = 1
                },
                NoLock = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, bmw_saleshistoryState?>(sal => sal.StateCode), ConditionOperator.Equal, (int) bmw_saleshistoryState.Active)
                    }
                }
            };
            if (Account.EntityLogicalName.Equals(owner.LogicalName))
            {
                query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, EntityReference>(sal => sal.bmw_accountid), ConditionOperator.Equal, owner.Id);
            }
            else if (Contact.EntityLogicalName.Equals(owner.LogicalName))
            {
                query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, EntityReference>(sal => sal.bmw_contactid), ConditionOperator.Equal, owner.Id);
            }
            else
            {
                return null;
            }

            query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, EntityReference>(sal => sal.bmw_vehicleid), ConditionOperator.Equal, vehicle.Id);
            if (dealer != null)
            {
                query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, EntityReference>(sal => sal.bmw_salesdealerid), ConditionOperator.Equal, dealer.Id);

            }
            if (team != null && team.Id != Guid.Empty)
            {
                query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, EntityReference>(sal => sal.OwnerId), ConditionOperator.Equal, team.Id);
            }

            if (!String.IsNullOrEmpty(salesTypeValue))
            {
                string licSalesType = this.GetCrmLicValue("SALES_TYPE_SALESHISTORY_8_1_34", salesTypeValue);

                if (!String.IsNullOrEmpty(licSalesType) && Enum.TryParse(licSalesType, out bmw_SalesTypes salesType))
                {
                    query.Criteria.AddCondition(CrmMetadataHelper.GetAttributeLogicalName<bmw_saleshistory, OptionSetValue>(sal => sal.bmw_salestype), ConditionOperator.Equal, Convert.ToInt32(salesType));
                }
            }

            return CommoncrmService.RetrieveMultiple(query)?.Entities?.FirstOrDefault()?.ToEntity<bmw_saleshistory>();
        }
        #endregion

        #region GetExecuteMultipleRequest()
        public ExecuteMultipleRequest GetExecuteMultipleRequest()
        {
            return new ExecuteMultipleRequest
            {
                //// Assign settings that define execution behavior: continue on error, return responses. 
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                //// Create an empty organization request collection.
                Requests = new OrganizationRequestCollection()
            };
        }
        #endregion

        #region CheckAndExecuteBatch(IOrganizationService organizationService, ref ExecuteMultipleRequest request)
        public ExecuteMultipleResponse CheckAndExecuteBatch(IOrganizationService organizationService, ref ExecuteMultipleRequest request)
        {
            if (request == null)
                return null;

            const int pageCount = 500;

            if (request.Requests.Count < pageCount)
                return null;

            ExecuteMultipleResponse exResponse = (ExecuteMultipleResponse)organizationService.Execute(request);
            request = this.GetExecuteMultipleRequest();
            return exResponse;
        }
        #endregion

        /// <summary>
        /// Upload File to File Share
        /// </summary>
        /// <param name="datalocation">J Object of Data location</param>
        /// <param name="xDocument">Xml Document which we need to upload</param>
        /// <returns></returns>
        public bool UploadFileToFileShare(bmw_datalocation datalocation, string content)
        {
            try
            {
                if (datalocation != null && !String.IsNullOrEmpty(content))
                {
                    string fileName = string.Empty;
                    Stream File = null;
                    bool encryptFile = Convert.ToBoolean(datalocation.bmw_end.Value);
                    if (Convert.ToInt32(datalocation.bmw_interface.Value) == Convert.ToInt32(bmw_Interfaces.ShowroomSalesReportExport) ||
                       Convert.ToInt32(datalocation.bmw_interface.Value) == Convert.ToInt32(bmw_Interfaces.CustomerSufficiencyReport))
                    {
                        fileName = datalocation.bmw_sender + "." + datalocation.bmw_receiverpixid + "." + datalocation.bmw_suffix;

                    }
                    else
                    {
                        fileName = datalocation.bmw_sender + "." + datalocation.bmw_receiverpixid + "." + datalocation.bmw_suffix + "." + DateTime.Now.ToString("ddMMyyyyHHmmss");
                    }

                    string Content = content;
                    if (encryptFile)
                    {
                        File = DataEncryptionDecryption(Content, datalocation, encryptFile);
                    }
                    else
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(Content);
                        File = new MemoryStream(byteArray);
                    }
                    if (File != null && datalocation.bmw_exportfolder != null)
                    {
                        UploadFileInFieShare(fileName, File, AzureStorageAccountConnectionString, datalocation.bmw_exportfolder);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Error Occurred While uploading File to to File Share : " + ex.Message);
                return false;
            }
            return false;
        }

        private static byte[] getBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public bool UploadFileToFileShare(string FileName, bmw_datalocation datalocation, string content)
        {
            try
            {
                if (datalocation != null && !String.IsNullOrEmpty(content))
                {
                    Stream File = null;
                    bool encryptFile = Convert.ToBoolean(datalocation.bmw_end != null ? datalocation.bmw_end.Value : false);
                    string fileName = FileName;
                    string Content = content;
                    CommonTraceLog.Info("In EncryptFile Value" + encryptFile);
                    if (encryptFile)
                    {
                        CommonTraceLog.Info("In EncryptFile");
                        File = DataEncryptionDecryption(Content, datalocation, encryptFile);
                        CommonTraceLog.Info("In EncryptFile Name" + fileName);
                    }
                    else
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(Content);
                        File = new MemoryStream(byteArray);
                    }
                    if (File != null && datalocation.bmw_exportfolder != null)
                    {
                        CommonTraceLog.Info("Before UploadFileInFieShare");
                        CommonTraceLog.Info("AzureStorageAccountConnectionString" + AzureStorageAccountConnectionString);
                        CommonTraceLog.Info("bmw_exportfolder" + datalocation.bmw_exportfolder);

                        return UploadFileInFieShare(fileName, File, AzureStorageAccountConnectionString, datalocation.bmw_exportfolder);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Info("Exception While Uploding File to FileShare " + ex.Message.ToString());

                Commonlogger.Error($"Exception While Uploding File to FileShare " + ex, bmw_log_bmw_reasonstate.Error, InterfaceRun, null);
                return false;
            }
            return false;
        }

        public bool UploadFileToSFTP(string FileName, bmw_datalocation datalocation, string content, Logger logger, bmw_interfacerun interfacerun)
        {
            try
            {
                logger.Debug("In Upload file", interfacerun);

                if (datalocation != null && !String.IsNullOrEmpty(content))
                {
                    logger.Debug("File Contins Data", interfacerun);
                    Stream File = null;
                    bool encryptFile = Convert.ToBoolean(datalocation.bmw_end != null ? datalocation.bmw_end.Value : false);

                    string Content = content;

                    if (encryptFile)
                    {
                        File = DataEncryptionDecryption(Content, datalocation, encryptFile);
                    }
                    else
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(Content);
                        File = new MemoryStream(byteArray);
                    }

                    if (File != null && datalocation.bmw_sftpoutputfoldername != null)
                    {
                        logger.Debug("In Export folder", interfacerun);
                        string path = Convert.ToString(datalocation.bmw_sftpoutputfoldername);
                        logger.Debug("Export Path " + path, interfacerun);
                        using (SftpClient client = GetSFTPClientConnection(datalocation))
                        {
                            logger.Debug("In SFTP connection ", interfacerun);
                            client.Connect();
                            logger.Debug("Connected to SFTP", interfacerun);
                            client.ChangeDirectory(path);
                            logger.Debug("Directory changed " + client.WorkingDirectory, interfacerun);

                            logger.Debug("File Path " + path + FileName, interfacerun);
                            client.WriteAllBytes(path + FileName, getBytes(File));

                            logger.Debug("File Put of SFTP  " + client.WorkingDirectory, interfacerun);
                            client.Disconnect();
                            logger.Debug("Disconnected", interfacerun);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception in SFTP connection " + ex.Message, interfacerun);
                return false;
            }
            return false;
        }

        public bool GetFileFromSFTP(bmw_datalocation datalocation, Logger logger)
        {
            try
            {
                if (datalocation.bmw_exportfolder != null && datalocation.bmw_sftpinputfoldername != null)
                {
                    logger.Debug("In Export folder", null);

                    string path = Convert.ToString(datalocation.bmw_sftpinputfoldername);

                    logger.Debug("Export Path " + path, null);

                    using (SftpClient client = GetSFTPClientConnection(datalocation))
                    {
                        client.Connect();

                        client.ChangeDirectory(path);

                        logger.Debug("SFTP connection to server success", this.InterfaceRun);

                        IEnumerable<SftpFile> files = client.ListDirectory(path).ToList<SftpFile>();

                        logger.Debug("SFTP Foreach loop " + files.Count(), this.InterfaceRun);

                        foreach (SftpFile file in files)
                        {
                            if (file.IsRegularFile)
                            {
                                if (file.IsDirectory)
                                {
                                    logger.Debug("fileIsDirectory IGNORE STEP", this.InterfaceRun);
                                    continue;
                                }

                                logger.Info("Reading The file " + file.Name, this.InterfaceRun);

                                using (var fs = new StreamReader(path + file.Name))
                                {
                                    string data = fs.ReadToEnd();
                                    bool encryptFile = Convert.ToBoolean(datalocation.bmw_end != null ? datalocation.bmw_end.Value : false);
                                    if (encryptFile)
                                    {
                                        Stream stream = DataEncryptionDecryption(data, datalocation, false);
                                        UploadFileInFieShare(file.Name, stream, AzureStorageAccountConnectionString, datalocation.bmw_exportfolder);
                                    }
                                    else
                                    {
                                        logger.Info("Uploading File from SFTP to File Share", this.InterfaceRun);
                                        byte[] byteArray = Encoding.UTF8.GetBytes(data);
                                        Stream File = new MemoryStream(byteArray);
                                        UploadFileInFieShare(file.Name, File, AzureStorageAccountConnectionString, datalocation.bmw_exportfolder);
                                    }
                                }
                            }
                        }
                        client.Disconnect();
                        logger.Debug("Connection Disconnected", this.InterfaceRun);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception in SFTP connection " + ex.Message, null);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Download uploaded file from temp folder
        /// </summary>
        /// <param name="AzureConnectionString"></param>
        /// <param name="exportfolderPath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool DownloadFile(string AzureConnectionString, string exportfolderPath, string filename)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureConnectionString);
                CloudFileClient cloudFileClient = storageAccount.CreateCloudFileClient();
                CloudFileShare cloudFileShare = cloudFileClient.GetShareReference(exportfolderPath);
                CloudFileDirectory rootDir = cloudFileShare.GetRootDirectoryReference();
                if (rootDir.Exists())
                {
                    CloudFile cloudFile = rootDir.GetFileReference(filename);
                    if (cloudFile.Exists())
                    {
                        string contents = cloudFile.DownloadText();
                        // convert string to stream
                        byte[] byteArray = Encoding.UTF8.GetBytes(contents);
                        MemoryStream stream = new MemoryStream(byteArray);
                        return UploadFileInFieShare(filename, stream, AzureConnectionString, OutputFileShare);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in while downloading file from temp folder : " + ex.Message);
                Commonlogger.Error($"Exception in while downloading file from temp folder" + ex, bmw_log_bmw_reasonstate.Error, InterfaceRun, null);
                return false;
            }
        }

        /// <summary>
        /// Upload File to File Share 
        /// </summary>
        /// <param name="FileName">File Name</param>
        /// <param name="stream">File Content</param>
        /// <param name="fileShareName">File Share name where we need to upload</param>
        /// <param name="AzureConnectionString">Azure Connection string to connect Azure</param>
        /// <param name="Path">Path After Root Folder</param>
        public bool UploadFileInFieShare(string FileName, Stream stream, string AzureConnectionString, string Path)
        {
            try
            {
                if (!String.IsNullOrEmpty(Path))
                {
                    CommonTraceLog.Info("path " + Path);
                    string storageAccountConnString = AzureConnectionString;
                    var storageAccount = CloudStorageAccount.Parse(storageAccountConnString);
                    var myClient = storageAccount.CreateCloudFileClient();
                    CloudFileShare share = myClient.GetShareReference(Path);
                    CloudFileDirectory rootDir = share.GetRootDirectoryReference();
                    CloudFile file = rootDir.GetFileReference(FileName);

                    Stream fileStream = stream;

                    file.UploadFromStream(fileStream);
                    fileStream.Dispose();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in UploadFileInFieShare : " + ex.Message);
                Commonlogger.Error($"Exception in UploadFileInFieShare" + ex, bmw_log_bmw_reasonstate.Error, InterfaceRun, null);
                return false;
            }
        }

        public string GetFileName(XDocument dataDocument, bmw_datalocation datalocation, string receiverCode, bool IsCounterChk = false)
        {
            try
            {
                string receiverPixId = String.Empty;

                switch (receiverCode)
                {
                    case "DealerPixId":

                        string buno = string.Empty;
                        buno = this.GetDealerNumber(dataDocument);

                        bool parameterValue = ExistsParameter("OPTE_DEALER_PIXID") ? GetCrmParameterValue<bool>("OPTE_DEALER_PIXID") : false;

                        if (parameterValue)
                        {
                            string DealersID = ExistsParameter("OPTE_DEALER_PIXID") ? GetCrmParameterValue<string>("OPTE_DEALER_PIXID") : string.Empty;
                            List<string> AllDealer = DealersID.Split(',').ToList();
                            if (AllDealer.Contains(buno))
                            {
                                receiverPixId = ExistsParameter("OPTE_DEALER_PIXID") ? GetCrmParameterValue<decimal?>("OPTE_DEALER_PIXID").ToString() : string.Empty;
                                receiverPixId = receiverPixId.Split('.')[0];
                            }
                            else
                            {
                                receiverPixId = this.GetDealerPixId(buno);
                            }
                        }
                        else
                        {
                            receiverPixId = this.GetDealerPixId(buno);
                        }

                        if (receiverPixId == null)
                        {
                            CommonTraceLog.Error("Dealer PIX ID not found for dealer with Dealer ID = '" + buno + "'");
                        }
                        break;
                    case "CSV":
                        receiverPixId = datalocation.bmw_receiverpixid;
                        break;
                    case "Xml":
                        receiverPixId = "10000000";
                        break;
                    default:
                        break;
                }

                string bmw_sender = datalocation.bmw_sender;
                string bmw_suffix = datalocation.bmw_suffix;

                if (!string.IsNullOrEmpty(bmw_sender) && !string.IsNullOrEmpty(bmw_suffix) && !string.IsNullOrEmpty(receiverPixId))
                {
                    int? counter = GetCounter(IsCounterChk);
                    return bmw_sender.Trim() + "." + receiverPixId + "." + bmw_suffix.Trim() + "." + counter;
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Error While Genrating File Name " + ex.Message);
            }
            return string.Empty;
        }


        #region GetDealerPixId(string dealerNumber)
        private string GetDealerPixId(string dealerNumber)
        {
            if (String.IsNullOrEmpty(dealerNumber)) return null;

            return (from d in CommonCrmServiceContext.bmw_dealerSet
                    where d.bmw_dealernumber == dealerNumber
                    select d.bmw_pixdealerid).FirstOrDefault();
        }
        #endregion

        #region GetCounter()
        protected int? GetCounter(bool IsCounterChk = false)
        {
            bmw_datalocation dataLoc = GetDataLocationCounter();
            if (dataLoc == null)
            {
                return null;
            }

            int? counter = dataLoc.bmw_lastfilenumber;

            // CrmRetailLeadManagement_8.1.3
            if (IsCounterChk && counter > 89999)
            {
                counter = 10000;
            }

            if (!counter.HasValue)
                return 1;
            if (counter.Value >= 0)
            {
                CommoncrmService.Update(new bmw_datalocation() { Id = dataLoc.Id, bmw_lastfilenumber = counter + 1 });
                return counter.Value + 1;
            }
            return null;
        }
        #endregion

        #region GetInterfaceType()
        public bmw_datalocation GetDataLocationCounter()
        {
            // Define Condition Values
            var QEbmw_datalocation_bmw_interface = this.InterfaceRun.bmw_interface.Value;
            // Instantiate QueryExpression QEbmw_datalocation
            var QEbmw_datalocation = new QueryExpression("bmw_datalocation");
            // Add columns to QEbmw_datalocation.ColumnSet
            QEbmw_datalocation.ColumnSet.AddColumns("bmw_lastfilenumber");
            QEbmw_datalocation.ColumnSet.AddColumns("bmw_interface");
            // Define filter QEbmw_datalocation.Criteria
            QEbmw_datalocation.Criteria.AddCondition("bmw_interface", ConditionOperator.Equal, QEbmw_datalocation_bmw_interface);
            QEbmw_datalocation.Criteria.AddCondition("bmw_lastfilenumber", ConditionOperator.NotNull);
            EntityCollection ecDataLocation = CommoncrmService.RetrieveMultiple(QEbmw_datalocation);

            if (ecDataLocation.Entities.Count > 0)
            {
                return ecDataLocation.Entities[0].ToEntity<bmw_datalocation>();
            }
            return null;
        }
        #endregion

        #region GetDealerNumber(XNode data)
        public string GetDealerNumber(XNode data)
        {
            if (data == null)
            {
                return String.Empty;
            }

            XElement dealerNode = data.XPathSelectElement("/DMSOpportunity/MessageBody/Dealer/CentralDealerID");

            if (dealerNode == null)
            {
                dealerNode = data.XPathSelectElement("/DMSCustomerAddressExport/MessageBody/Dealer/CentralDealerID");
            }

            return dealerNode?.Value;
        }
        #endregion

        public static bool LOVexists(string LOVName)
        {
            return CommonCrmServiceContext.bmw_lovSet.Where(l => l.bmw_name == LOVName).FirstOrDefault() != null;
        }

        /// <summary>
        /// Wrapper Method To create SFTP Client object
        /// </summary>
        /// <param name="datalocation"></param>
        /// <param name="credentials"></param>
        /// <param name="ann"></param>
        /// <returns></returns>
        public static SftpClient GetSFTPClientConnection(bmw_datalocation DataLocationObj)
        {
            return new SftpClient(GetSFTPConnection(DataLocationObj));
        }

        /// <summary>
        /// Method to create ConnectionInfo Object
        /// </summary>
        /// <param name="datalocation"></param>
        /// <param name="credentials"></param>
        /// <returns>Object of ConnectionInfo</returns>
        public static ConnectionInfo GetSFTPConnection(bmw_datalocation DataLocationObj)
        {
            bmw_credentials credentials = (from j in CommonCrmServiceContext.bmw_credentialsSet
                                           where j.Id.Equals(DataLocationObj.bmw_credentials.Id)
                                           select j).FirstOrDefault();

            ConnectionInfo toReturn = null;
            switch (Convert.ToString(credentials.bmw_authenticationtype.Value))
            {
                case /*"Active Directory"*/"174640001":
                    if (!String.IsNullOrEmpty(Convert.ToString(credentials.bmw_password)) &&
                        !String.IsNullOrEmpty(Convert.ToString(credentials.bmw_username)))
                    {
                        toReturn = new ConnectionInfo(Convert.ToString(DataLocationObj[bmw_datalocation.Fields.bmw_sftpserver]), Convert.ToInt32(DataLocationObj[bmw_datalocation.Fields.bmw_sftpport]),
                           Convert.ToString(credentials.bmw_username), GetAuthenticationByCredential(credentials));
                    }
                    CommonTraceLog.Info($"Inside Active Dir switch :");
                    break;

                case /*"PrivateKey"*/"174640004":

                    Annotation ann = GetAnnotation(credentials);
                    if (!String.IsNullOrEmpty(Convert.ToString(credentials.bmw_username)))
                    {
                        toReturn = new ConnectionInfo(Convert.ToString(DataLocationObj[bmw_datalocation.Fields.bmw_sftpserver]), Convert.ToInt32(DataLocationObj[bmw_datalocation.Fields.bmw_sftpport]),
                             Convert.ToString(credentials.bmw_username), GetAuthenticationByPrivateKey(credentials, ann));
                    }
                    CommonTraceLog.Info($"Inside Private key switch :");
                    break;
            }
            return toReturn;
        }

        /// <summary>
        ///  Method that returns array of object of AuthenticationMethod using UserName and Password
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns>>Object of AuthenticationMethod</returns>
        public static AuthenticationMethod[] GetAuthenticationByCredential(bmw_credentials credentials)
        {
            PasswordAuthenticationMethod privateKeyAuthenticationMethod = new PasswordAuthenticationMethod(Convert.ToString(credentials.bmw_username),
               Convert.ToString(credentials.bmw_password));
            return new AuthenticationMethod[] { privateKeyAuthenticationMethod };
        }

        /// <summary>
        ///  Method that returns array of object of AuthenticationMethod using PrivateKey File
        /// </summary>
        /// <returns>Object of AuthenticationMethod</returns>
        public static AuthenticationMethod[] GetAuthenticationByPrivateKey(bmw_credentials credentials, Annotation ann)
        {
            PrivateKeyFile privateKeyFile = null;
            if (ann != null)
            {
                try
                {
                    byte[] byteArray = Convert.FromBase64String(ann.DocumentBody);
                    Stream stream = new MemoryStream(byteArray);
                    privateKeyFile = new PrivateKeyFile(stream);
                }
                catch (Exception ex)
                {
                    CommonTraceLog.Error("Error in GetAuthenticationByPrivateKey : " + ex.Message);
                }
            }
            PrivateKeyAuthenticationMethod privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod(Convert.ToString(credentials.bmw_username),
                privateKeyFile);
            return new AuthenticationMethod[] { privateKeyAuthenticationMethod };
        }

        public static Annotation GetAnnotation(bmw_credentials credentials)
        {
            Annotation annotation = new Annotation();
            if (credentials != null)
            {
                annotation = (from a in CommonCrmServiceContext.AnnotationSet where a.ObjectId.Id == credentials.Id && a.IsDocument == true select a).FirstOrDefault();
            }
            return annotation;
        }

        public static string ConvertCsvFileToJsonObject(string[] lines, char Deli)
        {
            var csv = new List<string[]>();

            foreach (string line in lines)
                csv.Add(line.Split(Deli));

            var properties = lines[0].Split(Deli);

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }

        #region Upload zip file to fileshare
        public static bool UploadZIPFileToFileShare(byte[] compressedBytes, string path, string zipFolderName)
        {
            try
            {
                if (compressedBytes != null && path != null)
                {
                    string storageAccountConnString = AzureStorageAccountConnectionString;
                    var storageAccount = CloudStorageAccount.Parse(storageAccountConnString);
                    var myClient = storageAccount.CreateCloudFileClient();
                    CloudFileShare share = myClient.GetShareReference(path);
                    CloudFileDirectory rootDir = share.GetRootDirectoryReference();
                    ////CloudFile file = rootDir.GetFileReference("DailySalesReport.zip");
                    CloudFile file = rootDir.GetFileReference(zipFolderName);

                    Stream fileStream = new MemoryStream(compressedBytes);

                    file.UploadFromStream(fileStream);
                    fileStream.Dispose();

                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Exception in the UploadZIPFileToFileShare : " + ex.Message);
                return false;
            }
            return false;
        }
        #endregion

        public static void MoveFileToBackup(IOrganizationService organizationService, Logger logger, string myQueueItem, bmw_interfacerun InterfaceRun, string FileName, string taskName, string traceDataContainerName)
        {
            var commonClass = new CommonClass();

            Stream EncrpytedData = commonClass.CheckDataLocationForEncryptionDecryption(myQueueItem, true, InterfaceRun.bmw_interface.Value);

            if (EncrpytedData != null)
            {
                using (StreamReader streamReader = new StreamReader(EncrpytedData))
                {
                    string encryptData = streamReader.ReadToEnd();

                    logger.Debug("Data Decrypted Successfully", bmw_log_bmw_reasonstate.OK, InterfaceRun, null);

                    string storageAccountConnString = AzureStorageAccountConnectionString;
                    string blobURL = string.Empty;
                    PushDataIntoBlob(encryptData, (string.IsNullOrEmpty(FileName)) ? taskName + Guid.NewGuid().ToString() : FileName, traceDataContainerName, storageAccountConnString, "application/xml", out blobURL);
                    InterfaceRun.bmw_bloburl = blobURL;
                    InterfaceRun.bmw_filepath = FileName;
                    organizationService.Update(InterfaceRun);
                }
            }
        }

        public static void MoveFileToError(string myQueueItem, bmw_interfacerun interfaceRun, Logger logger, IOrganizationService organizationService, string fileName, string ErrortraceDataContainerName)
        {
            var commonClass = new CommonClass();
            Stream EncrpytedData = commonClass.CheckDataLocationForEncryptionDecryption(myQueueItem, true, interfaceRun.bmw_interface.Value);
            if (EncrpytedData != null)
            {
                using (StreamReader streamReader = new StreamReader(EncrpytedData))
                {
                    string encryptData = streamReader.ReadToEnd();

                    logger.Debug("Data Encrypted/Decrypted " + fileName, bmw_log_bmw_reasonstate.OK, interfaceRun, null);

                    string storageAccountConnString = AzureStorageAccountConnectionString;
                    string blobURL = string.Empty;

                    CommonClass.PushDataIntoBlob(encryptData, fileName, ErrortraceDataContainerName, storageAccountConnString, "application/xml", out blobURL);

                    interfaceRun.bmw_bloburl = blobURL;
                    interfaceRun.bmw_filepath = fileName;
                    organizationService.Update(interfaceRun);
                }
            }
        }

        public void UpdateInterfaceRunStatus(bool success, string myQueueItem, string FileName, string taskName, bmw_datalocation dataLocationResponse, IOrganizationService organizationService, bmw_interfacerun interfacerun)
        {
            if (success)
            {
                CommonClass.MoveFileToBackup(organizationService, Commonlogger, myQueueItem, interfacerun, FileName, taskName, Convert.ToString(dataLocationResponse.bmw_backupfolder));
                interfacerun.bmw_interfacerunstatus = new OptionSetValue(Convert.ToInt32(bmw_interfacerun_bmw_interfacerunstatus.Completed));
                interfacerun.StateCode = bmw_interfacerunState.Inactive;
                interfacerun.StatusCode = new OptionSetValue(Convert.ToInt32(bmw_interfacerun_StatusCode.Completed));
            }
            else
            {
                CommonClass.MoveFileToError(myQueueItem, interfacerun, Commonlogger, organizationService, FileName, Convert.ToString(dataLocationResponse.bmw_errorfolder));
                interfacerun.bmw_interfacerunstatus = new OptionSetValue(Convert.ToInt32(bmw_interfacerun_bmw_interfacerunstatus.Error));
            }
            organizationService.Update(interfacerun);
        }

        public static int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue must be lower than maxValue");
            }

            long diff = (long)maxValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);

            return (int)(minValue + (ui % diff));
        }


        private static uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }


        private static byte[] GenerateRandomBytes(int bytesNumber)
        {
            RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[bytesNumber];
            csp.GetBytes(buffer);
            return buffer;
        }

        public static DateTime RetrieveCurrentUsersTimeZoneSettings(DateTime date)
        {
            var currentUserSettings = CommoncrmService.RetrieveMultiple(
            new QueryExpression(UserSettings.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                               {
                            new ConditionExpression(UserSettings.Fields.SystemUserId, ConditionOperator.EqualUserId)
                               }
                }
            }).Entities[0].ToEntity<Entity>();

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = (int)currentUserSettings.Attributes[UserSettings.Fields.TimeZoneCode],
                UtcTime = date.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)CommoncrmService.Execute(request);

            return response.LocalTime;
        }

        public static bool PushImageIntoBlob(Stream Data, string fileName, string containerName, string storageAccountConnString, string contentType, out string blobURL)
        {
            blobURL = string.Empty;
            try
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnString);

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create a container for organizing blobs within the storage account.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                blob.Properties.ContentType = contentType;
                blob.UploadFromStreamAsync(Data).Wait();
                blobURL = blob.Uri.AbsoluteUri;
                return true;
            }
            catch (Exception ex)
            {
                CommonTraceLog.Error("Error in PushImageIntoBlob " + ex.Message);
                blobURL = string.Empty;
            }
            return false;
        }

        #region CreateOperationResultWithSubTasks<T>(string operationName)
        public virtual OperationResultWithSubTasks<T> CreateOperationResultWithSubTasks<T>(string operationName)
        {
            operationName = this.DotOperationName(operationName);
            return new OperationResultWithSubTasks<T>(this.CrmUrl/*, this.IntegrationServiceTimer.TaskName,*/, null, this.TimerGuid, this.PluginGuid,
                                       this.PluginAssemblyName, operationName, "", this.BmwLogOperationType, this.InterfaceRun);
        }
        #endregion

        #region TimerGuid
        public Guid TimerGuid { get; set; }
        #endregion

        #region DotOperationName(string operationName)
        private string DotOperationName(string operationName)
        {
            if (operationName.StartsWith(".")) return operationName;

            return "." + operationName;

        }
        #endregion

        #region PluginGuid
        public Guid PluginGuid
        {
            get { return this.pluginGuid; }
            set
            {
                // this property is allowed to be set only once by the plugin-factory
                if (this.pluginGuid != Guid.Empty)
                    return;
                this.pluginGuid = value;
            }
        }
        #endregion

        private Guid pluginGuid;
        private String pluginAssemblyName;

        #region PluginAssemblyName
        public string PluginAssemblyName
        {
            get { return pluginAssemblyName; }
            set
            {
                // this property is allowed to be set only once by the plugin-factory
                if (!String.IsNullOrEmpty(this.pluginAssemblyName))
                    return;

                this.pluginAssemblyName = value;
            }
        }
        #endregion

        #region BmwLogOperationType
        public bmw_log_bmw_operationtype BmwLogOperationType { get; }
        #endregion

        #region AddressUpdateRules
        /// <summary>
        /// This function is used for updating address details in AU as per defined combinations/rules.
        /// </summary>
        /// <param name="xAddress">Address node</param>
        /// <returns></returns>
        public static string AddressUpdateRules(XElement xAddress)
        {
            string FinalAddress = string.Empty;
            XElement xElement;
            string StreetLine1 = string.Empty;
            string StreetLine2 = string.Empty;
            xElement = xAddress.XPathSelectElement("StreetAddressLine2");
            if (xElement != null)
            {
                StreetLine2 = xElement.Value;
            }
            xElement = xAddress.XPathSelectElement("StreetAddress");
            if (xElement != null)
            {
                StreetLine1 = xElement.Value;
            }
            FinalAddress = AddressUpdateRules(StreetLine1, StreetLine2);

            return FinalAddress;
        }
        #endregion

        #region AddressUpdateRules
        /// <summary>
        /// This function is for CR 310 AU Market
        /// This function will update the address fields as per the below defined rules
        /// </summary>
        /// <param name="street1"></param>
        /// <param name="street2"></param>
        /// <returns></returns>
        public static string AddressUpdateRules(string street1, string street2)
        {
            string FinalAddress = string.Empty;
            string StreetLine1 = string.Empty;
            string StreetLine2 = string.Empty;
            if (!string.IsNullOrEmpty(street2))
            {
                StreetLine2 = street2;
            }

            if (!string.IsNullOrEmpty(street1))
            {
                StreetLine1 = street1;
            }

            if (string.IsNullOrEmpty(StreetLine1) && !string.IsNullOrEmpty(StreetLine2))
            {
                return StreetLine2;
            }

            if (string.IsNullOrEmpty(StreetLine2) && !string.IsNullOrEmpty(StreetLine1))
            {
                return StreetLine1;
            }

            if (!string.IsNullOrEmpty(StreetLine1) && Regex.IsMatch(StreetLine1, "^[^a-zA-Z0-9]+$") || !string.IsNullOrEmpty(StreetLine1) && Regex.IsMatch(StreetLine1, "^[0-9]+$"))
            {
                return string.Format("{0} {1}", StreetLine1, StreetLine2);
            }

            if (!string.IsNullOrEmpty(StreetLine2) && Regex.IsMatch(StreetLine2, "^[^a-zA-Z0-9]+$") || !string.IsNullOrEmpty(StreetLine2) && Regex.IsMatch(StreetLine2, "^[0-9]+$"))
            {
                return string.Format("{0} {1}", StreetLine2, StreetLine1);
            }

            if (!string.IsNullOrEmpty(StreetLine1) && !string.IsNullOrEmpty(StreetLine2))
            {
                if (StreetLine1.ToLower().StartsWith("u") && !StreetLine2.ToLower().StartsWith("u") || StreetLine1.ToLower().StartsWith("level") && !StreetLine2.ToLower().StartsWith("level") ||
                    StreetLine1.ToLower().StartsWith("apartment") && !StreetLine2.ToLower().StartsWith("apartment") || StreetLine1.ToLower().StartsWith("hanger") && !StreetLine2.ToLower().StartsWith("hanger") ||
                    StreetLine1.ToLower().StartsWith("po") && !StreetLine2.ToLower().StartsWith("po"))
                {
                    return string.Format("{0} {1}", StreetLine1, StreetLine2);
                }

                if (StreetLine2.ToLower().StartsWith("u") && !StreetLine1.ToLower().StartsWith("u") || StreetLine2.ToLower().StartsWith("level") && !StreetLine1.ToLower().StartsWith("level") ||
                    StreetLine2.ToLower().StartsWith("apartment") && !StreetLine1.ToLower().StartsWith("apartment") || StreetLine2.ToLower().StartsWith("hanger") && !StreetLine1.ToLower().StartsWith("hanger") ||
                    StreetLine2.ToLower().StartsWith("po") && !StreetLine1.ToLower().StartsWith("po"))
                {
                    return string.Format("{0} {1}", StreetLine2, StreetLine1);
                }

                if (!string.IsNullOrEmpty(StreetLine1) && Regex.IsMatch(StreetLine1, "^[0-9]+[a-zA-Z].*$"))
                {
                    return string.Format("{0} {1}", StreetLine1, StreetLine2);
                }

                if (!string.IsNullOrEmpty(StreetLine2) && Regex.IsMatch(StreetLine2, "^[0-9]+[a-zA-Z].*$"))
                {
                    return string.Format("{0} {1}", StreetLine2, StreetLine1);
                }
            }

            if (string.IsNullOrEmpty(FinalAddress))
            {
                FinalAddress = StreetLine2 + " " + StreetLine1;
                return string.Format("{0} {1}", StreetLine2, StreetLine1);
            }

            return FinalAddress;
        }
        #endregion

        public static string PhoneNumberUpdate(string number)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(number, "^[+](61)*"))
            {
                number = number.Replace("+61", "0");
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(number, "^(61)+"))
            {
                number = Regex.Replace(number, "^(61)", "0");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(number, "^0+"))
            {
                number = "0" + number;
            }
            number = number.Replace(" ", string.Empty);
            return number;
        }

        #region
        private static void CreateContentforAutogateAPI(XElement dealerdata, XElement dealerElement, TraceWriter log, string taskName, XElement prospect, XElement vehicle, XElement assignment, string channel)
        {
            try
            {
                XElement opportunityElement = dealerElement.CreateElement("Opportunity");
                if (taskName.Equals("AutogateAPI"))
                {
                    opportunityElement.CreateElement("Channel", channel);
                }
                XElement customerElement = opportunityElement.CreateElement("Customer");
                if (taskName.Equals("AutogateAPI"))
                {
                    customerElement.CreateElement("SalesChannel", "Autogate");
                }

                if (dealerdata.XPathSelectElement("BackendCustomerId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BackendCustomerId").Value))
                {
                    customerElement.CreateElement("BackendCustomerId", dealerdata.XPathSelectElement("BackendCustomerId").Value);
                }
                if (prospect.XPathSelectElement("LastName") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("LastName").Value))
                {
                    customerElement.CreateElement("LastName", prospect.XPathSelectElement("LastName").Value);
                }
                if (prospect.XPathSelectElement("FirstName") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("FirstName").Value))
                {
                    customerElement.CreateElement("FirstName", prospect.XPathSelectElement("FirstName").Value);
                }
                if (prospect.XPathSelectElement("Salutation") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("Salutation").Value))
                {
                    customerElement.CreateElement("Salutation", prospect.XPathSelectElement("Salutation").Value);
                }
                if (prospect.XPathSelectElement("Sex") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("Sex").Value))
                {
                    customerElement.CreateElement("Sex", prospect.XPathSelectElement("Sex").Value);
                }

                //customerElement.CreateElement("ContactType", "1");

                if (prospect.XPathSelectElement("Phone") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("Phone").Value))
                {
                    customerElement.CreateElement("Phone", prospect.XPathSelectElement("Phone").Value);
                }
                if (prospect.XPathSelectElement("MobilePhone") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("MobilePhone").Value))
                {
                    customerElement.CreateElement("MobilePhone", prospect.XPathSelectElement("MobilePhone").Value);
                }
                if (prospect.XPathSelectElement("Email") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("Email").Value))
                {
                    customerElement.CreateElement("Email", prospect.XPathSelectElement("Email").Value);
                }
                if (prospect.XPathSelectElement("BusinessPhone") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("BusinessPhone").Value))
                {
                    customerElement.CreateElement("BusinessPhone", prospect.XPathSelectElement("BusinessPhone").Value);
                }
                if (prospect.XPathSelectElement("MailPhone") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("MailPhone").Value))
                {
                    customerElement.CreateElement("MailPhone", prospect.XPathSelectElement("MailPhone").Value);
                }
                if (prospect.XPathSelectElement("JobTitle") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("JobTitle").Value))
                {
                    customerElement.CreateElement("JobTitle", prospect.XPathSelectElement("JobTitle").Value);
                }
                if (dealerdata.XPathSelectElement("Comments") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Comments").Value))
                {
                    string CommentVar = dealerdata.XPathSelectElement("Comments").Value;
                    if (taskName.Equals("AutogateAPI"))
                    {
                        if (CommentVar.Length <= 9000)
                        {
                            customerElement.CreateElement("Comments", dealerdata.XPathSelectElement("Comments").Value);
                        }
                        else
                        {
                            CommentVar = CommentVar.Substring(0, 9000);
                            customerElement.CreateElement("Comments", CommentVar);
                        }
                    }
                    else
                    {
                        customerElement.CreateElement("Comments", dealerdata.XPathSelectElement("Comments").Value);
                    }
                }
                if (dealerdata.XPathSelectElement("SourceCampaign") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SourceCampaign").Value))
                {
                    customerElement.CreateElement("SourceCampaign", dealerdata.XPathSelectElement("SourceCampaign").Value);
                }
                if (dealerdata.XPathSelectElement("SourceName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("SourceName").Value))
                {
                    customerElement.CreateElement("SourceName", dealerdata.XPathSelectElement("SourceName").Value);
                }
                if (dealerdata.XPathSelectElement("CampaignCode") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CampaignCode").Value))
                {
                    customerElement.CreateElement("CampaignCode", dealerdata.XPathSelectElement("CampaignCode").Value);
                }
                if (dealerdata.XPathSelectElement("CampaignName") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CampaignName").Value))
                {
                    customerElement.CreateElement("CampaignName", dealerdata.XPathSelectElement("CampaignName").Value);
                }
                if (dealerdata.XPathSelectElement("Temperature") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Temperature").Value))
                {
                    customerElement.CreateElement("Temperature", dealerdata.XPathSelectElement("Temperature").Value);
                }
                if (prospect.XPathSelectElement("City") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("City").Value))
                {
                    customerElement.CreateElement("City", prospect.XPathSelectElement("City").Value);
                }
                if (dealerdata.XPathSelectElement("Subject") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Subject").Value))
                {
                    customerElement.CreateElement("Subject", dealerdata.XPathSelectElement("Subject").Value);
                }

                XElement HobbiesElement = customerElement.CreateElement("Hobbies");
                if (dealerdata.XPathSelectElement("Hobby") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Hobby").Value))
                {
                    HobbiesElement.CreateElement("Hobby", dealerdata.XPathSelectElement("Hobby").Value);
                }

                XElement MaterialElement = customerElement.CreateElement("Material");
                if (dealerdata.XPathSelectElement("Material_code") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Material_code").Value))
                {
                    MaterialElement.CreateElement("Material_code", dealerdata.XPathSelectElement("Material_code").Value);
                }

                XElement CustomerProfileDataElement = customerElement.CreateElement("CustomerProfileData");

                if (taskName.Equals("AutogateAPI") && vehicle != null)
                {
                    string freecomment8 = string.Empty;
                    if (vehicle.Element("StockNumber") != null && !String.IsNullOrEmpty(vehicle.Element("StockNumber").Value))
                    {
                        freecomment8 = vehicle.Element("StockNumber").Name + " - " + vehicle.Element("StockNumber").Value;
                    }
                    if (vehicle.Element("Make") != null && !String.IsNullOrEmpty(vehicle.Element("Make").Value) && !String.IsNullOrEmpty(freecomment8))
                    {
                        freecomment8 = freecomment8 + " | " + vehicle.Element("Make").Name + " - " + vehicle.Element("Make").Value;
                    }
                    else if (vehicle.Element("Make") != null && !String.IsNullOrEmpty(vehicle.Element("Make").Value))
                    {
                        freecomment8 = vehicle.Element("Make").Name + " - " + vehicle.Element("Make").Value;
                    }
                    if (vehicle.Element("Model") != null && !String.IsNullOrEmpty(vehicle.Element("Model").Value) && !String.IsNullOrEmpty(freecomment8))
                    {
                        freecomment8 = freecomment8 + " | " + vehicle.Element("Model").Name + " - " + vehicle.Element("Model").Value;
                    }
                    else if (vehicle.Element("Model") != null && !String.IsNullOrEmpty(vehicle.Element("Model").Value))
                    {
                        freecomment8 = vehicle.Element("Model").Name + " - " + vehicle.Element("Model").Value;
                    }
                    if (vehicle.Element("RedbookCode") != null && !String.IsNullOrEmpty(vehicle.Element("RedbookCode").Value) && !String.IsNullOrEmpty(freecomment8))
                    {
                        freecomment8 = freecomment8 + " | " + vehicle.Element("RedbookCode").Name + " - " + vehicle.Element("RedbookCode").Value;
                    }
                    else if (vehicle.Element("RedbookCode") != null && !String.IsNullOrEmpty(vehicle.Element("RedbookCode").Value))
                    {
                        freecomment8 = vehicle.Element("RedbookCode").Name + " - " + vehicle.Element("RedbookCode").Value;
                    }
                    if (dealerdata.Element("Tags") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Tags").Value) && !String.IsNullOrEmpty(freecomment8))
                    {
                        freecomment8 = freecomment8 + " | " + dealerdata.XPathSelectElement("Tags").Value;
                    }
                    else if (dealerdata.Element("Tags") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Tags").Value))
                    {
                        freecomment8 = dealerdata.XPathSelectElement("Tags").Value;
                    }
                    CustomerProfileDataElement.CreateElement("FreeComment8", freecomment8);
                }
                if (taskName.Equals("AutogateAPI") && assignment != null)
                {
                    string freecomment9 = string.Empty;
                    if (assignment.Element("Email") != null && !String.IsNullOrEmpty(assignment.Element("Email").Value))
                    {
                        freecomment9 = assignment.Element("Email").Name + " : " + assignment.Element("Email").Value;
                    }
                    if (assignment.Element("Name") != null && !String.IsNullOrEmpty(assignment.Element("Name").Value) && !String.IsNullOrEmpty(freecomment9))
                    {
                        freecomment9 = freecomment9 + " | " + assignment.Element("Name").Name + " - " + assignment.Element("Name").Value;
                    }
                    else if (assignment.Element("Name") != null && !String.IsNullOrEmpty(assignment.Element("Name").Value))
                    {
                        freecomment9 = assignment.Element("Name").Name + " - " + assignment.Element("Name").Value;
                    }
                    if (assignment.Element("Assigned") != null && !String.IsNullOrEmpty(assignment.Element("Assigned").Value) && !String.IsNullOrEmpty(freecomment9))
                    {
                        freecomment9 = freecomment9 + " | " + assignment.Element("Assigned").Name + " - " + assignment.Element("Assigned").Value;
                    }
                    else if (assignment.Element("Assigned") != null && !String.IsNullOrEmpty(assignment.Element("Assigned").Value))
                    {
                        freecomment9 = assignment.Element("Assigned").Name + " - " + assignment.Element("Assigned").Value;
                    }
                    CustomerProfileDataElement.CreateElement("FreeComment9", freecomment9);
                }

                XElement AddressElement = customerElement.CreateElement("Address");
                if (dealerdata.XPathSelectElement("TypeOfAddress") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TypeOfAddress").Value))
                {
                    AddressElement.CreateElement("TypeOfAddress", dealerdata.XPathSelectElement("TypeOfAddress").Value);
                }
                if (dealerdata.XPathSelectElement("Prefecture") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Prefecture").Value))
                {
                    AddressElement.CreateElement("Prefecture", dealerdata.XPathSelectElement("Prefecture").Value);
                }
                if (prospect.XPathSelectElement("StreetAddress") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("StreetAddress").Value))
                {
                    AddressElement.CreateElement("StreetAddress", prospect.XPathSelectElement("StreetAddress").Value);
                }
                if (prospect.XPathSelectElement("CompanyName") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("CompanyName").Value))
                {
                    AddressElement.CreateElement("CompanyName", prospect.XPathSelectElement("CompanyName").Value);
                }
                if (dealerdata.XPathSelectElement("Town") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Town").Value))
                {
                    AddressElement.CreateElement("Town", dealerdata.XPathSelectElement("Town").Value);
                }
                if (prospect.XPathSelectElement("ZipCode") != null && !String.IsNullOrEmpty(prospect.XPathSelectElement("ZipCode").Value))
                {
                    AddressElement.CreateElement("ZipCode", prospect.XPathSelectElement("ZipCode").Value);
                }

                XElement ProductElement = customerElement.CreateElement("Product");

                if (dealerdata.XPathSelectElement("CurrentlyOwnedVehicle") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CurrentlyOwnedVehicle").Value))
                {
                    ProductElement.CreateElement("CurrentlyOwnedVehicle", dealerdata.XPathSelectElement("CurrentlyOwnedVehicle").Value);
                }

                ProductElement.CreateElement("VehicleOwned", "0");

                if (vehicle.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Brand").Value))
                {
                    ProductElement.CreateElement("Brand", vehicle.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    ProductElement.CreateElement("Brand", "BMW");
                }
                // CR 365 AU Market October Release
                else if (taskName.Equals("AutogateAPI"))
                {
                    if (vehicle.XPathSelectElement("Make") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Make").Value))
                    {
                        string MakeBrand = vehicle.XPathSelectElement("Make").Value;
                        if (MakeBrand.ToUpper() == "BMW")
                        {
                            ProductElement.CreateElement("Brand", "BMW");
                        }
                        else if (MakeBrand.ToUpper() == "BMW I" || MakeBrand.ToUpper() == "BMWI")
                        {
                            ProductElement.CreateElement("Brand", "BMW i");
                        }
                        else if (MakeBrand.ToUpper() == "MINI")
                        {
                            ProductElement.CreateElement("Brand", "MINI");
                        }
                        else if (MakeBrand.ToUpper() == "MOTO")
                        {
                            ProductElement.CreateElement("Brand", "MOTO");
                        }
                        else
                        {
                            ProductElement.CreateElement("Brand", "Other");
                        }
                    }
                    else
                    {
                        ProductElement.CreateElement("Brand", "BMW");
                    }                    
                }                
                if (vehicle.XPathSelectElement("Series") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Series").Value))
                {
                    ProductElement.CreateElement("Series", vehicle.XPathSelectElement("Series").Value);
                }
                if (dealerdata.XPathSelectElement("YearOfConstruction") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("YearOfConstruction").Value))
                {
                    ProductElement.CreateElement("YearOfConstruction", dealerdata.XPathSelectElement("YearOfConstruction").Value);
                }
                if (dealerdata.XPathSelectElement("UpcomingInspectionDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("UpcomingInspectionDate").Value))
                {
                    ProductElement.CreateElement("UpcomingInspectionDate", dealerdata.XPathSelectElement("UpcomingInspectionDate").Value);
                }

                // CR 459 AND CR 458 India Market October Release

                if (dealerdata.XPathSelectElement("plan") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("plan").Value))
                {
                    customerElement.CreateElement("plan", dealerdata.XPathSelectElement("plan").Value);
                }
                if (dealerdata.XPathSelectElement("emi") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("emi").Value))
                {
                    customerElement.CreateElement("emi", dealerdata.XPathSelectElement("emi").Value);
                }
                if (dealerdata.XPathSelectElement("downPayment") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("downPayment").Value))
                {
                    customerElement.CreateElement("downPayment", dealerdata.XPathSelectElement("downPayment").Value);
                }
                if (dealerdata.XPathSelectElement("tenure") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("tenure").Value))
                {
                    customerElement.CreateElement("tenure", dealerdata.XPathSelectElement("tenure").Value);
                }
                if (dealerdata.XPathSelectElement("lastEmi") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("lastEmi").Value))
                {
                    customerElement.CreateElement("lastEmi", dealerdata.XPathSelectElement("lastEmi").Value);
                }

                if (dealerdata.XPathSelectElement("bookingDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("bookingDate").Value))
                {
                    customerElement.CreateElement("bookingDate", dealerdata.XPathSelectElement("bookingDate").Value);
                }

                XElement ActivityElement = opportunityElement.CreateElement("Activity");

                if (dealerdata.XPathSelectElement("ActivityId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityId").Value))
                {
                    ActivityElement.CreateElement("ActivityId", dealerdata.XPathSelectElement("ActivityId").Value);
                }
                if (dealerdata.XPathSelectElement("ActivityType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityType").Value))
                {
                    ActivityElement.CreateElement("ActivityType", dealerdata.XPathSelectElement("ActivityType").Value);
                }
                if (dealerdata.XPathSelectElement("ActivityDueDate") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ActivityDueDate").Value))
                {
                    ActivityElement.CreateElement("ActivityDueDate", dealerdata.XPathSelectElement("ActivityDueDate").Value);
                }
                if (dealerdata.XPathSelectElement("DOR") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("DOR").Value))
                {
                    String result = DateTime.ParseExact(dealerdata.XPathSelectElement("DOR").Value, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    ActivityElement.CreateElement("DOR", result);
                }
                if (dealerdata.XPathSelectElement("RequestId") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("RequestId").Value))
                {
                    ActivityElement.CreateElement("RequestId", dealerdata.XPathSelectElement("RequestId").Value);
                }
                if (dealerdata.XPathSelectElement("TestDriveDate1") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate1").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate1", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate1").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                if (dealerdata.XPathSelectElement("TestDriveDate2") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate2").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate2", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate2").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                if (dealerdata.XPathSelectElement("TestDriveDate3") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TestDriveDate3").Value))
                {
                    ActivityElement.CreateElement("TestDriveDate3", DateTime.Parse(dealerdata.XPathSelectElement("TestDriveDate3").Value).ToString("yyyy-MM-ddThh:mm:ss.szzzz"));
                }
                // new tags
                if (dealerdata.XPathSelectElement("salesStage") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("salesStage").Value))
                {
                    ActivityElement.CreateElement("salesStage", dealerdata.XPathSelectElement("salesStage").Value);
                }

                if (dealerdata.XPathSelectElement("enquiryType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("enquiryType").Value))
                {
                    ActivityElement.CreateElement("enquiryType", dealerdata.XPathSelectElement("enquiryType").Value);
                }
                if (dealerdata.XPathSelectElement("customerType") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("customerType").Value))
                {
                    ActivityElement.CreateElement("customerType", dealerdata.XPathSelectElement("customerType").Value);
                }
                if (dealerdata.XPathSelectElement("BPS") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("BPS").Value))
                {
                    ActivityElement.CreateElement("BPS", dealerdata.XPathSelectElement("BPS").Value);
                }
                if (dealerdata.XPathSelectElement("CurrentCar") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CurrentCar").Value))
                {
                    ActivityElement.CreateElement("CurrentCar", dealerdata.XPathSelectElement("CurrentCar").Value);
                }
                if (dealerdata.XPathSelectElement("CarMonth") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CarMonth").Value))
                {
                    ActivityElement.CreateElement("CarMonth", dealerdata.XPathSelectElement("CarMonth").Value);
                }
                if (dealerdata.XPathSelectElement("CarYear") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("CarYear").Value))
                {
                    ActivityElement.CreateElement("CarYear", dealerdata.XPathSelectElement("CarYear").Value);
                }
                if (dealerdata.XPathSelectElement("TotalKilometer") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("TotalKilometer").Value))
                {
                    ActivityElement.CreateElement("TotalKilometer", dealerdata.XPathSelectElement("TotalKilometer").Value);
                }

                XElement ActivityProductElement = ActivityElement.CreateElement("Product");

                if (vehicle.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Brand").Value))
                {
                    ActivityProductElement.CreateElement("Brand", vehicle.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {                   
                    ActivityProductElement.CreateElement("Brand", "BMW");
                }
                // CR 365 AU Market October Release
                else if(taskName.Equals("AutogateAPI"))
                {
                    if (vehicle.XPathSelectElement("Make") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Make").Value))
                    {
                        string MakeBrand = vehicle.XPathSelectElement("Make").Value;
                        if (MakeBrand.ToUpper() == "BMW")
                        {
                            ActivityProductElement.CreateElement("Brand", "BMW");
                        }
                        else if (MakeBrand.ToUpper() == "BMW I" || MakeBrand.ToUpper() == "BMWI")
                        {
                            ActivityProductElement.CreateElement("Brand", "BMW i");
                        }
                        else if (MakeBrand.ToUpper() == "MINI")
                        {
                            ActivityProductElement.CreateElement("Brand", "MINI");
                        }
                        else if (MakeBrand.ToUpper() == "MOTO")
                        {
                            ActivityProductElement.CreateElement("Brand", "MOTO");
                        }
                        else
                        {
                            ActivityProductElement.CreateElement("Brand", "Other");
                        }
                    }
                    else
                    {
                        ActivityProductElement.CreateElement("Brand", "BMW");
                    }
                }

                if (vehicle.XPathSelectElement("Series") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Series").Value))
                {
                    ActivityProductElement.CreateElement("Series", vehicle.XPathSelectElement("Series").Value);
                }

                ActivityProductElement.CreateElement("BodyType");

                if (vehicle.XPathSelectElement("Model") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("Model").Value))
                {
                    ActivityProductElement.CreateElement("Model", vehicle.XPathSelectElement("Model").Value);
                }
                if (vehicle.XPathSelectElement("VGModelCode") != null && !String.IsNullOrEmpty(vehicle.XPathSelectElement("VGModelCode").Value))
                {
                    ActivityProductElement.CreateElement("VGModelCode", vehicle.XPathSelectElement("VGModelCode").Value);
                }
                if (dealerdata.XPathSelectElement("Transmission") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Transmission").Value))
                {
                    ActivityProductElement.CreateElement("Transmission", dealerdata.XPathSelectElement("Transmission").Value);
                }
                if (dealerdata.XPathSelectElement("Colour") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Colour").Value))
                {
                    ActivityProductElement.CreateElement("Colour", dealerdata.XPathSelectElement("Colour").Value);
                }
                if (dealerdata.XPathSelectElement("GradeID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("GradeID").Value))
                {
                    ActivityProductElement.CreateElement("GradeID", dealerdata.XPathSelectElement("GradeID").Value);
                }
                if (dealerdata.XPathSelectElement("ConfigurationID") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("ConfigurationID").Value))
                {
                    ActivityProductElement.CreateElement("ConfigurationID", dealerdata.XPathSelectElement("ConfigurationID").Value);
                }

                XElement ActivityVehicleReferenceElement = ActivityElement.CreateElement("VehicleReference");
                if (dealerdata.XPathSelectElement("Brand") != null && !String.IsNullOrEmpty(dealerdata.XPathSelectElement("Brand").Value))
                {
                    ActivityVehicleReferenceElement.CreateElement("Brand", dealerdata.XPathSelectElement("Brand").Value);
                }
                else if (taskName.Equals("SMOSSIntegration"))
                {
                    ActivityVehicleReferenceElement.CreateElement("Brand", "BMW");
                }
            }
            catch (Exception ex)
            {
                log.Info("error occured While creating the Xml ", ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// This is for class object creation from json for Import and Export Queue
        /// </summary>
        public class JsonQueues
        {
            public string OPTE813 { get; set; }
            public string CUST8191 { get; set; }
            public string CUST8190 { get; set; }
            public string SALESSERVICE8134 { get; set; }
        }
    }
}