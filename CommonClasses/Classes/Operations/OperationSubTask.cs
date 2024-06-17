using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations
{	
	public class OperationSubTask        
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors

		#region OperationSubTask(XContainer data)
		public OperationSubTask(XContainer data)
		{
			if (data == null)
				return;

			this.Tag = new XDocument(new XElement("subtask"));
			this.Tag.Root.Add(data.Elements());
			this.WasSuccessful = true;
		} 
		#endregion

		// Private Properties

		// Protected Properties

		// Public Properties

		public Guid Guid { get; set; }
		public bool WasSuccessful { get; set; }
		public string Note { get; set; }
		public XDocument Tag { get; private set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

        public Dictionary <bmw_interfacerun,List<string>> InterfaceRunList { get; set; }

		// Private Methods

		#region ResolveXmlData(XDocument data)
		private static XDocument ResolveXmlData(XDocument data)
		{						
			if (data == null)
				return null;

			XElement xXmlFile = data.Root.XPathSelectElement("data/content");

			if (xXmlFile == null)
			{
				return data;
			}

			XCData xCdata = new XCData(xXmlFile.Value);
			string fileContent = xCdata.Value;

			XDocument fileDocument;
			try
			{
				fileDocument = XDocument.Parse(fileContent); // zhavaruje, pokud utf-8 file obsahuje BOM
			}
			catch (Exception)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(fileContent);
				fileContent = Encoding.UTF8.GetString(bytes.Skip(3).ToArray());

				fileDocument = XDocument.Parse(fileContent);
			}

			return fileDocument;
		}
		#endregion

		#region ResolveData(XDocument data)
		private static string ResolveData(XDocument data)
		{
			if (data == null)
				return String.Empty;

			XElement xXmlFile = data.Root.XPathSelectElement("data/content");

			if (xXmlFile == null)
			{
				return String.Empty;
			}

			XCData xCdata = new XCData(xXmlFile.Value);
			return xCdata.Value;
		} 
		#endregion

		// Protected Methods

		// Public Methods

		#region GetContentDocument()
		public XDocument GetContentDocument()
		{
			return OperationSubTask.ResolveXmlData(this.Tag);
		} 
		#endregion

		#region GetContent()
		public string GetContent()
		{
			return OperationSubTask.ResolveData(this.Tag);
		} 
		#endregion 

		// Event Handlers

	}
}
