// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.IO;
using ProtoBuf;
using static System.Diagnostics.Contracts.Contract;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Uses protobuf to serialize and store values
	/// </summary>
	public class ProtobufValueFormatter : IValueFormatter
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
			var str = new MemoryStream();
			Serializer.Serialize(str, obj);
			var array = str.ToArray();
			Assert(array != null);
			return array;
		}

		/// <summary>
		/// To the object.
		/// </summary>
		/// <typeparam name="T">Type of data retrieved</typeparam>
		/// <param name="str">The string.</param>
		/// <returns>The live object</returns>
		public T ToObject<T>(object str)
		{
			Requires(str is byte[]);

			var buf = new MemoryStream((byte[])str);
			var obj = Serializer.Deserialize<T>(buf);
			return obj;
		}
	}
}