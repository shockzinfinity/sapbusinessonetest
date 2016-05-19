using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Common
{
	// 여기서 recordset 에 대한 sap company 연결이 필요함.
	internal class SAPB1ObjectReader<T> : IEnumerable<T>, IEnumerable where T : class, new()
	{
		Enumerator _enumerator;
		string _query;

		internal SAPB1ObjectReader(string query)
		{
			this._enumerator = new Enumerator(query);
		}

		public IEnumerator<T> GetEnumerator()
		{
			Enumerator e = this._enumerator;

			if (e == null) throw new InvalidOperationException("Cannot enumerate more than once");

			this._enumerator = null;

			return e;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// TODO: 이 부분을 sap recordset 으로 교체 필요
		class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private SAPbobsCOM.Recordset _recordset = SAPCompany.DICompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;

			FieldInfo[] _fields;
			int[] _fieldLookup;
			T _current;
			public T Current { get { return this._current; } }
			object IEnumerator.Current { get { return this._current; } }

			internal Enumerator(string query)
			{
				this._fields = typeof(T).GetFields(); // TODO: DTO 에서 프로퍼티 방식으로 갈 경우, 교체 필요
				_recordset.DoQuery(query);
			}

			public bool MoveNext()
			{
				// recordset 첫번째 행 도달해서 들어옴.
				if (this._fieldLookup == null)
					this.InitFieldLookup();

				T instance = new T();

				for (int i = 0, n = this._fields.Length; i < n; i++)
				{
					int index = this._fieldLookup[i];

					if (index >= 0)
					{
						FieldInfo fi = this._fields[i];

						if (this._recordset.Fields.Item(index).IsNull() == SAPbobsCOM.BoYesNoEnum.tYES)
						{
							fi.SetValue(instance, null);
						}
						else
						{
							fi.SetValue(instance, this._recordset.Fields.Item(index).Value);
						}
					}
				}

				this._current = instance;

				if (_recordset.EoF)
				{
					return false;
				}
				else
				{
					_recordset.MoveNext();
					return true;
				}
			}

			public void Reset()
			{

			}

			#region IDisposable implementation

			// Flag: Dipose 가 이미 호출됐는지 체크
			bool disposed = false;
			// SafeHandle 인스턴스 생성
			SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

			// 노출되는 Dispose 메서드
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			// 하위의 가상 메서드
			protected virtual void Dispose(bool disposing)
			{
				if (disposed)
					return;

				if (disposing)
				{
					handle.Dispose();
					// 관리자원은 여기서 해제
				}

				// 비관리자원은 여기서 해제
				if (_recordset != null) Marshal.ReleaseComObject(_recordset);

				disposed = true;
			}

			#endregion

			private void InitFieldLookup()
			{
				Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

				var fieldInfos = typeof(T).GetFieldsBySpecific<CustomFieldAttribute>();
				for (int i = 0, n = this._recordset.Fields.Count; i < n; i++)
				{
					//map.Add(this._recordset.Fields.Item(i).Name, i);
					// 이부분은 CustomField 의 값을 받아야 함.
					string name = this._recordset.Fields.Item(i).Name;

					map.Add(fieldInfos.Where(x => x.GetCustomFieldAttributeValue(c => c.FieldName == name)).SingleOrDefault().Name, i);
				}

				this._fieldLookup = new int[this._fields.Length];

				for (int i = 0, n = this._fields.Length; i < n; i++)
				{
					int index;
					if (map.TryGetValue(this._fields[i].Name, out index))
					{
						this._fieldLookup[i] = index;
					}
					else
					{
						this._fieldLookup[i] = -1;
					}
				}
			}
		}
	}
}
