using System;
using System.Text;
using System.Text.RegularExpressions;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions
{
    public class TextExpression : Expression
    {
        protected override string ExpressionValue
        {
            get { return this.ReplaceInterfaceTypeCounter(this.ReplaceParameters(this.ExpressionText)); }
        }

        private string GetParameterValue(string name, string subpart)
        {
            string result;
            switch (subpart)
            {
                case ".bmw_booleanvalue":
                    CommonClass commonClass = new CommonClass();
                    result = Convert.ToString(CrmParametersManager.Instance.GetParameterValue<bool>(this.CrmUrl, name, null, null));
                    break;
                case ".bmw_datevalue":
                    result = Convert.ToString(CrmParametersManager.Instance.GetParameterValue<DateTime?>(this.CrmUrl, name, null, null));
                    break;
                case ".bmw_decimalvalue":
                    result = Convert.ToString(CrmParametersManager.Instance.GetParameterValue<decimal?>(this.CrmUrl, name, null, null));
                    break;
                default:
                    result = CrmParametersManager.Instance.GetParameterValue<string>(this.CrmUrl, name, null, null);
                    break;
            }

            return result;
        }

        private string GetInterfaceTypeCounter()
        {
            if (this.InterfaceType == null)
            {
                throw new ApplicationException("can not evaluate Interface Type field, when Interface type is empty");
            }

            int? counter = this.InterfaceType.bmw_lastfilenumber;

            if (!counter.HasValue)
                counter = 1;
            if (counter.Value >= 0)
                counter = counter.Value + 1;

            this.InterfaceType.bmw_lastfilenumber = counter;

            return counter.Value.ToString("00000");
        }

        public override void Parse(string expression)
        {
            this.ExpressionText = expression;
        }

        public string ReplaceParameters(string text)
        {
            StringBuilder val = new StringBuilder();
            Regex expr = new Regex(@"\{CrmParameter:(?<parameterName>.*?)(?<subpart>\..*){0,1}\}", RegexOptions.IgnoreCase);

            MatchCollection matches = expr.Matches(text);

            int lastPosition = 0;
            foreach (Match match in matches)
            {
                val.Append(text.Substring(lastPosition, match.Index - lastPosition));
                
                string parameterValue = this.GetParameterValue(match.Groups["parameterName"].Value,
                    match.Groups["subpart"].Success ? match.Groups["subpart"].Value : null);

                val.Append(parameterValue);
                lastPosition = match.Length + match.Index;
            }
            val.Append(text.Substring(lastPosition, text.Length - lastPosition));

            return val.ToString();
        }

        public string ReplaceInterfaceTypeCounter(string text)
        {
            StringBuilder val = new StringBuilder();
            Regex expr = new Regex(@"\{InterfaceType:Counter\}", RegexOptions.IgnoreCase);

            MatchCollection matches = expr.Matches(text);

            int lastPosition = 0;
            foreach (Match match in matches)
            {
                val.Append(text.Substring(lastPosition, match.Index - lastPosition));

                string parameterValue = this.GetInterfaceTypeCounter();

                val.Append(parameterValue);
                lastPosition = match.Index + match.Length;
            }
            val.Append(text.Substring(lastPosition, text.Length - lastPosition));

            return val.ToString();
        }
    }
}
