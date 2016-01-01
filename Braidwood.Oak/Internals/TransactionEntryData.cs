// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;

namespace Braidwood.Oak
{
	/// <summary>
	/// Represents an accounting transaction entry to be stored
	/// </summary>
	public class TransactionEntryData
	{
		/// <summary>
		/// Gets or sets the account code.
		/// </summary>
		/// <value>
		/// The account code.
		/// </value>
		public string AccountCode { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public decimal Value { get; set; }

		/// <summary>
		/// Gets or sets the entry identifier in the general account log.
		/// </summary>
		/// <value>
		/// The entry identifier.
		/// </value>
		public Guid EntryId { get; set; }
	}
}