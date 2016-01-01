// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;

namespace Braidwood.Tristany
{
	public interface ISortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Gets the data bounded by speficied range.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns>Bounded data.</returns>
		IEnumerable<KeyValuePair<TKey, TValue>> GetRange(TKey from, TKey to);
	}
}