using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace Braidwood.Tristany
{
	/// <summary>
	/// Implementation of SortedDictionary based on Microsoft Sql Server CE
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class SqlCeSortedDictionary<TKey, TValue> : AdoNetSortedDictionaryBase<TKey, TValue>
	{
		public SqlCeSortedDictionary(string dictionaryName, IDbConnection db, IValueFormatter formatter,
			DbCommandBuilder commandBuilder)
			: base(dictionaryName, db, formatter, commandBuilder)
		{
			EnsureTable();
		}

		private void EnsureTable()
		{
			var args = new Dictionary<string, object>()
			{
				{"tableName", TableName}
			};
			
			var cmdText1 = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
			var cmd1 = CreateCommand(this.Db, cmdText1, args);
			var cnt = (int)cmd1.ExecuteScalar();
			if (cnt > 0)
			{
				return;
			}

			var cmdText = $"CREATE TABLE {EscapedTableName} ({EscapedKeyColumnName} {ResolveType(typeof(TKey))} PRIMARY KEY, {EscapedValueColumnName} IMAGE)";
			var cmd = CreateCommand(Db, cmdText);
			cmd.ExecuteNonQuery();
		}
	}
}