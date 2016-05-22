using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	internal class ColumnProjector : DbExpressionVisitor
	{
		// DbExpressionVisitor를 이용하므로, 여기서 컬럼 정의 변경?
		// 아예 ColumnDeclaration 을 따로 가도 괜찮을듯...

		Nominator _nominator;
		Dictionary<ColumnExpression, ColumnExpression> _map;
		List<ColumnDeclaration> _columns;
		HashSet<string> _columnNames;
		HashSet<Expression> _candidates;
		string _existingAlias;
		string _newAlias;
		int _columnCount;

		internal ColumnProjector(Func<Expression, bool> fnCanBeColumn)
		{
			this._nominator = new Nominator(fnCanBeColumn);
		}

		internal ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
		{
			this._map = new Dictionary<ColumnExpression, ColumnExpression>();
			this._columns = new List<ColumnDeclaration>();
			this._columnNames = new HashSet<string>();
			this._newAlias = newAlias;
			this._existingAlias = existingAlias;
			this._candidates = this._nominator.Nominate(expression);

			return new ProjectedColumns(this.Visit(expression), this._columns.AsReadOnly());
		}

		public override Expression Visit(Expression expression)
		{
			if (this._candidates.Contains(expression))
			{
				if (expression.NodeType == (ExpressionType)DbExpressionType.Column)
				{
					ColumnExpression column = (ColumnExpression)expression;
					ColumnExpression mapped;

					if (this._map.TryGetValue(column, out mapped))
					{
						return mapped;
					}

					if (this._existingAlias == column.Alias)
					{
						int ordinal = this._columns.Count;
						string columnName = this.GetUniqueColumnName(column.Name);
						this._columns.Add(new ColumnDeclaration(columnName, column));
						mapped = new ColumnExpression(column.Type, this._newAlias, columnName, ordinal);
						this._map[column] = mapped;
						this._columnNames.Add(columnName);

						return mapped;
					}

					// must be referring to outer scope
					return column;
				}
				else
				{
					string columnName = this.GetNextColumnName();
					int ordinal = this._columns.Count;
					this._columns.Add(new ColumnDeclaration(columnName, expression));

					return new ColumnExpression(expression.Type, this._newAlias, columnName, ordinal);
				}
			}
			else
			{
				return base.Visit(expression);
			}
		}

		private bool IsColumnNameInUse(string name)
		{
			return this._columnNames.Contains(name);
		}

		private string GetUniqueColumnName(string name)
		{
			string baseName = name;
			int suffix = 1;
			while (this.IsColumnNameInUse(name))
			{
				name = baseName + (suffix++);
			}

			return name;
		}

		private string GetNextColumnName()
		{
			return this.GetUniqueColumnName("C" + (_columnCount++));
		}

		class Nominator : DbExpressionVisitor
		{
			Func<Expression, bool> _fnCanBeColumn;
			bool _isBlocked;
			HashSet<Expression> _candidates;

			internal Nominator(Func<Expression, bool> fnCanBeColumn)
			{
				this._fnCanBeColumn = fnCanBeColumn;
			}

			internal HashSet<Expression> Nominate(Expression expression)
			{
				this._candidates = new HashSet<Expression>();
				this._isBlocked = false;
				this.Visit(expression);

				return this._candidates;
			}

			public override Expression Visit(Expression expression)
			{
				if (expression != null)
				{
					bool saveIsBlocked = this._isBlocked;
					this._isBlocked = false;
					base.Visit(expression);
					if (!this._isBlocked)
					{
						if (this._fnCanBeColumn(expression))
						{
							this._candidates.Add(expression);
						}
						else
						{
							this._isBlocked = true;
						}
					}

					this._isBlocked |= saveIsBlocked;
				}

				return expression;
			}
		}
	}

	#region ColumnProjector 재작성 으로 주석

	//internal class ColumnProjector : ExpressionVisitor
	//{
	//	StringBuilder _sb;
	//	int _columnIndex;
	//	ParameterExpression _row;
	//	static MethodInfo _miGetValue;

	//	internal ColumnProjector()
	//	{
	//		if (_miGetValue == null)
	//		{
	//			_miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
	//		}
	//	}

	//	internal ColumnProjection ProjectColumns(Expression expression, ParameterExpression row)
	//	{
	//		// TODO: 이 부분도 어트리뷰트 추가해야할 수 있다. SAP 용 따로 가는게 나은가?
	//		this._sb = new StringBuilder();
	//		this._row = row;
	//		Expression selector = this.Visit(expression);

	//		return new ColumnProjection { Columns = this._sb.ToString(), Selector = selector }; // CAUTION
	//	}

	//	protected override Expression VisitMember(MemberExpression m)
	//	{
	//		if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
	//		{
	//			if (this._sb.Length > 0)
	//			{
	//				this._sb.Append(", ");
	//			}

	//			this._sb.Append(m.Member.Name); // TODO: 이 부분 어트리뷰트

	//			// ProjectionRow 의 GetValue 콜
	//			// TODO: SAPObjectReader 와의 관계 고민 필요
	//			return Expression.Convert(Expression.Call(this._row, _miGetValue, Expression.Constant(_columnIndex++)), m.Type);
	//		}
	//		else
	//		{
	//			return base.VisitMember(m);
	//		}
	//	}
	//}

	#endregion
}
