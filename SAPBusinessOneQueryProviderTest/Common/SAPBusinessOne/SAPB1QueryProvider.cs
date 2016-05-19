using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	// TODO: 레코드 셋을 쓰므로, 커넥션은 필요하지 않음.
	public class SAPB1QueryProvider : QueryProvider
	{
		public SAPB1QueryProvider() { }

		#region QueryProvider 추상 구현

		public override object Execute(Expression expression)
		{
			string query = this.Translate(expression);
			// Dispose 는 SAPB1ObjectReader 에서 가져가기로 함.

			Type elementType = TypeSystem.GetElementType(expression.Type);

			// 실질적인 리더 부분 - recordset으로 대체 필요
			return Activator.CreateInstance(
				typeof(SAPB1ObjectReader<>).MakeGenericType(elementType),
				BindingFlags.Instance | BindingFlags.NonPublic, null,
				new object[] { query }, null);
		}

		public override string GetQueryText(Expression expression)
		{
			return this.Translate(expression);
		}

		#endregion

		private string Translate(Expression expression)
		{
			return new SAPB1QueryTranslator().Translate(expression);
		}
	}
}
