using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging
{
	public class Log4netIntegrationServiceXmlLayout : XmlLayoutSchemaLog4j
	{
		// Constants

		// Delegates

		// Events

		// Private Fields
		private ILoggerRepository repository;

		// Constructors
		private Log4netIntegrationServiceXmlLayout()
		{ }

		public Log4netIntegrationServiceXmlLayout(ILoggerRepository repository)
		{
			this.repository = repository;
		}

		// Private Properties

		// Protected Properties
		protected override void FormatXml(System.Xml.XmlWriter writer, log4net.Core.LoggingEvent loggingEvent)
		{
			base.FormatXml(writer, loggingEvent);
		}

		public override void Format(System.IO.TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
		{
			LoggingEvent newLoggingEvent = loggingEvent;
			if (loggingEvent.MessageObject is string)
			{
				loggingEvent.Fix -= FixFlags.All;
				string message = loggingEvent.MessageObject as string;
				if (!String.IsNullOrEmpty(message))
				{
					message = message.Replace(", no exception provided", "");
					string[] splitted = message.Split(new string[] {", exception information:"}, StringSplitOptions.None);
					if (splitted.Length > 1)
					{
						message = splitted[0];
						string exceptionDump = splitted[1].Trim();

						loggingEvent.GetProperties()["ExceptionDump"] = exceptionDump;
					} 

					newLoggingEvent = new LoggingEvent(typeof(string), this.repository, loggingEvent.LoggerName, loggingEvent.Level, message, loggingEvent.ExceptionObject);
					foreach (DictionaryEntry property in loggingEvent.Properties)
					{
						newLoggingEvent.Properties[property.Key.ToString()] = property.Value;
					}
				}
			}
			base.Format(writer, newLoggingEvent);
		}

		// Public Properties

		// Private Methods

		// Protected Methods

		// Public Methods

		// Event Handlers

	}
}
