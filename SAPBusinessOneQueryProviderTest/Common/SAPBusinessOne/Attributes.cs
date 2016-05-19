using System;

namespace Common
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class CustomB1ObjectAttribute : Attribute
	{
		// 타입
		// 타입에 따른 이름, 혹은 쿼리, 프로시저

		private B1ObjectType _b1ObjectType;
		public B1ObjectType B1ObjectType { get { return _b1ObjectType; }set { _b1ObjectType = value; } }
		private string _contents = "";
		public string Contents { get { return _contents; } set { _contents = value; } }

		public CustomB1ObjectAttribute(B1ObjectType objectType, string contents)
		{
			_b1ObjectType = objectType;
			_contents = contents;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class CustomFieldAttribute : Attribute
	{
		// 해당 필드명
		private string _fieldName = "";
		public string FieldName { get { return _fieldName; } set { _fieldName = value; } }

		public CustomFieldAttribute(string fieldName)
		{
			_fieldName = fieldName;
		}
	}

	public enum B1ObjectType
	{
		None = 0,
		Table = 1,
		View = 2,
		CustomQuery = 3,
		Procedure = 4,
		Function = 5
	}
}
