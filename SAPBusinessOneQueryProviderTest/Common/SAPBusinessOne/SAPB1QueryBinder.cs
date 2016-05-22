using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	// QueryTranslator 대체
	internal class SAPB1QueryBinder : ExpressionVisitor
	{
		B1ObjectType _b1ObjectType;
		ColumnProjector _columnProjector;
		Dictionary<ParameterExpression, Expression> _map;
		int _aliasCount;

		internal SAPB1QueryBinder()
		{
			_b1ObjectType = B1ObjectType.None;
			this._columnProjector = new ColumnProjector(this.CanBeColumn);
		}

		private bool CanBeColumn(Expression expression)
		{
			return expression.NodeType == (ExpressionType)DbExpressionType.Column;
		}

		internal Expression Bind(Expression expression)
		{
			this._map = new Dictionary<ParameterExpression, Expression>();

			return this.Visit(expression);
		}

		private static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
			{
				e = ((UnaryExpression)e).Operand;
			}

			return e;
		}

		private string GetNextAlias()
		{
			return "T" + (_aliasCount++);
		}

		private static string GetExistingAlias(Expression source)
		{
			switch ((DbExpressionType)source.NodeType)
			{
				case DbExpressionType.Table: return ((TableExpression)source).Alias;
				case DbExpressionType.Select: return ((SelectExpression)source).Alias;
				default:
					throw new InvalidOperationException(string.Format("Invalid source node type '{0}'", source.NodeType));
			}
		}

		private bool IsTable(object value)
		{
			// TODO: Table 일 경우 Constant 로 처리되서 Expression 이 해석되도록 이 메서드를 통해 체크함.
			// 이 부분이 B1ObjectType 에 따라 변경이 필요한지에 대한 판단이 필요함.
			IQueryable q = value as IQueryable;
			// table 에 대한 정의가 constant 로 넘어옴, VisitConstant 에서 정의됨.
			return q != null && q.Expression.NodeType == ExpressionType.Constant;
		}

		private string GetTableName(object table)
		{
			IQueryable tableQuery = (IQueryable)table;
			Type rowType = tableQuery.ElementType;

			if (_b1ObjectType == B1ObjectType.Table)
			{
				string objectContents = rowType.GetCustomB1ObjectAttributeValue(x => x.Contents);
				return objectContents;
			}
			else if (_b1ObjectType == B1ObjectType.CustomQuery)
			{
				return string.Empty;
			}
			else if (_b1ObjectType == B1ObjectType.Procedure)
			{
				return string.Empty;
			}
			else if (_b1ObjectType == B1ObjectType.View)
			{
				return string.Empty;
			}
			else
			{
				return rowType.Name;
			}
		}

		private string GetColumnName(MemberInfo member)
		{
			if (_b1ObjectType != B1ObjectType.None)
			{
				return member.GetCustomFieldAttributeValue(x => x.FieldName);
			}
			else
			{
				return member.Name;
			}
		}

		private Type GetColumnType(MemberInfo member)
		{
			// TODO: 이 부분은 SAP 과의 연계를 생각해야 함.
			// 또한 어트리뷰트 형태로 가게 될 경우의 매핑문제도 자동화 할수 있을 것으로 보임.

			FieldInfo fi = member as FieldInfo;

			if (fi != null)
			{
				return fi.FieldType;
			}

			PropertyInfo pi = (PropertyInfo)member;

			return pi.PropertyType;
		}

		private IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
		{
			return rowType.GetFields().Cast<MemberInfo>(); // 매핑된 필드들 리턴
		}

		private ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
		{
			return this._columnProjector.ProjectColumns(expression, newAlias, existingAlias);
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Queryable) ||
				m.Method.DeclaringType == typeof(Enumerable))
			{
				switch (m.Method.Name)
				{
					case "Where": return this.BindWhere(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
					case "Select": return this.BindSelect(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
				}

				throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
			}

			return base.VisitMethodCall(m);
		}

		private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
		{
			ProjectionExpression projection = (ProjectionExpression)this.Visit(source);
			this._map[predicate.Parameters[0]] = projection.Projector;
			Expression where = this.Visit(predicate.Body);
			string alias = this.GetNextAlias();
			ProjectedColumns pc = this.ProjectColumns(projection.Projector, alias, GetExistingAlias(projection.Source));

			return new ProjectionExpression(new SelectExpression(resultType, alias, pc.Columns, projection.Source, where), pc.Projector);
		}

		private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
		{
			ProjectionExpression projection = (ProjectionExpression)this.Visit(source);
			this._map[selector.Parameters[0]] = projection.Projector;
			Expression expression = this.Visit(selector.Body);
			string alias = this.GetNextAlias();
			ProjectedColumns pc = this.ProjectColumns(expression, alias, GetExistingAlias(projection.Source));

			return new ProjectionExpression(new SelectExpression(resultType, alias, pc.Columns, projection.Source, null), pc.Projector);
		}

		private ProjectionExpression GetTableProjection(object value)
		{
			// 테이블 명 받아서 VisitConstant 처리를 위함.
			IQueryable table = (IQueryable)value;

			_b1ObjectType = table.ElementType.GetCustomB1ObjectAttributeValue(x => x.B1ObjectType);

			string tableAlias = this.GetNextAlias();
			string selectAlias = this.GetNextAlias();

			List<MemberBinding> bindings = new List<MemberBinding>();
			List<ColumnDeclaration> columns = new List<ColumnDeclaration>();
			foreach (MemberInfo mi in this.GetMappedMembers(table.ElementType)) // 해당타입의 멤버리스트 반환
			{
				// 이부분을 어트리뷰트로 체인지?
				string columnName = this.GetColumnName(mi);
				Type columnType = this.GetColumnType(mi);
				int ordinal = columns.Count; // 어트리뷰트 지정된 카운트로 대체?

				// TODO: ColumnExpression 생성자 변경 필요
				bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName, ordinal)));
				columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, tableAlias, columnName, ordinal)));
			}

			// 바인딩에 따라사 expression 을 초기화 
			Expression projector = Expression.MemberInit(Expression.New(table.ElementType), bindings);
			Type resultType = typeof(IEnumerable<>).MakeGenericType(table.ElementType);

			return new ProjectionExpression(new SelectExpression(resultType, selectAlias, columns, new TableExpression(resultType, tableAlias, this.GetTableName(table)), null), projector);
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (this.IsTable(c.Value))
				return GetTableProjection(c.Value);

			return c;
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			Expression e;
			if (this._map.TryGetValue(p, out e))
			{
				return e;
			}

			return p;
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			Expression source = this.Visit(m.Expression);

			switch (source.NodeType)
			{
				case ExpressionType.MemberInit:
					MemberInitExpression min = (MemberInitExpression)source;
					for (int i = 0, n = min.Bindings.Count; i < n; i++)
					{
						MemberAssignment assign = min.Bindings[i] as MemberAssignment;
						if (assign != null && MembersMatch(assign.Member, m.Member))
							return assign.Expression;
					}
					break;
				case ExpressionType.New:
					NewExpression nex = (NewExpression)source;
					if (nex.Members != null)
					{
						for (int i = 0, n = nex.Members.Count; i < n; i++)
						{
							if (MembersMatch(nex.Members[i], m.Member))
								return nex.Arguments[i];
						}
					}
					break;
			}

			if (source == m.Expression)
			{
				return m;
			}

			return MakeMemberAccess(source, m.Member);
		}

		private bool MembersMatch(MemberInfo a, MemberInfo b)
		{
			if (a == b) return true;

			if (a is MethodInfo && b is PropertyInfo)
				return a == ((PropertyInfo)b).GetGetMethod();
			else if (a is PropertyInfo && b is MethodInfo)
				return ((PropertyInfo)a).GetGetMethod() == b;

			return false;
		}

		private Expression MakeMemberAccess(Expression source, MemberInfo mi)
		{
			FieldInfo fi = mi as FieldInfo;

			if (fi != null) return Expression.Field(source, fi);

			PropertyInfo pi = (PropertyInfo)mi;

			return Expression.Property(source, pi);
		}
	}
}
