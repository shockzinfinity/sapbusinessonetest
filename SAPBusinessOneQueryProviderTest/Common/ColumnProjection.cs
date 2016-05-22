using System;
using System.Linq.Expressions;

namespace Common
{
	[Obsolete]
	internal class ColumnProjection
	{
		internal string Columns;
		internal Expression Selector;
	}
}
