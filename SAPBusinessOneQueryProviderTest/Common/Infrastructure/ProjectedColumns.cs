using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Common
{
	internal sealed class ProjectedColumns
	{
		Expression _projector;
		internal Expression Projector { get { return this._projector; } }
		ReadOnlyCollection<ColumnDeclaration> _columns;
		internal ReadOnlyCollection<ColumnDeclaration> Columns { get { return this._columns; } }

		internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
		{
			this._projector = projector;
			this._columns = columns;
		}
	}
}
