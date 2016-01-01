// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Braidwood.Tristany;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Braidwood.Oak.Test
{
	[TestClass]
	public class UnitTest2
	{
		private IDataStructureRepository factory;
		private EnterpriseAccounting uut;

		[TestInitialize]
		public void Setup()
		{
			factory = new InMemoryDataStructureRepository();
			uut = new EnterpriseAccounting(factory, "testEnterprise");
		}
		
		[TestMethod]
		public void AddAccount_GivenNewAccount()
		{
			var account = new NewAccount();
			account.Code = "111";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;
			
			uut.AddAccount(account);
		}

		[TestMethod]
		[ExpectedException(typeof(EnterpriseAccountingException))]
		public void AddAccount_GivenAlreadyUsedAccountCode_ShouldThrow()
		{
			var account = new NewAccount();
			account.Code = "111";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);
			uut.AddAccount(account);
		}

		[TestMethod]
		public void AddTransaction_GivenTwoEntriesWithEqualSums()
		{
			var account = new NewAccount();
			account.Code = "001";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);

			account = new NewAccount();
			account.Code = "021";
			account.InitialBalance = 10000;
			account.Name = "Proveedores";
			account.Nature = AccountNature.Credit;

			uut.AddAccount(account);

			var transaction = new NewTransaction();
			transaction.Holder = "holder";
			transaction.Entries = new List<AccountTransactionEntry>();
			transaction.Entries.Add(new AccountTransactionEntry() { AccountCode = "001", Value = 200});
			transaction.Entries.Add(new AccountTransactionEntry() { AccountCode = "021", Value = -200 });

			uut.AddTransaction(transaction);
		}

		[TestMethod]
		[ExpectedException(typeof(EnterpriseAccountingException))]
		public void AddTransaction_GivenUnknownAccountCode_ShouldThrow()
		{
			var account = new NewAccount();
			account.Code = "111";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);

			var transaction = new NewTransaction();
			transaction.Holder = "holder";
			transaction.Entries = new List<AccountTransactionEntry>();
			transaction.Entries.Add(new AccountTransactionEntry() { AccountCode = "111", Value = 200 });
			transaction.Entries.Add(new AccountTransactionEntry() { AccountCode = "021", Value = -200 });

			uut.AddTransaction(transaction);
		}

		[TestMethod]
		[ExpectedException(typeof(EnterpriseAccountingException))]
		public void AddTransaction_GivenOneEntry_ShouldThrow()
		{
			var account = new NewAccount();
			account.Code = "111";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);

			var transaction = new NewTransaction();
			transaction.Holder = "holder";
			transaction.Entries = new List<AccountTransactionEntry>();
			transaction.Entries.Add(new AccountTransactionEntry() { AccountCode = "001", Value = 200 });

			uut.AddTransaction(transaction);
		}

		[TestMethod]
		public void ListAccounts()
		{
			var account = new NewAccount();
			account.Code = "111";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);

			var accounts = uut.ListAccounts();
			Assert.AreEqual(accounts.Count(), 1);

			account = new NewAccount();
			account.Code = "112";
			account.InitialBalance = 10000;
			account.Name = "Bancos";
			account.Nature = AccountNature.Debit;

			uut.AddAccount(account);

			accounts = uut.ListAccounts();
			Assert.AreEqual(accounts.Count(), 2);
		}
	}
}