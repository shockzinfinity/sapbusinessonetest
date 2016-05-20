using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	public class SAPB1QueryProvider : QueryProvider
	{
		public SAPB1QueryProvider() { }

		#region QueryProvider 추상 구현

		public override object Execute(Expression expression)
		{
			// Translate메서드 시그니처 변경
			TranslateResult result = this.Translate(expression);
			string query = result.CommandText;
			// Dispose 는 SAPB1ObjectReader 에서 가져가기로 함.

			Type elementType = TypeSystem.GetElementType(expression.Type);

			if (result.Projector != null)
			{
				// 프로젝션
				Delegate projector = result.Projector.Compile();

				return Activator.CreateInstance(
					typeof(SAPB1ProjectionReader<>).MakeGenericType(elementType),
					BindingFlags.Instance | BindingFlags.NonPublic, null,
					new object[] { query, projector }, null);
			}
			else
			{
				// 일반
				return Activator.CreateInstance(
					typeof(SAPB1ObjectReader<>).MakeGenericType(elementType),
					BindingFlags.Instance | BindingFlags.NonPublic, null,
					new object[] { query }, null);
			}
		}

		public override string GetQueryText(Expression expression)
		{
			return this.Translate(expression).CommandText;
		}

		#endregion

		private TranslateResult Translate(Expression expression)
		{
			// Evaluator 추가로 인해, expression tree 상의 변수 부분을 대체
			expression = Evaluator.PartialEval(expression);

			return new SAPB1QueryTranslator().Translate(expression);
		}
	}
}
