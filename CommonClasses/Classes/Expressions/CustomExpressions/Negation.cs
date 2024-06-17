using System;
using System.Text.RegularExpressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions
{
    public class Negation : Expression
    {
        private const string Template = @"!Not\{(?<body>.*)\}";

        Expression Body { get; set; }

        protected override string ExpressionValue
        {
            get { return (!Boolean.Parse(this.Body.Evaluate())).ToString(); }
        }

        public override void Parse(string expression)
        {
            this.ExpressionText = expression;
            Regex expr = new Regex(Negation.Template);
            Match m = expr.Match(expression);
            this.Body = Parser.Parse(m.Groups["body"].Value, this.CrmUrl, this.InterfaceType);
        }

        public static bool IsNegation(string expression)
        {
            Regex expr = new Regex(Negation.Template);
            return expr.IsMatch(expression);
        }
    }
}
