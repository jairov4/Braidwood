using System.Collections.Generic;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Sorted Dictionary able to increment a value
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public interface IIncrementingSortedDictionary<TKey, TValue> : ISortedDictionary<TKey, TValue>
	{
		/// <summary>
		/// Increments the values at specified keys
		/// </summary>
		/// <param name="increments">The increments.</param>
		void Increment(IEnumerable<KeyValuePair<TKey, TValue>> increments);

		/// <summary>
		/// Increments the value at specified key
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="increment">The increment.</param>
		void Increment(TKey key, TValue increment);
	}
}