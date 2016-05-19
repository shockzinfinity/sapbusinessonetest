﻿using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	// TODO: 레코드 셋을 쓰므로, 커넥션은 필요하지 않음.
	public class SAPB1QueryProvider : QueryProvider
	{
		DbConnection _connection;

		public SAPB1QueryProvider(DbConnection connection)
		{
			this._connection = connection;
		}

		#region QueryProvider 추상 구현

		public override object Execute(Expression expression)
		{
			DbCommand command = this._connection.CreateCommand(); // TODO: recordset 은 커넥션은 필요없으므로, 구조적으로 다르게 가야함
			command.CommandText = this.Translate(expression); // 여기서 expression 에 대한 쿼리 제네레이션은 끝날듯...이 부분을 기존 쿼리 혹은 프로시저, 테이블과 어떻게 조합할지 결정

			DbDataReader reader = command.ExecuteReader(); // recordset 으로 대체 필요
			Type elementType = TypeSystem.GetElementType(expression.Type);

			// 실질적인 리더 부분 - recordset으로 대체 필요
			return Activator.CreateInstance(
				typeof(SAPB1ObjectReader<>).MakeGenericType(elementType),
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
			return new SAPB1QueryTranslator().Translate(expression);
		}
	}
}
