using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class CustomerSufficiencyReportContainer
    {
        public string JPDealerCode { get; set; }
        public string AGDealerCode { get; set; }
        public string AGOutletCode { get; set; }
        public string JPOutletCode { get; set; }
        public string BrandCode { get; set; }
        public string SalerOrSeriesIdentification { get; set; }
        public string SalesOrSeries { get; set; }
        public string GroupCode { get; set; }
        public string ReportingYearAndMonth { get; set; }
        public string DataSentDate { get; set; }
        public string Categories { get; set; }
        public string ProcessCategories { get; set; }
        public string Plan_ActualIdentification { get; set; }
        public string TotalNumber { get; set; }
        public string DataSentTime { get; set; }

    }
}
