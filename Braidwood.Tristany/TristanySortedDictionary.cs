// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using static System.Diagnostics.Contracts.Contract;

namespace Braidwood.Tristany
{
	/// <summary>
	/// RDBMS based implementation of <see cref="ISortedDictionary{TKey,TValue}"/>.
	/// It uses ADO .NET to access backing store
	/// </summary>
	/// <typeparam name="TKey">Type of the key</typeparam>
	/// <typeparam name="TValue">Type of the value</typeparam>
	public class TristanySortedDictionary<TKey, TValue> : ISortedDictionary<TKey, TValue>
	{
		public IDbConnection Db { get; }
		public string DictionaryName { get; }
		public IValueFormatter Formatter { get; }
		public DbCommandBuilder Builder { get; }

		public TristanySortedDictionary(string dictionaryName, IDbConnection db, IValueFormatter formatter,
			DbCommandBuilder commandBuilder)
		{
			Requires(!string.IsNullOrWhiteSpace(dictionaryName));
			Requires(db != null);
			Requires(formatter != null);
			Requires(commandBuilder != null);

			Formatter = formatter;
			Builder = commandBuilder;
			Db = db;
			DictionaryName = EscapeIdentifier(dictionaryName);
		}

		public IEnumerable<TKey> KeysStream
		{
			get
			{
				var cmd = CreateCommand("SELECT [Key] FROM @tableName ORDER BY [Key] ASC");
				using (var dataReader = cmd.ExecuteReader())
				{
					while (dataReader.Read())
					{
						var values = new object[1];
						dataReader.GetValues(values);
						var key = (TKey) values[0];
						yield return key;
					}
				}
			}
		}

		public IEnumerable<TValue> ValuesStream
		{
			get
			{
				var cmd = CreateCommand("SELECT [Value] FROM @tableName ORDER BY [Key] ASC");
				using (var dataReader = cmd.ExecuteReader())
				{
					while (dataReader.Read())
					{
						var values = new object[1];
						dataReader.GetValues(values);
						var value = Formatter.ToObject<TValue>((byte[]) values[0]);
						yield return value;
					}
				}
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			var cmd = CreateCommand("SELECT [Key], [Value] FROM @tableName ORDER BY [Key] ASC");
			using (var dataReader = cmd.ExecuteReader())
			{
				while (dataReader.Read())
				{
					var values = new object[2];
					dataReader.GetValues(values);
					var value = Formatter.ToObject<TValue>((byte[]) values[1]);
					var pair = new KeyValuePair<TKey, TValue>((TKey) values[0], value);
					yield return pair;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			var enumerator = GetEnumerator();
			Assert(enumerator != null);
			return enumerator;
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			var cmd = CreateCommand("INSERT INTO @tableName ([Key], [Value]) VALUES (@key, @value)",
				new Dictionary<string, object>
				{
					{"key", item.Key},
					{"value", Formatter.ToStorage(item.Value)}
				});
			cmd.ExecuteNonQuery();
			Assert(Count >= 1);
		}

		public void Clear()
		{
			var cmd = CreateCommand("TRUNCATE TABLE @tableName");
			cmd.ExecuteNonQuery();

			Assert(Count == 0);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			var cmd = CreateCommand("SELECT TOP 1 1 FROM @tableName WHERE [Key]=@key AND [Value]=@value",
				new Dictionary<string, object>
				{
					{"key", item.Key},
					{"value", Formatter.ToStorage(item.Value)}
				});
			using (var reader = cmd.ExecuteReader())
			{
				return reader.Read();
			}
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			Requires(array != null);
			Requires(arrayIndex >= 0);

			foreach (var pair in this)
			{
				array[arrayIndex++] = pair;
			}
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			var cmd = CreateCommand("DELETE FROM @tableName WHERE [Key]=@key AND [Value]=@value",
				new Dictionary<string, object>
				{
					{"key", item.Key},
					{"value", Formatter.ToStorage(item.Value)}
				});
			return cmd.ExecuteNonQuery() > 0;
		}

		public int Count
		{
			get
			{
				var cmd = CreateCommand(@"SELECT SUM(row_count) FROM sys.dm_db_partition_stats WHERE object_id = OBJECT_ID('@tableName') AND index_id < 2 GROUP BY OBJECT_NAME(object_id)");
				var result = cmd.ExecuteScalar();
				var count = (int) (long) result;
				Assert(count >= 0);
				return count;
			}
		}

		public bool IsReadOnly => false;

		public bool ContainsKey(TKey key)
		{
			var cmd = CreateCommand("SELECT TOP 1 1 FROM @tableName WHERE [Key]=@key",
				new Dictionary<string, object>
				{
					{"key", key}
				});
			using (var reader = cmd.ExecuteReader())
			{
				return reader.NextResult();
			}
		}

		public void Add(TKey key, TValue value)
		{
			var cmd = CreateCommand("INSERT INTO @tableName ([Key], [Value]) VALUES (@key, @value)",
				new Dictionary<string, object>
				{
					{"key", key},
					{"value", Formatter.ToStorage(value)}
				});
			cmd.ExecuteNonQuery();
		}

		public bool Remove(TKey key)
		{
			var cmd = CreateCommand("DELETE FROM @tableName WHERE [Key]=@key",
				new Dictionary<string, object>
				{
					{"key", key}
				});
			return cmd.ExecuteNonQuery() > 0;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var cmd = CreateCommand("SELECT [Value] FROM @tableName WHERE [Key]=@key ORDER BY [Key] ASC",
				new Dictionary<string, object>
				{
					{"key", key}
				});
			using (var dataReader = cmd.ExecuteReader())
			{
				while (dataReader.Read())
				{
					var values = new object[1];
					dataReader.GetValues(values);
					value = Formatter.ToObject<TValue>((byte[]) values[0]);
					return true;
				}
			}
			value = default(TValue);
			return false;
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue val;
				if (!TryGetValue(key, out val)) throw new KeyNotFoundException();
				return val;
			}
			set { Add(key, value); }
		}

		public ICollection<TKey> Keys => KeysStream.ToList();

		public ICollection<TValue> Values => ValuesStream.ToList();

		protected string EscapeIdentifier(string dictionaryName)
		{
			Requires(!string.IsNullOrWhiteSpace(dictionaryName));

			dictionaryName = Builder.QuoteIdentifier(dictionaryName);

			Assert(dictionaryName != null);
			return dictionaryName;
		}

		public static void CreateTable(IDbConnection connection, string dictionaryName, DbCommandBuilder commandBuilder,
			bool inMemory = false)
		{
			Requires(connection != null);
			Requires(!string.IsNullOrWhiteSpace(dictionaryName));
			Requires(commandBuilder != null);

			dictionaryName = commandBuilder.QuoteIdentifier(dictionaryName);
			var cmdText = "CREATE TABLE @tableName ([Key] " + ResolveType(typeof (TKey)) +
			              " PRIMARY KEY, [Value] VARBINARY(MAX))";
			if (inMemory)
			{
				cmdText += " WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA)";
			}
			var cmd = CreateCommand(connection, dictionaryName, cmdText);
			cmd.ExecuteNonQuery();
		}

		private static string ResolveType(Type keyType)
		{
			Requires(keyType != null);

			if (keyType == typeof (string)) return "NVARCHAR(512)";
			if (keyType == typeof (int)) return "INT";
			if (keyType == typeof (long)) return "INT";
			if (keyType == typeof (double)) return "FLOAT(53)";
			if (keyType == typeof (float)) return "FLOAT";
			if (keyType == typeof (Guid)) return "GUID";
			throw new NotSupportedException("Tipo de llave no soportado: " + keyType.FullName);
		}

		protected IDbCommand CreateCommand(string query, Dictionary<string, object> parameters = null)
		{
			Requires(!string.IsNullOrWhiteSpace(query));

			var cmd = CreateCommand(Db, DictionaryName, query, parameters);
			Assert(cmd != null);
			return cmd;
		}

		private static IDbCommand CreateCommand(IDbConnection db, string dictionaryName, string query, 
			Dictionary<string, object> parameters = null)
		{
			Requires(db != null);
			Requires(!string.IsNullOrWhiteSpace(query));
			
			var cmd = db.CreateCommand();
			cmd.CommandText = query.Replace("@tableName", dictionaryName);
			parameters = parameters ?? new Dictionary<string, object>();
			parameters.Add("tableName", dictionaryName);
			foreach (var parameter in parameters)
			{
				var p = cmd.CreateParameter();
				p.ParameterName = "@" + parameter.Key;
				p.Value = parameter.Value;
				cmd.Parameters.Add(p);
			}

			Assert(cmd != null);
			return cmd;
		}

		public static void DropTable(IDbConnection connection, string dictionaryName, DbCommandBuilder commandBuilder)
		{
			Requires(commandBuilder != null);
			Requires(!string.IsNullOrWhiteSpace(dictionaryName));

			dictionaryName = commandBuilder.QuoteIdentifier(dictionaryName);
			var cmd = CreateCommand(connection, dictionaryName, @"DROP TABLE @tableName");
			cmd.ExecuteNonQuery();
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetRange(TKey from, TKey to)
		{
			var cmd = CreateCommand("SELECT [Key], [Value] FROM @tableName WHERE [Key] BETWEEN @from AND @to ORDER BY [Key] ASC",
				new Dictionary<string, object>
				{
					{"from", from},
					{"to", to}
				});

			using (var dataReader = cmd.ExecuteReader())
			{
				while (dataReader.Read())
				{
					var values = new object[2];
					dataReader.GetValues(values);
					var value = Formatter.ToObject<TValue>((byte[]) values[1]);
					var pair = new KeyValuePair<TKey, TValue>((TKey) values[0], value);
					yield return pair;
				}
			}
		}
	}
}