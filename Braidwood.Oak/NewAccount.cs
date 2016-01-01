// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

namespace Braidwood.Oak
{
	/// <summary>
	/// Represents a new account to be created
	/// </summary>
	public class NewAccount
	{
		/// <summary>
		/// Gets or sets the account code.
		/// </summary>
		/// <value>
		/// The code.
		/// </value>
		public string Code { get; set; }

		/// <summary>
		/// Gets or sets the account name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the account nature.
		/// </summary>
		/// <value>
		/// The nature.
		/// </value>
		public AccountNature Nature { get; set; }

		/// <summary>
		/// Gets or sets the initial balance.
		/// </summary>
		/// <value>
		/// The initial balance.
		/// </value>
		public decimal InitialBalance { get; set; }
	}
}