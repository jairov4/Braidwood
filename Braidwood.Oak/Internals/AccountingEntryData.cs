// <copyright company="Skivent Ltda.">
// Copyright (c) 2013, All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;

namespace Braidwood.Oak.Internals
{
	public class AccountingEntryData
	{
		public Guid TransactionId { get; set; }
		public DateTimeOffset Time { get; set; }
		public string AccountCode { get; set; }
		public decimal Value { get; set; }
	}
}