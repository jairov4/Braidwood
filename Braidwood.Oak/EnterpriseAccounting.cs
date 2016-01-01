// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using Braidwood.Tristany;
using System;
using System.Collections.Generic;
using System.Linq;
using Braidwood.Oak.Internals;
using static System.Diagnostics.Contracts.Contract;

namespace Braidwood.Oak
{
	/// <summary>
	/// Business logic for Enterprise Accounting
	/// </summary>
	public class EnterpriseAccounting
	{
		private readonly ISortedDictionary<string, AccountData> _accountDataByCode;
		private readonly ISortedDictionary<Guid, AccountingPeriodBalanceData> _balancesById;
		private readonly IIncrementingSortedDictionary<string, decimal> _accountBalanceByCode;
		private readonly ISortedDictionary<Guid, TransactionData> _transactionsById;
		private readonly IDataStructureRepository _repository;
		private readonly string _enterpriseId;

		protected static void Requires(bool condition, string message)
		{
			if(condition) return;
			throw new EnterpriseAccountingException(message);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnterpriseAccounting"/> class.
		/// </summary>
		/// <param name="repository">The repository of data structures.</param>
		/// <param name="enterpriseId">The enterprise identifier.</param>
		public EnterpriseAccounting(IDataStructureRepository repository, string enterpriseId)
		{
			Requires(repository != null, "Repositorio de estructuras de datos invalido");
			Requires(!string.IsNullOrWhiteSpace(enterpriseId), "Identificador de empresa invalido");
			
			repository.Get(enterpriseId + "_AccountDataByCode", out _accountDataByCode);
			repository.Get(enterpriseId + "_AccountBalanceByCode", out _accountBalanceByCode);
			repository.Get(enterpriseId + "_Balances", out _balancesById);
			repository.Get(enterpriseId + "_TransactionsById", out _transactionsById);
			_repository = repository;
			_enterpriseId = enterpriseId;
		}

		public void AddAccount(NewAccount newAccount)
		{
			Requires(newAccount != null, "Nueva cuenta invalida");
			Requires(!string.IsNullOrWhiteSpace(newAccount.Code), "Codigo de nueva cuenta invalido");
			Requires(!_accountDataByCode.ContainsKey(newAccount.Code), "Codigo de cuenta ya esta en uso");
			Requires(!_accountBalanceByCode.ContainsKey(newAccount.Code), "Codigo de cuenta ya esta en uso");
			Ensures(_accountDataByCode.ContainsKey(newAccount.Code));
			Ensures(_accountBalanceByCode.ContainsKey(newAccount.Code));

			var accountData = new AccountData();
			accountData.Code = newAccount.Code;
			accountData.Name = newAccount.Name;
			accountData.Nature = newAccount.Nature;
			_accountDataByCode.Add(accountData.Code, accountData);
			_accountBalanceByCode.Add(accountData.Code, newAccount.InitialBalance);
		}

		public IEnumerable<AccountDescriptor> ListAccounts()
		{
			var l = new List<AccountDescriptor>(_accountDataByCode.Count);
			foreach (var item in _accountDataByCode.Values)
			{
				var desc = new AccountDescriptor();
				desc.Code = item.Code;
				desc.Name = item.Name;
				l.Add(desc);
			}

			Assert(l != null);
			return l;
		}

		public IEnumerable<AccountingPeriodBalance> GetBalances(DateTime? since, DateTime? to)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds a new accounting transaction.
		/// </summary>
		/// <param name="transaction">The accounting transaction.</param>
		/// <returns>Identificador de transaccion</returns>
		public Guid AddTransaction(NewTransaction transaction)
		{
			Requires(transaction != null, "Nueva transaccion invalida");
			Requires(transaction.Entries != null, "Asientos contables en la transaccion invalidos");
			Requires(transaction.Entries.Count > 0, "Nueva transaccion sin asientos contables");
			Requires(transaction.Entries.Sum(x => x.Value) == 0, "Nueva transaccion cuyas sumas no son iguales");
			Requires(transaction.Entries.All(x => _accountDataByCode.ContainsKey(x.AccountCode)), "Codigo de cuenta no existe");
			
			var now = DateTimeOffset.Now;
			var transactionId = Guid.NewGuid();

			var transactionData = new TransactionData();
			transactionData.Holder = transaction.Holder;
			transactionData.Entries = new List<TransactionEntryData>();
			transactionData.Time = now;

			var balanceIncrements = new Dictionary<string, decimal>();

			foreach (var item in transaction.Entries)
			{
				var entryId = Guid.NewGuid();

				var transactionEntryData = new TransactionEntryData();
				transactionEntryData.AccountCode = item.AccountCode;
				transactionEntryData.Value = item.Value;
				transactionEntryData.EntryId = entryId;
				if (transactionEntryData.Value > 0) transactionData.Total += transactionEntryData.Value;
				transactionData.Entries.Add(transactionEntryData);

				ISortedDictionary<Guid, AccountingEntryData> recordDictionary;
				_repository.Get(GetAccountRecordsLookupTableName(item.AccountCode), out recordDictionary);

				var accountingEntryData = new AccountingEntryData();
				accountingEntryData.Time = now;
				accountingEntryData.AccountCode = item.AccountCode;
				accountingEntryData.Value = item.Value;
				accountingEntryData.TransactionId = transactionId;
				recordDictionary.Add(entryId, accountingEntryData);

				decimal increment;
				if (balanceIncrements.TryGetValue(item.AccountCode, out increment))
				{
					balanceIncrements[item.AccountCode] = increment + transactionEntryData.Value;
				}
				else
				{
					balanceIncrements.Add(item.AccountCode, transactionEntryData.Value);
				}
			}

			_accountBalanceByCode.Increment(balanceIncrements);
			_transactionsById.Add(transactionId, transactionData);

			return transactionId;
		}

		/// <summary>
		/// Gets the name of the newAccount records lookup table.
		/// </summary>
		/// <param name="accountCode">The newAccount code.</param>
		/// <returns>The name</returns>
		private string GetAccountRecordsLookupTableName(string accountCode)
		{
			Requires(!string.IsNullOrEmpty(accountCode), "Codigo de cuenta no valido");
			return _enterpriseId + "_AccountRecords_" + accountCode;
		}

		public ICollection<NewTransaction> ListTransactions(IEnumerable<string> accounts, DateTime? from, DateTime? to,
			string thirdParty)
		{
			return null;
		}

		public void ClosePeriod()
		{
		}
	}
}
