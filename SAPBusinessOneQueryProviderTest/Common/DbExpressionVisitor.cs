using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Common
{
	// TODO: 기존 ExpressionVisitor를 대체할 것이므로, 현재 SAPB1QueryTranslator 도 대체 가능한지 고민 필요
	internal class DbExpressionVisitor : ExpressionVisitor
	{
		public override Expression Visit(Expression expression)
		{
			if (expression == null) return null;

			switch ((DbExpressionType)expression.NodeType)
			{
				case DbExpressionType.Table: return this.VisitTable((TableExpression)expression);
				case DbExpressionType.Column: return this.VisitColumn((ColumnExpression)expression);
				case DbExpressionType.Select: return this.VisitSelect((SelectExpression)expression);
				case DbExpressionType.Projection: return this.VisitProjection((ProjectionExpression)expression);
				default: return base.Visit(expression);
			}
		}

		protected virtual Expression VisitTable(TableExpression table)
		{
			return table;
		}

		protected virtual Expression VisitColumn(ColumnExpression column)
		{
			return column;
		}

		protected virtual Expression VisitSource(Expression source)
		{
			return this.Visit(source);
		}

		protected virtual Expression VisitSelect(SelectExpression select)
		{
			Expression from = this.VisitSource(select.From);
			Expression where = this.Visit(select.Where);

			ReadOnlyCollection<ColumnDeclaration> columns = this.VisitColumnDeclarations(select.Columns);

			if (from != select.From || where != select.Where || columns != select.Columns)
			{
				return new SelectExpression(select.Type, select.Alias, columns, from, where);
			}

			return select;
		}

		protected virtual Expression VisitProjection(ProjectionExpression projection)
		{
			SelectExpression source = (SelectExpression)this.Visit(projection.Source);
			Expression projector = this.Visit(projection.Projector);

			if (source != projection.Source || projector != projection.Projector)
			{
				return new ProjectionExpression(source, projector);
			}

			return projection;
		}

		protected ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
		{
			List<ColumnDeclaration> alternate = null;

			for (int i = 0, n = columns.Count; i < n; i++)
			{
				ColumnDeclaration column = columns[i];
				Expression e = this.Visit(column.Expression);

				if (alternate == null && e != column.Expression)
				{
					alternate = columns.Take(i).ToList();
				}

				if (alternate != null)
				{
					alternate.Add(new ColumnDeclaration(column.Name, e));
				}
			}

			if (alternate != null)
			{
				return alternate.AsReadOnly();
			}

			return columns;
		}
	}
}
