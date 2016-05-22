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
			TranslateResult result = this.Translate(expression);
			Delegate projector = result.Projector.Compile();

			DbCommand command = this._connection.CreateCommand();
			//command.CommandText = this.Translate(expression);
			command.CommandText = result.CommandText;
			DbDataReader reader = command.ExecuteReader();
			Type elementType = TypeSystem.GetElementType(expression.Type);

			//if (result.Projector != null)
			//{
			//	// projection
			//	Delegate projector = result.Projector.Compile();

			//	return Activator.CreateInstance(
			//		typeof(ProjectionReader<>).MakeGenericType(elementType),
			//		BindingFlags.Instance | BindingFlags.NonPublic, null,
			//		new object[] { reader, projector }, null);
			//}
			//else
			//{
			//	// general
			//	return Activator.CreateInstance(
			//		typeof(ObjectReader<>).MakeGenericType(elementType),
			//		BindingFlags.Instance | BindingFlags.NonPublic, null,
			//		new object[] { reader }, null);
			//}

			return Activator.CreateInstance(
				typeof(ProjectionReader<>).MakeGenericType(elementType),
				BindingFlags.Instance | BindingFlags.NonPublic, null,
				new object[] { reader, projector }, null);
		}

		public override string GetQueryText(Expression expression)
		{
			return this.Translate(expression).CommandText;
		}

		#endregion

		// Projection 추가로 인해 변경부분
		//private string Translate(Expression expression)
		//{
		//	return new QueryTranslator().Translate(expression);
		//}
		private TranslateResult Translate(Expression expression)
		{
			expression = Evaluator.PartialEval(expression);

			ProjectionExpression projection = (ProjectionExpression)new QueryBinder().Bind(expression);
			//ProjectionExpression projection = (ProjectionExpression)new SAPB1QueryBinder().Bind(expression);
			string commandText = new QueryFormatter().Format(projection.Source);
			LambdaExpression projector = new ProjectionBuilder().Build(projection.Projector);

			//return new QueryTranslator().Translate(expression);
			return new TranslateResult { CommandText = commandText, Projector = projector };
		}

		internal class TranslateResult
		{
			internal string CommandText;
			internal LambdaExpression Projector;
		}
	}
}
