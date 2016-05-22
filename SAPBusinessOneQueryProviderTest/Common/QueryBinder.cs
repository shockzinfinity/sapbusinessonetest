﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	internal class QueryBinder : ExpressionVisitor
	{
		ColumnProjector _columnProjector;
		Dictionary<ParameterExpression, Expression> _map;
		int _aliasCount;

		internal QueryBinder()
		{
			this._columnProjector = new ColumnProjector(CanBeColumn);
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
			return "t" + (_aliasCount++);
		}

		private ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
		{
			return this._columnProjector.ProjectColumns(expression, newAlias, existingAlias);
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
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

			return new ProjectionExpression(
				new SelectExpression(resultType, alias, pc.Columns, projection.Source, where),
				pc.Projector);
		}

		private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
		{
			ProjectionExpression projection = (ProjectionExpression)this.Visit(source);
			this._map[selector.Parameters[0]] = projection.Projector;
			Expression expression = this.Visit(selector.Body);
			string alias = this.GetNextAlias();
			ProjectedColumns pc = this.ProjectColumns(expression, alias, GetExistingAlias(projection.Source));

			return new ProjectionExpression(
				new SelectExpression(resultType, alias, pc.Columns, projection.Source, null),
				pc.Projector);
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
			IQueryable q = value as IQueryable;

			return q != null && q.Expression.NodeType == ExpressionType.Constant;
		}

		private string GetTableName(object table)
		{
			IQueryable tableQuery = (IQueryable)table;
			Type rowType = tableQuery.ElementType;

			return rowType.Name; // 여기서 어트리뷰트?
		}

		private string GetColumnName(MemberInfo member)
		{
			return member.Name; // 여기서 어트리뷰트?
		}

		private Type GetColumnType(MemberInfo member)
		{
			// 필드인지 프로퍼티인지 체크 하는 부분
			// 여기서 sap 타입에 대한 변환을 받아야 하는가?
			// TODO: 여기서 변환에 대한 고민 필요

			FieldInfo fi = member as FieldInfo;

			if (fi != null) return fi.FieldType;

			PropertyInfo pi = (PropertyInfo)member;

			return pi.PropertyType;
		}

		private IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
		{
			// 매핑된 멤버 리스트 sap에서는?
			return rowType.GetFields().Cast<MemberInfo>();
		}

		private ProjectionExpression GetTableProjection(object value)
		{
			IQueryable table = (IQueryable)value;

			// 중첩 select 시에 알리아스 받아오는 부분
			string tableAlias = this.GetNextAlias();
			string selectAlias = this.GetNextAlias();
			List<MemberBinding> bindings = new List<MemberBinding>();
			List<ColumnDeclaration> columns = new List<ColumnDeclaration>();

			foreach (MemberInfo mi in this.GetMappedMembers(table.ElementType))
			{
				string columnName = this.GetColumnName(mi);
				Type columnType = this.GetColumnType(mi);
				int ordinal = columns.Count;

				bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName, ordinal)));
				columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, tableAlias, columnName, ordinal)));
			}

			Expression projector = Expression.MemberInit(Expression.New(table.ElementType), bindings);
			Type resultType = typeof(IEnumerable<>).MakeGenericType(table.ElementType);

			return new ProjectionExpression(
				new SelectExpression(resultType,
									selectAlias,
									columns,
									new TableExpression(resultType,
														tableAlias,
														this.GetTableName(table)),
									null),
				projector);
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (this.IsTable(c.Value)) return GetTableProjection(c.Value);

			return c;
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			Expression e;
			if (this._map.TryGetValue(p, out e)) return e;

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

						if (assign != null && MembersMatch(assign.Member, m.Member)) return assign.Expression;
					}
					break;
				case ExpressionType.New:
					NewExpression nex = (NewExpression)source;

					if (nex.Members != null)
					{
						for (int i = 0, n = nex.Members.Count; i < n; i++)
						{
							if (MembersMatch(nex.Members[i], m.Member)) return nex.Arguments[i];
						}
					}
					break;
			}

			if (source == m.Expression) return m;

			return MakeMemberAccess(source, m.Member);
		}

		private bool MembersMatch(MemberInfo a, MemberInfo b)
		{
			if (a == b) return true;

			if (a is MethodInfo && b is PropertyInfo)
			{
				return a == ((PropertyInfo)b).GetGetMethod();
			}
			else if (a is PropertyInfo && b is MethodInfo)
			{
				return ((PropertyInfo)a).GetGetMethod() == b;
			}

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
