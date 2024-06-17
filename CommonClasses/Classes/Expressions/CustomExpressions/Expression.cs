using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions.CustomExpressions
{
    public abstract class Expression
    {
        public string CrmUrl { get; set; }
        public bmw_interfacetype InterfaceType { get; set; }

        protected string ExpressionText { get; set; }

        protected abstract string ExpressionValue { get; }

        public virtual string Evaluate()
        {
            return this.ExpressionValue;
        }

        public abstract void Parse(string expression);
    }
}
