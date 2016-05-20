using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Common
{
	internal class SAPB1QueryTranslator : ExpressionVisitor
	{
		StringBuilder _sb;
		B1ObjectType _b1ObjectType;

		ParameterExpression _row;
		ColumnProjection _projection;

		internal SAPB1QueryTranslator() { }

		//internal string Translate(Expression expression)
		//{
		//	_b1ObjectType = B1ObjectType.None;

		//	this._sb = new StringBuilder();
		//	this.Visit(expression);

		//	return this._sb.ToString();
		//}

		internal TranslateResult Translate(Expression expression)
		{
			_b1ObjectType = B1ObjectType.None;
			this._sb = new StringBuilder();
			this._row = Expression.Parameter(typeof(ProjectionRow), "row");
			this.Visit(expression);

			return new TranslateResult
			{
				CommandText = this._sb.ToString(),
				Projector = this._projection != null ? Expression.Lambda(this._projection.Selector, this._row) : null
			};
		}

		private static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
			{
				e = ((UnaryExpression)e).Operand;
			}

			return e;
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Queryable))
			{
				if (m.Method.Name == "Where")
				{
					// 여기도 만약 타입이 query 라면 굳이 SELECT * FROM 을 붙일 필요가 있는가?
					// 하지만, WHERE 절 변환 부분이 걸림
					_sb.Append("SELECT * FROM (");
					this.Visit(m.Arguments[0]); // TODO: 이 부분을 어트리뷰트의 컨텐츠를 불러오도록...(e.g. Contents 에 쿼리를 직접 입력할 경우 고려 필요)
					_sb.Append(") AS T WHERE ");

					LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
					this.Visit(lambda.Body);

					return m;
				}
				else if(m.Method.Name == "Select")
				{
					LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
					ColumnProjection projection = new SAPB1ColumnProjector().ProjectColumns(lambda.Body, this._row);

					_sb.Append("SELECT ");
					_sb.Append(projection.Columns);
					_sb.Append(" FROM (");
					this.Visit(m.Arguments[0]);
					_sb.Append(") AS T ");
					this._projection = projection;

					return m;
				}
			}

			throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
		}

		protected override Expression VisitUnary(UnaryExpression u)
		{
			switch (u.NodeType)
			{
				case ExpressionType.Not:
					_sb.Append(" NOT ");
					this.Visit(u.Operand);
					break;
				default:
					throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
			}

			return u;
		}

		protected override Expression VisitBinary(BinaryExpression b)
		{
			_sb.Append("(");
			this.Visit(b.Left);

			switch (b.NodeType)
			{
				case ExpressionType.And:
					_sb.Append(" AND ");
					break;
				case ExpressionType.Or:
					_sb.Append(" OR ");
					break;
				case ExpressionType.Equal:
					_sb.Append(" = ");
					break;
				case ExpressionType.NotEqual:
					_sb.Append(" <> ");
					break;
				case ExpressionType.LessThan:
					_sb.Append(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					_sb.Append(" <= ");
					break;
				case ExpressionType.GreaterThan:
					_sb.Append(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					_sb.Append(" >= ");
					break;
				default:
					throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
			}

			this.Visit(b.Right);
			_sb.Append(")");

			return b;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			IQueryable q = c.Value as IQueryable;

			if (q != null)
			{
				// ElementType 이 어트리뷰트가 지정되어 있게 되면,
				// 타입에 따라 분기를 태워야 함
				// 상황에 맞게 SELECT * FROM 을 넣을지 안넣을지 결정
				//_sb.Append("SELECT * FROM ");
				//_sb.Append(q.ElementType.Name); // TODO: 여기서 어트리뷰트 받아와야 함.

				// IQueryable 에서 Expression.Constant(this) 로 들어오는 것에서 힌트
				_b1ObjectType = q.ElementType.GetCustomB1ObjectAttributeValue(x => x.B1ObjectType);
				string objectContents = q.ElementType.GetCustomB1ObjectAttributeValue(x => x.Contents);

				if (_b1ObjectType == B1ObjectType.Table)
				{
					_sb.Append("SELECT * FROM ");
					_sb.Append(objectContents);
				}
				else if (_b1ObjectType == B1ObjectType.CustomQuery)
				{

				}
				else if (_b1ObjectType == B1ObjectType.Procedure)
				{

				}
				else if (_b1ObjectType == B1ObjectType.View)
				{

				}
				else
				{
					_sb.Append("SELECT * FROM ");
					_sb.Append(objectContents);
				}
			}
			else if (c.Value == null)
			{
				_sb.Append("NULL");
			}
			else
			{
				switch (Type.GetTypeCode(c.Value.GetType()))
				{
					case TypeCode.Boolean:
						_sb.Append(((bool)c.Value) ? 1 : 0);
						break;
					case TypeCode.String:
						_sb.Append("'");
						_sb.Append(c.Value);
						_sb.Append("'");
						break;
					case TypeCode.Object:
						throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
					default:
						_sb.Append(c.Value);
						break;
				}
			}

			return c;
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
			{
				// 여기서 어트리뷰트 받기
				if (_b1ObjectType != B1ObjectType.None)
				{
					string fieldName = m.Member.GetCustomFieldAttributeValue(x => x.FieldName);
					_sb.Append(fieldName);
				}
				else
				{
					_sb.Append(m.Member.Name);
				}

				return m;
			}

			throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
		}
	}
}
