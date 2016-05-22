using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Common
{
	[Obsolete]
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
				this._fields = typeof(T).GetFields();
			}

			public bool MoveNext()
			{
				if (this._reader.Read())
				{
					if (this._fieldLookup == null)
					{
						this.InitFieldLookup();
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
								fi.SetValue(instance, null);
							}
							else
							{
								fi.SetValue(instance, this._reader.GetValue(index));
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
