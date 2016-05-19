using System;
using System.Linq;
using System.Reflection;

namespace Common
{
	public static class ReflectionExtensions
	{
		public static string GetAttributeValueBy<T>(this PropertyInfo pi, Func<T, string> expression) where T : Attribute
		{
			T attribute = pi.GetCustomAttributes(typeof(T), false).Cast<T>().SingleOrDefault();

			return expression.Invoke(attribute);
		}

		public static PropertyInfo[] GetPropertiesBySpecific<T>(this Type requestType) where T : Attribute
		{
			return requestType.GetProperties().Where(x => x.IsDefined(typeof(T), false)).ToArray();
		}

		public static B1ObjectType GetCustomB1ObjectAttributeValue(this Type t, Func<CustomB1ObjectAttribute, B1ObjectType> expression)
		{
			CustomB1ObjectAttribute attribute = t.GetCustomAttribute<CustomB1ObjectAttribute>();

			return expression.Invoke(attribute);
		}

		public static string GetCustomB1ObjectAttributeValue(this Type t, Func<CustomB1ObjectAttribute, string> expression)
		{
			CustomB1ObjectAttribute attribute = t.GetCustomAttribute<CustomB1ObjectAttribute>();

			return expression.Invoke(attribute);
		}

		public static string GetCustomFieldAttributeValue(this MemberInfo t, Func<CustomFieldAttribute, string> expression)
		{
			CustomFieldAttribute attribute = t.GetCustomAttribute<CustomFieldAttribute>();

			return expression.Invoke(attribute);
		}
	}
}
