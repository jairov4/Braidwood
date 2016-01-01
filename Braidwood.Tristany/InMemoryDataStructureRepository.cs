// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Collections.Generic;
using static System.Diagnostics.Contracts.Contract;

namespace Braidwood.Tristany
{
	/// <summary>
	/// In memory data structure repository
	/// </summary>
	public sealed class InMemoryDataStructureRepository : IDataStructureRepository
	{
		private readonly IDictionary<string, object> _repository = new Dictionary<string, object>();

		/// <summary>
		/// Gets the specified data structure by name.
		/// </summary>
		/// <typeparam name="T">Type of data structure</typeparam>
		/// <param name="name">The name of the data set</param>
		/// <param name="dataStructure">The data structure instance</param>
		public void Get<T>(string name, out T dataStructure) where T : class
		{
			Requires(!string.IsNullOrWhiteSpace(name));

			dataStructure = (T) Provide(typeof (T), name);

			Assert(dataStructure != null);
		}

		/// <summary>
		/// Provides the specified type.
		/// </summary>
		/// <param name="type">Type of datastructure.</param>
		/// <param name="name">The name.</param>
		/// <returns>The datastructure required</returns>
		public object Provide(Type type, string name)
		{
			Requires(type != null);
			Requires(!string.IsNullOrWhiteSpace(name));
			object obj = null;
			if (type.GetGenericTypeDefinition() == typeof (ISortedDictionary<,>) 
				|| type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
			{
				var customSortedDictionary = typeof (InMemorySortedDictionary<,>);
				var genArgs = type.GetGenericArguments();
				var tkey = genArgs[0];
				var tvalue = genArgs[1];
				var readyType = customSortedDictionary.MakeGenericType(tkey, tvalue);
				Assert(type.IsAssignableFrom(readyType));
				obj = Activator.CreateInstance(readyType);
			}
			else if (type.GetGenericTypeDefinition() == typeof (IIncrementingSortedDictionary<,>))
			{
				var customSortedDictionary = typeof (InMemoryIncrementingSortedDictionary<,>);
				var genArgs = type.GetGenericArguments();
				var tkey = genArgs[0];
				var tvalue = genArgs[1];
				var readyType = customSortedDictionary.MakeGenericType(tkey, tvalue);
				Assert(type.IsAssignableFrom(readyType));
				obj = Activator.CreateInstance(readyType);
			}
			else
			{
				throw new NotSupportedException("Tipo de estructura de datos no soportada " + type);
			}

			Assert(obj != null);
			return obj;
		}
	}
}