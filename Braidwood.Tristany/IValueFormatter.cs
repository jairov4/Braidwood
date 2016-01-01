// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

namespace Braidwood.Tristany
{
	/// <summary>
	/// Formats the values to and from storage engine
	/// </summary>
	public interface IValueFormatter
	{
		/// <summary>
		/// Serializes the input object to a format able to be stored.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>Sotrage compatible object</returns>
		object ToStorage(object obj);

		/// <summary>
		/// Converts the storage input object into a live application object
		/// </summary>
		/// <typeparam name="T">Type of object retrieved</typeparam>
		/// <param name="storageObject">The storage object</param>
		/// <returns>Live object</returns>
		T ToObject<T>(object storageObject);
	}
}