using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Common
{
	[Obsolete]
	internal class SAPB1ColumnProjector : ExpressionVisitor
	{
		StringBuilder _sb;
		int _columnIndex;
		ParameterExpression _row;
		static MethodInfo _miGetValue;

		internal SAPB1ColumnProjector()
		{
			if (_miGetValue == null)
			{
				_miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
			}
		}

		internal ColumnProjection ProjectColumns(Expression expression, ParameterExpression row)
		{
			this._sb = new StringBuilder();
			this._row = row;
			Expression selector = this.Visit(expression);

			return new ColumnProjection { Columns = this._sb.ToString(), Selector = selector };
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
			{
				if (this._sb.Length > 0)
				{
					this._sb.Append(", ");
				}

				// 어트리뷰트의 컬럼 불러오기
				string fieldName = m.Member.GetCustomFieldAttributeValue(x => x.FieldName);
				this._sb.Append(fieldName);

				return Expression.Convert(Expression.Call(this._row, _miGetValue, Expression.Constant(_columnIndex++)), m.Type);
			}
			else
			{
				return base.VisitMember(m);
			}
		}
	}
}
