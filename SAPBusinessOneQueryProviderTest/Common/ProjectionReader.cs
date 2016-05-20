using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Common
{
	internal class ProjectionReader<T> : IEnumerable<T>, IEnumerable
	{
		Enumerator _enumerator;

		internal ProjectionReader(DbDataReader reader, Func<ProjectionRow, T> projector)
		{
			this._enumerator = new Enumerator(reader, projector);
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

		// 반복기 자체가 ProjectionRow
		class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
		{
			DbDataReader _reader;
			T _current;
			public T Current { get { return this._current; } }
			object IEnumerator.Current { get { return this._current; } }
			Func<ProjectionRow, T> _projector;

			internal Enumerator(DbDataReader reader, Func<ProjectionRow, T> projector)
			{
				this._reader = reader;
				this._projector = projector;
			}

			// ProjectionRow 의 추상메서드 구현
			public override object GetValue(int index)
			{
				if (index >= 0)
				{
					if (this._reader.IsDBNull(index))
					{
						return null;
					}
					else
					{
						return this._reader.GetValue(index);
					}
				}

				throw new IndexOutOfRangeException();
			}

			public bool MoveNext()
			{
				if (this._reader.Read())
				{
					this._current = this._projector(this);
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
		}
	}
}
