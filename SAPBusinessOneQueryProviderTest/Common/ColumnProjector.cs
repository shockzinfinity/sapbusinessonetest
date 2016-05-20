using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	internal class ColumnProjector : ExpressionVisitor
	{
		StringBuilder _sb;
		int _columnIndex;
		ParameterExpression _row;
		static MethodInfo _miGetValue;

		internal ColumnProjector()
		{
			if (_miGetValue == null)
			{
				_miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
			}
		}

		internal ColumnProjection ProjectColumns(Expression expression, ParameterExpression row)
		{
			// TODO: 이 부분도 어트리뷰트 추가해야할 수 있다. SAP 용 따로 가는게 나은가?
			this._sb = new StringBuilder();
			this._row = row;
			Expression selector = this.Visit(expression);

			return new ColumnProjection { Columns = this._sb.ToString(), Selector = selector }; // CAUTION
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
			{
				if (this._sb.Length > 0)
				{
					this._sb.Append(", ");
				}

				this._sb.Append(m.Member.Name); // TODO: 이 부분 어트리뷰트

				// ProjectionRow 의 GetValue 콜
				// TODO: SAPObjectReader 와의 관계 고민 필요
				return Expression.Convert(Expression.Call(this._row, _miGetValue, Expression.Constant(_columnIndex++)), m.Type);
			}
			else
			{
				return base.VisitMember(m);
			}
		}
	}
}
