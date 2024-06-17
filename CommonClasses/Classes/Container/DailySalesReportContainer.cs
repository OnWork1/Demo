using System.Collections.Generic;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
    public class DailySalesReportContainer
    {
        public string ReportTargetDate { get; set; }
        public string SendingDate { get; set; }
        public string SendingTime { get; set; }
        public string JPDealerCode { get; set; }
        public string JPOutletCode { get; set; }
        public string BrandCode { get; set; }
        public string BusinessLine { get; set; }
        public string DataType { get; set; }
        public string CustomerType { get; set; }
        public string SeriesModelType { get; set; }
        public string SeriesModelCode { get; set; }
        public string TypeOfCounting { get; set; }
        public string ConfirmationsPast { get; set; }
        public string ConfirmationsCurrent { get; set; }
        public string ExDemo { get; set; }
        public string ConfirmationsAll { get; set; }
        public string PlanCurrentMonth { get; set; }
        public string AHotActual { get; set; }
        public string OverallFutureReg { get; set; }
        public string OverallAll { get; set; }
        public string ImmediateDecision { get; set; }
        public string Future1Actual { get; set; }
        public string Future2Actual { get; set; }
        public string Future3Actual { get; set; }
        public string Future4Actual { get; set; }
        public string NonRTDemo { get; set; }
        public string ShowroomTotal { get; set; }
        public string ShowroomTotalBMWi { get; set; }
        public string ShowroomPersonalInfo { get; set; }
        public string ShowroomPlan { get; set; }
        public string ExternalEventTotal { get; set; }
        public string ExternalEventTotalBMWi { get; set; }
        public string ExternalEventPersonalInfo { get; set; }
        public string ExternalEventPlan { get; set; }
        public string TwoWayCommunication { get; set; }
        public string TwoWayCommunicationTarget { get; set; }
        public string TestDrive { get; set; }
        public string TestDriveTarget { get; set; }
        public string Appraisal { get; set; }
        public string AppraisalTarget { get; set; }
        public string Quotation { get; set; }
        public string QuotationTarget { get; set; }
        public string AHot { get; set; }
        public string AHotTarget { get; set; }
        public string AGDealerCode { get; set; }
        public string AGOutletCode { get; set; }
        public string PO_CurrentMonthRegistration { get; set; }
        public string PO_NextorLaterMonthRegistration { get; set; }
        public string PO_All { get; set; }
        public string B_Hot { get; set; }
        public string TypeOfAttackList { get; set; }
        public string ShowroomVisitActualFigure { get; set; }
        public string ShowroomVisitorCount { get; set; }
        public string ExternalEventVisitorActualFigure { get; set; }
        public string ExternalEventVisitorCount { get; set; }
        public string DataSentDate { get; set; }
        public string DataSentTime { get; set; }


    }

    public class DailySalesReportContainerEL
    {
        public string ReportTargetMonth { get; set; }
        public string SendingDate { get; set; }
        public string SendingTime { get; set; }
        public string JPDealerCode { get; set; }
        public string JPOutletCode { get; set; }
        public string BrandCode { get; set; }
        public string BusinessLine { get; set; }
        public string DataType { get; set; }
        public string CustomerType { get; set; }
        public string CustomerNumber { get; set; }
        public string VIN { get; set; }
        public string SalesConsultantCode { get; set; }
        public string SeriesCodeforSending { get; set; }
        public string SeriesName { get; set; }
        public string AGModelCode { get; set; }
        public string ModelName { get; set; }
        public string OrderDate { get; set; }
        public string RegistrationPlannedDate { get; set; }
        public string RegistrationDateActual { get; set; }
        public string AGDealerCode { get; set; }
        public string AGOutletCode { get; set; }
    }
}
