

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml
{
    public class FetchXmlUtils
    {
        #region CreatePagedXml(string xml, string cookie, int page, int count)
        public static string CreatePagedXml(string xml, string cookie, int page, int count)
        {
            // Load document
            XmlDocument doc = new XmlDocument();
            using (StringReader stringReader = new StringReader(xml))
            {
                using (XmlTextReader reader = new XmlTextReader(stringReader))
                {
                    doc.Load(reader);
                }
            }

            return FetchXmlUtils.CreatePagedXml(doc, cookie, page, count);
        }
        #endregion

        #region CreatePagedXml(XmlDocument doc, string cookie, int page, int count)
        public static string CreatePagedXml(XmlDocument doc, string cookie, int page, int count)
        {
            if (doc.DocumentElement == null)
                return String.Empty;

            XmlAttributeCollection attrs = doc.DocumentElement.Attributes;

            if (cookie != null)
            {
                XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = cookie;
                attrs.Append(pagingAttr);
            }

            XmlAttribute pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = Convert.ToString(page);
            attrs.Append(pageAttr);

            XmlAttribute countAttr = doc.CreateAttribute("count");
            countAttr.Value = Convert.ToString(count);
            attrs.Append(countAttr);


            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                {
                    doc.WriteTo(writer);
                }///writer.Close();
            }
            return sb.ToString();
        }
        #endregion

    }
}
