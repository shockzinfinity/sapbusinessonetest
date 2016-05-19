using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Common
{
	internal class ObjectReader<T> : IEnumerable<T>, IEnumerable where T : class, new()
	{
		Enumerator _enumerator;

		internal ObjectReader(DbDataReader reader)
		{
			this._enumerator = new Enumerator(reader);
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
			DbDataReader _reader;
			FieldInfo[] _fields;
			int[] _fieldLookup;
			T _current;
			public T Current { get { return this._current; } }
			object IEnumerator.Current { get { return this._current; } }

			internal Enumerator(DbDataReader reader)
			{
				this._reader = reader;
				this._fields = typeof(T).GetFields(); // TODO: DTO 에서 프로퍼티 방식으로 갈 경우, 교체 필요
			}

			public bool MoveNext()
			{
				if (this._reader.Read())
				{
					if (this._fieldLookup == null)
					{
						this.InitFieldLookup(); // TODO: 각 필드에 대한 인덱스설정, 쿼리 결과에 인덱스가 동일
					}

					T instance = new T();

					for (int i = 0, n = this._fields.Length; i < n; i++)
					{
						int index = this._fieldLookup[i];

						if (index >= 0)
						{
							FieldInfo fi = this._fields[i];

							if (this._reader.IsDBNull(index))
							{
								fi.SetValue(instance, null); // 여기서 타입에 대한 고민 필요 (형변환 해야하나? 아니면 디폴트로 셋팅해줘야 하나?)
							}
							else
							{
								fi.SetValue(instance, this._reader.GetValue(index)); // 여기서 타입에 대한 고민 필요
							}
						}
					}

					this._current = instance;

					return true;
				}

				return false;
			}

			public void Reset()
			{

			}

			public void Dispose()
			{
				this._reader.Dispose();
			}

			private void InitFieldLookup()
			{
				Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

				for (int i = 0, n = this._reader.FieldCount; i < n; i++)
				{
					map.Add(this._reader.GetName(i), i);
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
