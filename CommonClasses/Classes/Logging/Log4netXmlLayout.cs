using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using log4net.Core;
using log4net.Layout;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging
{
	public class Log4netXmlLayout : XmlLayoutBase
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		public Log4netXmlLayout()
		{
			//this.Header = "<html><body><table>";
			//this.Footer = "</table></body></html>";
		}

		// Private Properties

		// Protected Properties

		// Public Properties

		// Private Methods

		// Protected Methods

		// Public Methods

		// Event Handlers


		protected override void FormatXml(XmlWriter writer, LoggingEvent loggingEvent)
		{
			//%date	%level	%property{SourcePluginGuid}	%property{SourcePluginAssemblyName}%property{OperationName}	%property{SourcePluginActivityType}	%property{OperationDescription}	%property{StartTime}	%property{EndTime}	%property{Message}	%property{WasSuccessfull}	%message%newline
			var properties = loggingEvent.GetProperties();
			string[] keys = properties.GetKeys();
			writer.WriteStartElement("event");
			writer.WriteAttributeString("timeStamp", loggingEvent.TimeStamp.ToBinary().ToString());
			writer.WriteAttributeString("threadName", loggingEvent.ThreadName);
			writer.WriteAttributeString("domain", loggingEvent.Domain);
			writer.WriteAttributeString("level", loggingEvent.Level.Name);
			writer.WriteAttributeString("userName", loggingEvent.UserName);

			keys = new string[]
			       	{
			       		"StartTime",
			       		"EndTime",
			       		"SourcePluginAssemblyName",
			       		"OperationName", //usualy method name
			       		"OperationDescription",
			       		"WasSuccessfull",
			       		"ExceptionDump",
			       		"SourcePluginActivityType",
			       		"SourcePluginGuid",
						"log4net:HostName"
			       	};

			WriteProperty(writer, properties, keys);

			writer.WriteStartElement("message");
			writer.WriteString(loggingEvent.RenderedMessage.Replace(", no exception provided", ""));
			writer.WriteEndElement();

			writer.WriteEndElement();
		}

		private void WriteProperty(XmlWriter writer, log4net.Util.PropertiesDictionary properties, string[] propertiesKeys)
		{
			foreach (string key in propertiesKeys)
			{
				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", key);
				writer.WriteString(properties[key] == null ? "--null--" : properties[key].ToString());
				writer.WriteEndElement();
			}
		}
	}
}
