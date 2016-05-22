using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{
	public abstract class QueryProvider : IQueryProvider
	{
		public QueryProvider() { }

		#region IQueryProvider 구현

		IQueryable IQueryProvider.CreateQuery(Expression expression)
		{
			Type elementType = TypeSystem.GetElementType(expression.Type);

			try
			{
				return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
			}
			catch (TargetInvocationException tie)
			{
				throw tie.InnerException;
			}
		}

		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
		{
			return new Query<TElement>(this, expression);
		}

		object IQueryProvider.Execute(Expression expression)
		{
			return this.Execute(expression);
		}

		TResult IQueryProvider.Execute<TResult>(Expression expression)
		{
			return (TResult)this.Execute(expression);
		}

		#endregion

		public abstract string GetQueryText(Expression expression);
		public abstract object Execute(Expression expression);
	}
}
