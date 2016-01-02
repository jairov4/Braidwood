// <copyright company="Skivent Ltda.">
// Copyright (c) 2013, All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Implementation of SortedDictionary based on Microsoft Sql Server
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class SqlSortedDictionary<TKey, TValue> : AdoNetSortedDictionaryBase<TKey, TValue>
	{
		public SqlSortedDictionary(string dictionaryName, IDbConnection db, IValueFormatter formatter,
			DbCommandBuilder commandBuilder, bool inMemory)
			: base(dictionaryName, db, formatter, commandBuilder)
		{
			InMemoryTable = inMemory;
			EnsureTable();
		}

		public bool InMemoryTable { get; }

		public override int Count
		{
			get
			{
				using (var cmd = CreateCommand($"SELECT SUM(row_count) FROM sys.dm_db_partition_stats WHERE object_id = OBJECT_ID('{EscapedTableName}') AND index_id < 2 GROUP BY OBJECT_NAME(object_id)")
					)
				{
					var result = cmd.ExecuteScalar();
					var count = (int) (long) result;
					Contract.Assert(count >= 0);
					return count;
				}
			}
		}

		private void EnsureTable()
		{
			var args = new Dictionary<string, object>()
			{
				{"tableName", TableName}
			};

			var cmdText1 = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
			var cmd1 = CreateCommand(this.Db, cmdText1, args);
			var cnt = (int) cmd1.ExecuteScalar();
			if (cnt > 0)
			{
				return;
			}

			var add = InMemoryTable ? "WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA)" : string.Empty;
			var cmdText =
				$"CREATE TABLE {EscapedTableName} ({EscapedKeyColumnName} {ResolveType(typeof (TKey))} PRIMARY KEY, {EscapedValueColumnName} VARBINARY(MAX)) {add}";
			var cmd = CreateCommand(Db, cmdText);
			cmd.ExecuteNonQuery();
		}
	}
}