using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using BMW.IntegrationService.CrmGenerated;
using log4net;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "StandardLog4Net.config", Watch = true)]

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging
{
    public class Logger
    {
        // Constants

        // Delegates

        // Events

        // Private Fields
        private IOrganizationService crmService;

        private readonly object locker = new object();
        private static Dictionary<string, ILog> dynamicLoggers;
        private static object staticLocker;
        bool logIsDebugEnabled = false, logIsErrorEnabled = false, logIsFatalEnabled = false, logIsInfoEnabled = false, logIsWarnEnabled = false;
        private static TraceWriter log;
        // Static Constructor
        static Logger()
        {
            Logger.staticLocker = new object();
            Logger.dynamicLoggers = new Dictionary<string, ILog>();
        }

        #region Logger(Type type, IOrganizationService service)
        private Logger(Type type, IOrganizationService service)
            : this()
        {
            //this.crmOrganizationUrl = crmOrganizationUrl;

            try
            {
                this.crmService = service;
            }
            catch (Exception ex)
            {
                log.Error("Logger, constructor, could not get crmOrganizationService, logging to crm won't be available", ex);
            }

            //this.Log = GetDyamicLogger(this.FileName);

        }
        #endregion

        #region Logger(string name, string crmOrganizationUrl)
        private Logger(IOrganizationService crmService)
            : this()
        {
            this.crmService = crmService;
            logIsDebugEnabled = ConfigurationManager.AppSettings["IsDebugEnabled"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["IsDebugEnabled"]) : false;
            logIsErrorEnabled = ConfigurationManager.AppSettings["IsErrorEnabled"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["IsErrorEnabled"]) : false;
            logIsFatalEnabled = ConfigurationManager.AppSettings["IsFatalEnabled"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["IsFatalEnabled"]) : false;
            logIsInfoEnabled = ConfigurationManager.AppSettings["IsInfoEnabled"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["IsInfoEnabled"]) : false;
            logIsWarnEnabled = ConfigurationManager.AppSettings["IsWarnEnabled"] != null ? Convert.ToBoolean(ConfigurationManager.AppSettings["IsWarnEnabled"]) : false;
        }
        #endregion

        #region Logger(Type type)
        private Logger(Type type)
            : this()
        {
            //this.Log = log4net.LogManager.GetLogger(type);
        }
        #endregion

        #region Logger(string name)
        private Logger(string name)
            : this()
        {
            if (name.StartsWith("."))
                name = name.Substring(1);

            //this.Log = log4net.LogManager.GetLogger(name);
        }
        #endregion

        #region Logger()
        private Logger()
        {
            //this.InitializeLog4NetProperties();

            //this.crmOrganizationUrl = null;
            this.crmService = null;
            this.locker = new object();

            //XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StandardLog4Net.config")));
        }
        #endregion


        // Private Methods

        // Protected Methods
        #region LogCrmInfo(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        protected void LogCrmInfo(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            this.LogIntoCrm(logEntryName, bmw_log_bmw_operationtype, bmw_log_bmw_logrecordtype.INFO, crmReasonState, entity, logEntryReasonText, exceptionDump, interfaceRun,null);
        }
        #endregion
        #region LogCrmDebug(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        protected void LogCrmDebug(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            this.LogIntoCrm(logEntryName, bmw_log_bmw_operationtype, bmw_log_bmw_logrecordtype.DEBUG, crmReasonState, entity, logEntryReasonText, exceptionDump, interfaceRun,null);
        }
        protected void LogCrmWarn(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState,
           bmw_interfacerun interfaceRun, Entity entity)
        {
            this.LogIntoCrm(logEntryName, bmw_log_bmw_operationtype, bmw_log_bmw_logrecordtype.WARNING, crmReasonState, entity, logEntryReasonText, exceptionDump, interfaceRun,null);
        }
        #endregion
        #region LogCrmError(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        protected void LogCrmError(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity, string jPFileDetails)
        {
            this.LogIntoCrm(logEntryName, bmw_log_bmw_operationtype, bmw_log_bmw_logrecordtype.PROCESSERROR, crmReasonState, entity, logEntryReasonText, exceptionDump, interfaceRun,jPFileDetails);
        }
        #endregion
        #region LogCrmFatal(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        protected void LogCrmFatal(string logEntryName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, string logEntryReasonText, string exceptionDump, bmw_log_bmw_reasonstate crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            this.LogIntoCrm(logEntryName, bmw_log_bmw_operationtype, bmw_log_bmw_logrecordtype.ASSEMBLYERROR, crmReasonState, entity, logEntryReasonText, exceptionDump, interfaceRun,null);
        }
        #endregion

        #region LogIntoCrm(string logEntryName, bmw_log_bmw_operationtype? operationType, bmw_log_bmw_logrecordtype logRecordType, bmw_log_bmw_reasonstate logReasonState, Entity entity, string logEntryReasonText, string exceptionDump, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity)
        protected void LogIntoCrm(
            string logEntryName,
            bmw_log_bmw_operationtype? operationType,
            bmw_log_bmw_logrecordtype logRecordType,
            bmw_log_bmw_reasonstate logReasonState,
            Entity entity,
            string logEntryReasonText,
            string exceptionDump,
            bmw_interfacerun interfaceRun, string jpFileDetails)
        {
            switch (logRecordType)
            {
                case bmw_log_bmw_logrecordtype.DEBUG:
                    if (log != null)
                        log.Info(logEntryReasonText);
                    if (!logIsDebugEnabled) return;                    
                    break;

                case bmw_log_bmw_logrecordtype.INFO:
                    if (log != null)
                        log.Info(logEntryReasonText);
                    if (!logIsInfoEnabled) return;                  
                    break;

                case bmw_log_bmw_logrecordtype.PROCESSERROR:
                    if (log != null && !string.IsNullOrEmpty(jpFileDetails))
                    {
                        log.Error(logEntryReasonText);
                        log.Error(jpFileDetails);
                    }
                    else if (log != null)
                        log.Error(logEntryReasonText);
                    if (!logIsErrorEnabled) return;                    
                    break;

                case bmw_log_bmw_logrecordtype.ASSEMBLYERROR:
                    if (log != null)
                        log.Error(logEntryReasonText);
                    if (!logIsFatalEnabled) return;                    
                    break;
            }

            string entityName = (entity == null ? String.Empty : entity.LogicalName);
            string entityGuid = (entity == null ? String.Empty : entity.Id.ToString());


            if (entity != null && entity.LogicalName != null && entity.LogicalName.StartsWith("###"))
            {
                entityName = entity.LogicalName.Replace("#", "");
                entityGuid = String.Empty;
            }

            bmw_log logEntry = new bmw_log
            {
                bmw_name = logEntryName,
                bmw_operationtype = operationType == null ? null : new OptionSetValue((int)operationType),
                bmw_logrecordtype = new OptionSetValue((int)logRecordType),
                bmw_reasonstate = new OptionSetValue((int)logReasonState),
                bmw_entity = entityName,
                bmw_reasontext = CutEnd(logEntryReasonText, 3800),
                bmw_exception = CutEnd(exceptionDump, 3800),
                bmw_FileDetails = jpFileDetails
            };
            if (interfaceRun != null && interfaceRun.bmw_interfacerunId.HasValue)
            {
                logEntry.bmw_interfacerunid = new EntityReference(interfaceRun.LogicalName, interfaceRun.bmw_interfacerunId.Value);
            }
            else
            {
                logEntry.bmw_reasontext += Environment.NewLine + "Related InterfaceRun could not be determined";
            }

            try
            {
                this.crmService.Create(logEntry);
            }
            catch (Exception ex)
            {
                try
                {
                    logEntry.bmw_interfacerunid = null;
                    this.crmService.Create(logEntry);
                }
                catch (Exception)
                {
                    this.Error("Unable to create Log entry, logEntry.reasonText.Length:" + Environment.NewLine + logEntry.bmw_reasontext.Length.ToString(), ex, null, null);
                }
            }
        }
        #endregion



        // Public static methods

        #region GetLoggerWithCrmLogging(string name, string crmOrganizationUrl)
        public static Logger GetLoggerWithCrmLogging(IOrganizationService crmService, TraceWriter logWriter)
        {
            log = logWriter;

            lock (staticLocker)
            {
                return new Logger(crmService);
            }
        }
        #endregion
        #region GetLoggerWithCrmLogging(Type type, string crmOrganizationUrl)
        public static Logger GetLoggerWithCrmLogging(Type type, IOrganizationService crmService)
        {
            lock (staticLocker)
            {
                return new Logger(type, crmService);
            }
        }
        #endregion

        #region GetLogger(string name)
        public static Logger GetLogger(string name)
        {
            lock (staticLocker)
            {
                return new Logger(name);
            }
        }
        #endregion
        #region GetLogger(Type type)
        public static Logger GetLogger(Type type)
        {
            lock (staticLocker)
            {
                return new Logger(type);
            }
        }
        #endregion

        // Public methods
        #region Error() ...
        //#region Error(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, Entity entity = null)
        //public void Error(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
        //    bmw_interfacerun interfaceRun, Entity entity = null)
        //{
        //    this.Error(message, ex, crmReasonState, interfaceRun,  entity);
        //}

        //#endregion
        #region Error(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        public void Error(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity = null, string jPFileDetails = null)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Error(message, ex);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmError(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, DumpException(ex), (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun,
                                 entity,jPFileDetails);
            }
        }
        #endregion
        #region Error(string message, bmw_interfacerun interfaceRun)
        public void Error(string message, bmw_interfacerun interfaceRun,string jPFileDetails = null)
        {
            this.Error(message, bmw_log_bmw_reasonstate.Error, interfaceRun,null,jPFileDetails);
        }
        #endregion
        #region Error(string message, Exception ex, bmw_interfacerun interfaceRun)
        public void Error(string message, Exception ex, bmw_interfacerun interfaceRun)
        {
            this.Error(message, ex, bmw_log_bmw_reasonstate.Error, interfaceRun);
        }
        #endregion

        #region Error(string message, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, bmw_logbmw_Severity severity, Entity entity)
        public void Error(string message, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity = null, string jPFileDetails = null)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Error(message);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmError(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, null, (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun, entity, jPFileDetails);
            }
        }
        #endregion
        #endregion

        #region Fatal() ...
        #region Fatal(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, Entity entity)
        public void Fatal(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Fatal(message, ex);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmFatal(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, DumpException(ex), (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun,
                                 entity);
            }
        }
        #endregion
        #region Fatal(string message, bmw_interfacerun interfaceRun, Entity entity = null)
        public void Fatal(string message,
            bmw_interfacerun interfaceRun, Entity entity = null)
        {
            this.Fatal(message, bmw_log_bmw_reasonstate.Error, interfaceRun, entity);
        }
        public void Fatal(string message, Exception ex,
            bmw_interfacerun interfaceRun, Entity entity = null)
        {
            this.Fatal(message, ex, null, interfaceRun, entity);
        }
        public void Fatal(string message, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Fatal(message);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmFatal(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, null, (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun, entity);
            }
        }
        #endregion
        #endregion

        #region Info() ...
        #region Info(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, Entity entity = null)

        public void Info(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Info(message, ex);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmInfo("Log", null, message, DumpException(ex), (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun,
                                entity);
            }
        }
        #endregion
        #region Info(string message, bmw_interfacerun interfaceRun)
        public void Info(string message, bmw_interfacerun interfaceRun)
        {
            this.Info(message, bmw_log_bmw_reasonstate.OK, interfaceRun);
        }

        public void Info(string message, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity = null)
        {
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmInfo(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, null, (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun, entity);
            }
        }
        #endregion



        #endregion

        #region Debug() ...
        #region Debug(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, Entity entity = null)


        public void Debug(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Debug(message, ex);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmDebug(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, DumpException(ex), (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun,
                                 entity);
            }
        }
        #endregion
        #region Debug(string message, bmw_interfacerun interfaceRun)
        public void Debug(string message, bmw_interfacerun interfaceRun)
        {
            this.Debug(message, bmw_log_bmw_reasonstate.OK, interfaceRun);
        }



        public void Debug(string message, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity = null)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Debug(message);	
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmDebug(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, null, (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun, entity);
            }
        }
        #endregion

        #endregion

        #region Warn() ...
        #region Warn(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState, bmw_interfacerun interfaceRun, Entity entity = null)


        public void Warn(string message, Exception ex, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Warn(message, ex);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmDebug(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, DumpException(ex), (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun,
                                 entity);
            }
        }
        #endregion
        #region Warn(string message, bmw_interfacerun interfaceRun)
        public void Warn(string message, bmw_interfacerun interfaceRun)
        {
            this.Warn(message, bmw_log_bmw_reasonstate.NotConfirmed, interfaceRun);
        }


        public void Warn(string message, bmw_log_bmw_reasonstate? crmReasonState,
            bmw_interfacerun interfaceRun, Entity entity = null)
        {
            lock (locker)
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                //this.Log.Warn(message);
            }
            if (this.crmService != null && crmReasonState.HasValue)
            {
                this.LogCrmWarn(interfaceRun?.bmw_name == null ? "Log" : interfaceRun?.bmw_name, null, message, null, (bmw_log_bmw_reasonstate)crmReasonState, interfaceRun, entity);
            }
        }
        #endregion

        #endregion
        #region CutEnd(this string input, int newLength)
        /// <summary>
        /// returns the input string, cuts all characters that exceed the given number
        /// </summary>
        /// <param name="input">string to work with</param>
        /// <param name="newLength">maximal length of final string</param>
        /// <returns></returns>
        protected string CutEnd(string input, int newLength)
        {
            if (String.IsNullOrEmpty(input)) return input;
            return input.Length > newLength ? input.Substring(0, newLength) : input;
        }
        #endregion

        public string DumpException(Exception ex)
        {
            string message = "exception type: " + ex.GetType().Name + Environment.NewLine +
                             "message: " + (ex.Message ?? "null") + Environment.NewLine +
                             "stack trace:" + Environment.NewLine +
                             ex.StackTrace ?? "null";
            if (ex.InnerException != null)
            {
                message += Environment.NewLine + "Inner exception:" + Environment.NewLine +
                           DumpException(ex.InnerException);
            }
            return message;
        }
        #region ToBool(this string txt)
        public bool ToBool(string txt)
        {
            if (String.IsNullOrEmpty(txt))
                return false;

            txt = txt.ToLower().Trim();
            if (txt == "1" || txt == "y" || txt == "true" || txt == "ano") return true;
            if (txt == "0" || txt == "n" || txt == "false" || txt == "ne") return false;

            return false;
        }
        #endregion
    }
}
