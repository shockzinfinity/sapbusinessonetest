using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Common
{
	internal class QueryFormatter : DbExpressionVisitor
	{
		StringBuilder _sb;
		int _indent = 2;
		int _depth;

		internal QueryFormatter() { }

		internal string Format(Expression expression)
		{
			this._sb = new StringBuilder();
			this.Visit(expression);

			return this._sb.ToString();
		}

		protected enum Identation
		{
			Same,
			Inner,
			Outer
		}

		internal int IdentationWidth { get { return this._indent; } set { this._indent = value; } }

		private void AppendNewLine(Identation style)
		{
			_sb.AppendLine();
			if (style == Identation.Inner)
			{
				this._depth++;
			}
			else if (style == Identation.Outer)
			{
				this._depth--;
				Debug.Assert(this._depth >= 0);
			}

			for (int i = 0, n = this._depth * this._indent; i < n; i++)
			{
				_sb.Append(" ");
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
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
				case ExpressionType.Equal:
					_sb.Append(" = ");
					break;
				case ExpressionType.GreaterThan:
					_sb.Append(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					_sb.Append(" >= ");
					break;
				case ExpressionType.LessThan:
					_sb.Append(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					_sb.Append(" <= ");
					break;
				case ExpressionType.NotEqual:
					_sb.Append(" <> ");
					break;
				case ExpressionType.Or:
					_sb.Append(" OR ");
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
			if (c.Value == null) _sb.Append("NULL");
			else
			{
				switch (Type.GetTypeCode(c.Value.GetType()))
				{
					case TypeCode.Object:
						throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
					case TypeCode.Boolean:
						_sb.Append(((bool)c.Value) ? 1 : 0);
						break;
					case TypeCode.String:
						_sb.Append("'");
						_sb.Append(c.Value);
						_sb.Append("'");
						break;
					default:
						_sb.Append(c.Value);
						break;
				}
			}

			return c;
		}

		protected override Expression VisitColumn(ColumnExpression column)
		{
			if (!string.IsNullOrEmpty(column.Alias))
			{
				_sb.Append(column.Alias);
				_sb.Append(".");
			}

			_sb.Append(column.Name);

			return column;
		}

		protected override Expression VisitSelect(SelectExpression select)
		{
			_sb.Append("SELECT ");

			for (int i = 0, n = select.Columns.Count; i < n; i++)
			{
				ColumnDeclaration column = select.Columns[i];

				if (i > 0) _sb.Append(", ");

				ColumnExpression c = this.Visit(column.Expression) as ColumnExpression;

				if (c == null || c.Name != select.Columns[i].Name)
				{
					_sb.Append(" AS ");
					_sb.Append(column.Name);
				}
			}

			if (select.From != null)
			{
				this.AppendNewLine(Identation.Same);
				_sb.Append("FROM ");
				this.VisitSource(select.From);
			}

			if (select.Where != null)
			{
				this.AppendNewLine(Identation.Same);
				_sb.Append("WHERE ");
				this.Visit(select.Where);
			}

			return select;
		}

		protected override Expression VisitSource(Expression source)
		{
			switch ((DbExpressionType)source.NodeType)
			{
				case DbExpressionType.Table:
					TableExpression table = (TableExpression)source;
					_sb.Append(table.Name);
					_sb.Append(" AS ");
					_sb.Append(table.Alias);
					break;
				case DbExpressionType.Select:
					SelectExpression select = (SelectExpression)source;
					_sb.Append("(");
					this.AppendNewLine(Identation.Inner);
					this.Visit(select);
					this.AppendNewLine(Identation.Outer);
					_sb.Append(")");
					_sb.Append(" AS ");
					_sb.Append(select.Alias);
					break;
				default:
					throw new InvalidOperationException("Select source is not valid type");
			}

			return source;
		}
	}
}
