using System;
using System.Linq.Expressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions
{
	public class ExpressionHelper
	{
		#region GetMemberExpressionFromCondition(ConditionalExpression conditionalExpression)
		public static MemberExpression GetMemberExpressionFromCondition(ConditionalExpression conditionalExpression)
		{
			Expression<Func<bool>> cExpr = Expression.Lambda<Func<bool>>(conditionalExpression.Test);
			Func<bool> cFunction = cExpr.Compile();
			Expression result = cFunction.Invoke() ? conditionalExpression.IfTrue : conditionalExpression.IfFalse;
			if (result is ConditionalExpression)
			{
				result = ExpressionHelper.GetMemberExpressionFromCondition(result as ConditionalExpression);
			}
			return result as MemberExpression;
		}
		#endregion
	}
}
