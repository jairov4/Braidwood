// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Collections.Generic;

namespace Braidwood.Oak
{
	/// <summary>
	/// Transaction to be stored
	/// </summary>
	public class TransactionData
	{
		public DateTimeOffset Time { get; set; }
		public string Holder { get; set; }
		public decimal Total { get; set; }
		public ICollection<TransactionEntryData> Entries { get; set; }
	}
}