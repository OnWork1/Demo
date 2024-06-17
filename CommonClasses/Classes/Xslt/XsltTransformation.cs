using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Caching;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Xslt
{
	public class XsltTransformation
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní proměné
		private readonly Cache cache; 

		// Constructors - Konstruktory

		#region XsltTransformation()
		public XsltTransformation()
		{
			this.cache = new Cache();
		} 
		#endregion

		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti

		// Private Methods - Privátní metody

		// Protected Methods - Protected metody

		// Public Methods - Public metody

		#region Transform(XDocument xmlToTransform, XDocument xslTemplate)
		public XDocument Transform(XDocument xmlToTransform, XDocument xslTemplate)
		{
            SHA256 sha256 = new SHA256CryptoServiceProvider();
			MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(xslTemplate.ToString().Trim()));
			string hash = Encoding.Default.GetString(sha256.ComputeHash(memoryStream)) + xslTemplate.ToString().Trim().Length.ToString(CultureInfo.InvariantCulture);

			XslCompiledTransform xslCompiledTransform;

			if (!this.cache.GetMyCachedItem(hash, out xslCompiledTransform))
			{
				xslCompiledTransform = new XslCompiledTransform(false);
				xslCompiledTransform.Load(xslTemplate.CreateReader());

				this.cache.AddToCache(hash, xslCompiledTransform, Cache.CachePriority.NotRemovable);
			}

			XDocument ret = new XDocument();
			using (XmlWriter writer = ret.CreateWriter())
			{
				xslCompiledTransform.Transform(xmlToTransform.CreateReader(), writer);
			}

			return ret;
		} 
		#endregion

		#region Transform(string xmlToTransform, string xslTemplate)
		public string Transform(string xmlToTransform, string xslTemplate)
		{
			return this.Transform(XDocument.Parse(xmlToTransform), XDocument.Parse(xslTemplate)).ToString();
		} 
		#endregion

		// Event Handlers - Události
	}
}
