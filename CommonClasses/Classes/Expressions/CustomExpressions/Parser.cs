using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions
{
    public class Parser
    {
        public static Expression Parse(string expression, string crmUrl, bmw_interfacetype interfaceType = null)
        {
            Expression result = Negation.IsNegation(expression) ? new Negation() : (Condition.IsCondition(expression) ? (Expression)new Condition() : new TextExpression());
            result.CrmUrl = crmUrl;
            result.InterfaceType = interfaceType;
            result.Parse(expression);
            return result;
        }

        public static bool IsExpression(string expression, out string body)
        {
            Regex expr = new Regex("^\\s*Expr\\s*?=\\s*?(?<body>\\S.*)");

            Match match = expr.Match(expression);
            if (match.Success)
            {
                body = match.Groups["body"].Success ? match.Groups["body"].Value : null;
                return true;
            }
            body = null;
            return false;
        }
    }
}
