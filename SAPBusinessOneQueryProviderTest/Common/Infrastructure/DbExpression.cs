﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Common
{
	internal enum DbExpressionType
	{
		Table = 1000, // Linq.ExpressionType 과 안 겹치는 것이 중요
		Column,
		Select,
		Projection
	}

	internal class TableExpression : Expression
	{
		string _alias;
		internal string Alias { get { return _alias; } }
		string _name;
		internal string Name { get { return _name; } }
		Type _type;
		public override Type Type { get { return _type; } }
		public override ExpressionType NodeType { get { return (ExpressionType)DbExpressionType.Table; } }

		internal TableExpression(Type type, string alias, string name)
		{
			this._alias = alias;
			this._name = name;
			this._type = type;
		}
	}

	internal class ColumnExpression : Expression
	{
		string _alias;
		internal string Alias { get { return _alias; } }
		string _name;
		internal string Name { get { return _name; } }
		int _ordinal;
		internal int Ordinal { get { return _ordinal; } }
		Type _type;
		public override Type Type { get { return _type; } }
		public override ExpressionType NodeType { get { return (ExpressionType)DbExpressionType.Column; } }

		internal ColumnExpression(Type type, string alias, string name, int ordinal)
		{
			this._type = type;
			this._alias = alias;
			this._name = name;
			this._ordinal = ordinal;
		}
	}

	internal class ColumnDeclaration
	{
		string _name;
		internal string Name { get { return _name; } }
		Expression _expression;
		internal Expression Expression { get { return _expression; } }

		internal ColumnDeclaration(string name, Expression expression)
		{
			this._name = name;
			this._expression = expression;
		}
	}

	internal class SelectExpression : Expression
	{
		string _alias;
		internal string Alias { get { return _alias; } }
		ReadOnlyCollection<ColumnDeclaration> _columns;
		internal ReadOnlyCollection<ColumnDeclaration> Columns { get { return this._columns; } }
		Expression _from;
		internal Expression From { get { return this._from; } }
		Expression _where;
		internal Expression Where { get { return this._where; } }
		Type _type;
		public override Type Type { get { return _type; } }
		public override ExpressionType NodeType { get { return (ExpressionType)DbExpressionType.Select; } }

		internal SelectExpression(Type type, string alias, IEnumerable<ColumnDeclaration> columns, Expression from, Expression where)
		{
			this._type = type;
			this._alias = alias;
			this._columns = columns as ReadOnlyCollection<ColumnDeclaration>;
			if (this._columns == null) this._columns = new List<ColumnDeclaration>(columns).AsReadOnly();

			this._from = from;
			this._where = where;
		}
	}

	internal class ProjectionExpression : Expression
	{
		SelectExpression _source;
		internal SelectExpression Source { get { return this._source; } }
		Expression _projector;
		internal Expression Projector { get { return this._projector; } }
		public override Type Type { get { return _projector.Type; } }
		public override ExpressionType NodeType { get { return (ExpressionType)DbExpressionType.Projection; } }

		internal ProjectionExpression(SelectExpression source, Expression projector)
		{
			this._source = source;
			this._projector = projector;
		}
	}
}
