// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;

namespace Braidwood.Oak
{
	/// <summary>
	/// Represents an accounting transaction to be created
	/// </summary>
	public class NewTransaction
	{
		/// <summary>
		/// Gets or sets the holder.
		/// </summary>
		/// <value>
		/// The holder.
		/// </value>
		public string Holder { get; set; }

		/// <summary>
		/// Gets or sets the transaction entries.
		/// </summary>
		/// <value>
		/// The entries.
		/// </value>
		public ICollection<AccountTransactionEntry> Entries { get; set; }
	}
}