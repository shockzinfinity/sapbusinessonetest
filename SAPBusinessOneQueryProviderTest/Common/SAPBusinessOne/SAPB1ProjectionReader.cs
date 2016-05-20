using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Common
{
	internal class SAPB1ProjectionReader<T> : IEnumerable<T>, IEnumerable
	{
		Enumerator _enumerator;
		string _query;

		internal SAPB1ProjectionReader(string query, Func<ProjectionRow, T> projector)
		{
			this._enumerator = new Enumerator(query, projector);
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

		class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
		{
			private SAPbobsCOM.Recordset _recordset = SAPCompany.DICompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;

			Func<ProjectionRow, T> _projector;
			T _current;
			public T Current { get { return this._current; } }
			object IEnumerator.Current { get { return this._current; } }

			internal Enumerator(string query, Func<ProjectionRow, T> projector)
			{
				_recordset.DoQuery(query);
				this._projector = projector;
			}

			public override object GetValue(int index)
			{
				if (index >= 0)
				{
					if (this._recordset.Fields.Item(index).IsNull() == SAPbobsCOM.BoYesNoEnum.tYES)
					{
						return null;
					}
					else
					{
						return this._recordset.Fields.Item(index).Value;
					}
				}

				throw new IndexOutOfRangeException();
			}

			public bool MoveNext()
			{
				this._current = this._projector(this);

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
		}
	}
}
