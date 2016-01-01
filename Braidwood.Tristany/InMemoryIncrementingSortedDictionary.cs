// <copyright company="Skivent Ltda.">
// Copyright (c) 2013, All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Implementation In memory of <see cref="IIncrementingSortedDictionary{TKey,TValue}"/>
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class InMemoryIncrementingSortedDictionary<TKey, TValue> : InMemorySortedDictionary<TKey, TValue>,
		IIncrementingSortedDictionary<TKey, TValue>
	{
		/// <summary>
		/// Increments the values at specified keys
		/// </summary>
		/// <param name="increments">The increments.</param>
		public void Increment(IEnumerable<KeyValuePair<TKey, TValue>> increments)
		{
			Contract.Requires(Contract.ForAll(increments, x => ContainsKey(x.Key)));

			foreach (var increment in increments)
			{
				Increment(increment.Key, increment.Value);
			}
		}

		/// <summary>
		/// Increments the values at specified keys
		/// </summary>
		/// <param name="key"></param>
		/// <param name="increment"></param>
		/// <exception cref="System.NotSupportedException">Type of value not supported</exception>
		public void Increment(TKey key, TValue increment)
		{
			Contract.Requires(ContainsKey(key));

			var v = this[key];
			if (typeof (TValue) == typeof (int))
			{
				var r = (int) (object) v + (int) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (short))
			{
				var r = (short) (object) v + (short) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (byte))
			{
				var r = (byte) (object) v + (byte) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (uint))
			{
				var r = (uint) (object) v + (uint) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (ushort))
			{
				var r = (ushort) (object) v + (ushort) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (long))
			{
				var r = (long) (object) v + (long) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (ulong))
			{
				var r = (ulong) (object) v + (ulong) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (decimal))
			{
				var r = (decimal) (object) v + (decimal) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (float))
			{
				var r = (float) (object) v + (float) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else if (typeof (TValue) == typeof (double))
			{
				var r = (double) (object) v + (double) (object) increment;
				this[key] = (TValue) (object) r;
			}
			else
			{
				throw new NotSupportedException("Type of value not supported: " + typeof (TValue).FullName);
			}
		}
	}
}