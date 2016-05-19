using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	public class DbQueryProvider : QueryProvider
	{
		DbConnection _connection;

		public DbQueryProvider(DbConnection connection)
		{
			this._connection = connection;
		}

		#region QueryProvider 추상 구현

		public override object Execute(Expression expression)
		{
			DbCommand command = this._connection.CreateCommand();
			command.CommandText = this.Translate(expression);

			DbDataReader reader = command.ExecuteReader();
			Type elementType = TypeSystem.GetElementType(expression.Type);

			return Activator.CreateInstance(
				typeof(ObjectReader<>).MakeGenericType(elementType),
				BindingFlags.Instance | BindingFlags.NonPublic, null,
				new object[] { reader }, null);
		}

		public override string GetQueryText(Expression expression)
		{
			return this.Translate(expression);
		}

		#endregion

		private string Translate(Expression expression)
		{
			return new QueryTranslator().Translate(expression);
		}
	}
}
