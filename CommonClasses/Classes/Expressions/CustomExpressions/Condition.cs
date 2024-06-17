using System;
using System.Text.RegularExpressions;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions
{
    public class Condition : Expression
    {
        private const string ExpressionTemplate = @"\{(?<condition>.*?)\}\s*\?\s*\{(?<trueBranch>.*)\}\s*:\s*\{(?<falseBranch>.*)\}";

        protected override string ExpressionValue
        {
            get
            {
                return Boolean.Parse(this.ConditionExpression.Evaluate()) ? this.TrueValue.Evaluate() : this.FalseValue.Evaluate();
            }
        }

        public Expression ConditionExpression { get; set; }
        public Expression TrueValue { get; set; }
        public Expression FalseValue { get; set; }

        public static bool IsCondition(string expression)
        {
            Regex expr = new Regex(Condition.ExpressionTemplate);
            return expr.IsMatch(expression);
        }

        public override void Parse(string expression)
        {
            Regex expr = new Regex(Condition.ExpressionTemplate);
            Match m = expr.Match(expression);

            this.ExpressionText = expression;
            this.ConditionExpression = Parser.Parse(m.Groups["condition"].Value, this.CrmUrl, this.InterfaceType);
            this.TrueValue = Parser.Parse(m.Groups["trueBranch"].Value, this.CrmUrl, this.InterfaceType);
            this.FalseValue = Parser.Parse(m.Groups["falseBranch"].Value, this.CrmUrl, this.InterfaceType);
        }

        
    }
}
