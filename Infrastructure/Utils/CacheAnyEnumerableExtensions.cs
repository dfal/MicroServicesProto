﻿using System.Collections;
using System.Collections.Generic;

namespace Infrastructure.Utils
{
	/// <summary>
	/// Prevents double enumeration (and potential roundtrip to the data source) when checking 
	/// for the presence of items in an enumeration.
	/// </summary>
	static class CacheAnyEnumerableExtensions
	{
		public static IAnyEnumerable<T> AsCachedAnyEnumerable<T>(this IEnumerable<T> source)
		{
			return new AnyEnumerable<T>(source);
		}

		public interface IAnyEnumerable<out T> : IEnumerable<T>
		{
			bool Any();
		}

		private class AnyEnumerable<T> : IAnyEnumerable<T>
		{
			private readonly IEnumerable<T> enumerable;
			private IEnumerator<T> enumerator;
			private bool hasAny;

			public AnyEnumerable(IEnumerable<T> enumerable)
			{
				this.enumerable = enumerable;
			}

			public bool Any()
			{
				this.InitializeEnumerator();

				return this.hasAny;
			}

			public IEnumerator<T> GetEnumerator()
			{
				this.InitializeEnumerator();

				return this.enumerator;
			}

			private void InitializeEnumerator()
			{
				if (this.enumerator != null) return;
				
				var inner = this.enumerable.GetEnumerator();
				this.hasAny = inner.MoveNext();
				this.enumerator = new SkipFirstEnumerator(inner, this.hasAny);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			private class SkipFirstEnumerator : IEnumerator<T>
			{
				private readonly IEnumerator<T> inner;
				private readonly bool hasNext;
				private bool isFirst = true;

				public SkipFirstEnumerator(IEnumerator<T> inner, bool hasNext)
				{
					this.inner = inner;
					this.hasNext = hasNext;
				}

				public T Current { get { return this.inner.Current; } }

				public void Dispose()
				{
					this.inner.Dispose();
				}

				object IEnumerator.Current { get { return this.Current; } }

				public bool MoveNext()
				{
					if (!this.isFirst) return this.inner.MoveNext();
					
					this.isFirst = false;
					return this.hasNext;
				}

				public void Reset()
				{
					this.inner.Reset();
				}
			}
		}
	}
}
