using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	internal class ProjectionBuilder : DbExpressionVisitor
	{
		ParameterExpression _row;
		private static MethodInfo _miGetValue;

		internal ProjectionBuilder()
		{
			if (_miGetValue == null)
				_miGetValue = typeof(ProjectionRow).GetMethod("GetValue"); // TODO: SAP 쪽에 맞춰서 수정
		}

		internal LambdaExpression Build(Expression expression)
		{
			this._row = Expression.Parameter(typeof(ProjectionRow), "row");
			Expression body = this.Visit(expression);

			return Expression.Lambda(body, this._row);
		}

		protected override Expression VisitColumn(ColumnExpression column)
		{
			return Expression.Convert(Expression.Call(this._row, _miGetValue, Expression.Constant(column.Ordinal)), column.Type);
		}
	}
}
