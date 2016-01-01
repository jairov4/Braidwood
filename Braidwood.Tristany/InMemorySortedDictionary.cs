// <copyright company="Skivent Ltda.">
// Copyright (c) 2013, All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Implementation of <see cref="ISortedDictionary{TKey,TValue}" /> in memory
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value</typeparam>
	public class InMemorySortedDictionary<TKey, TValue> : SortedList<TKey, TValue>, ISortedDictionary<TKey,TValue>
	{
		public IEnumerable<KeyValuePair<TKey, TValue>> GetRange(TKey from, TKey to)
		{
			foreach (var v in this)
			{
				if (Comparer.Compare(v.Key, to) <= 0) yield break;
				if (Comparer.Compare(v.Key, from) < 0) continue;
				yield return v;
			}
		}
	}
}