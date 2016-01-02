// <copyright company="Skivent Ltda.">
// Copyright (c) 2013, All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using static System.Diagnostics.Contracts.Contract;

namespace Braidwood.Tristany
{
	/// <summary>
	/// RDBMS based inmplementation of <see cref="IIncrementingSortedDictionary{TKey,TValue}"/>.
	/// It uses ADO .NET to access backing store
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class SqlCeIncrementingSortedDictionary<TKey, TValue> : SqlCeSortedDictionary<TKey, TValue>,
		IIncrementingSortedDictionary<TKey, TValue>
	{
		public SqlCeIncrementingSortedDictionary(string dictionaryName, IDbConnection db, DbCommandBuilder commandBuilder)
			: base(dictionaryName, db, new BypassValueFormatter(), commandBuilder)
		{
			Requires(typeof (TValue) == typeof (int)
			                  || typeof (TValue) == typeof (uint)
			                  || typeof (TValue) == typeof (long)
			                  || typeof (TValue) == typeof (ulong)
			                  || typeof (TValue) == typeof (float)
			                  || typeof (TValue) == typeof (double)
			                  || typeof (TValue) == typeof (decimal)
			                  || typeof (TValue) == typeof (byte)
			                  || typeof (TValue) == typeof (short)
			                  || typeof (TValue) == typeof (ushort));
		}

		/// <summary>
		/// Increments the values at specified keys
		/// </summary>
		/// <param name="increments">The increments.</param>
		public void Increment(IEnumerable<KeyValuePair<TKey, TValue>> increments)
		{
			Requires(increments != null);

			foreach (var increment in increments)
			{
				Increment(increment.Key, increment.Value);
			}
		}

		/// <summary>
		/// Increments the values at specified keys
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="increment">The increment</param>
		public void Increment(TKey key, TValue increment)
		{
			var args = new Dictionary<string, object>
			{
				{"key", key},
				{"inc", increment}
			};
			using (var cmd = CreateCommand($"UPDATE {EscapedTableName} SET {EscapedValueColumnName}={EscapedValueColumnName}+@inc WHERE {EscapedKeyColumnName}=@key", args))
			{
				var r = cmd.ExecuteNonQuery();
				Assert(r > 0);
			}
		}

		/// <summary>
		/// Formatter that just bypass the same object
		/// </summary>
		private class BypassValueFormatter : IValueFormatter
		{
			/// <summary>
			/// Serializes the input object to a format able to be stored.
			/// </summary>
			/// <param name="obj">The object.</param>
			/// <returns>
			/// Sotrage compatible object
			/// </returns>
			public object ToStorage(object obj)
			{
				return obj;
			}

			/// <summary>
			/// Converts the storage input object into a live application object
			/// </summary>
			/// <typeparam name="T">Type of object retrieved</typeparam>
			/// <param name="storageObject">The storage object</param>
			/// <returns>
			/// Live object
			/// </returns>
			public T ToObject<T>(object storageObject)
			{
				return (T) storageObject;
			}
		}
	}
}