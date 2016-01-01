// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Diagnostics.Contracts;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Repository of named data structures
	/// </summary>
	public interface IDataStructureRepository
	{
		/// <summary>
		/// Gets the specified data structure by name.
		/// </summary>
		/// <typeparam name="T">Type of data structure</typeparam>
		/// <param name="name">The name of the data set</param>
		/// <param name="dataStructure">The data structure instance</param>
		void Get<T>(string name, out T dataStructure) where T : class;
	}
}