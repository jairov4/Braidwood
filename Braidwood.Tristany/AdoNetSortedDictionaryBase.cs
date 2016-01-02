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
	public abstract class AdoNetSortedDictionaryBase<TKey, TValue> : ISortedDictionary<TKey, TValue>
	{
		public IDbConnection Db { get; }
		
		public IValueFormatter Formatter { get; }

		public DbCommandBuilder Builder { get; }

		public string EscapedTableName { get; }

		public string TableName { get; }

		public string EscapedKeyColumnName { get; }

		public string EscapedValueColumnName { get; }

		protected AdoNetSortedDictionaryBase(string dictionaryName, IDbConnection db, IValueFormatter formatter,
			DbCommandBuilder commandBuilder)
		{
			Requires(!string.IsNullOrWhiteSpace(dictionaryName));
			Requires(db != null);
			Requires(formatter != null);
			Requires(commandBuilder != null);
			
			Formatter = formatter;
			Builder = commandBuilder;
			Db = db;
			TableName = dictionaryName;
			EscapedTableName = EscapeIdentifier(dictionaryName);
			EscapedKeyColumnName = EscapeIdentifier("Key");
			EscapedValueColumnName = EscapeIdentifier("Value");
		}

		public IEnumerable<TKey> KeysStream
		{
			get
			{
				using (var cmd = CreateCommand($"SELECT {EscapedKeyColumnName} FROM {EscapedTableName} ORDER BY {EscapedKeyColumnName} ASC"))
				using (var dataReader = cmd.ExecuteReader())
				{
					while (dataReader.Read())
					{
						var values = new object[1];
						dataReader.GetValues(values);
						var key = (TKey)values[0];
						yield return key;
					}
				}
			}
		}

		public IEnumerable<TValue> ValuesStream
		{
			get
			{
				using (var cmd = CreateCommand($"SELECT {EscapedValueColumnName} FROM {EscapedTableName} ORDER BY {EscapedKeyColumnName} ASC"))
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
			using (var cmd = CreateCommand($"SELECT {EscapedKeyColumnName}, {EscapedValueColumnName} FROM {EscapedTableName} ORDER BY {EscapedKeyColumnName} ASC"))
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
			var args = new Dictionary<string, object>
			{
				{"key", item.Key},
				{"value", Formatter.ToStorage(item.Value)}
			};
			using (var cmd = CreateCommand($"INSERT INTO {EscapedTableName} ({EscapedKeyColumnName}, {EscapedValueColumnName}) VALUES (@key, @value)", args))
			{
				cmd.ExecuteNonQuery();
			}
			Assert(Count >= 1);
		}

		public void Clear()
		{
			using (var cmd = CreateCommand($"DELETE FROM {EscapedTableName}"))
			{
				cmd.ExecuteNonQuery();
			}
			Assert(Count == 0);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			var args = new Dictionary<string, object>
			{
				{"key", item.Key},
				{"value", Formatter.ToStorage(item.Value)}
			};
			using (var cmd = CreateCommand($"SELECT TOP 1 1 FROM {EscapedTableName} WHERE {EscapedKeyColumnName}=@key AND {EscapedValueColumnName}=@value", args))
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
			var args = new Dictionary<string, object>
			{
				{"key", item.Key},
				{"value", Formatter.ToStorage(item.Value)}
			};
			using (var cmd = CreateCommand($"DELETE FROM {EscapedTableName} WHERE {EscapedKeyColumnName}=@key AND {EscapedValueColumnName}=@value", args))
			{
				return cmd.ExecuteNonQuery() > 0;
			}
		}

		public virtual int Count
		{
			get
			{
				using (var cmd = CreateCommand($"SELECT COUNT({EscapedKeyColumnName}) FROM {EscapedTableName}"))
                {
					var result = cmd.ExecuteScalar();
					var count = Convert.ToInt32(result);
					Assert(count >= 0);
					return count;
				}
			}
		}

		public bool IsReadOnly => false;

		public bool ContainsKey(TKey key)
		{
			var args = new Dictionary<string, object> { {"key", key} };
			using (var cmd = CreateCommand($"SELECT TOP 1 1 FROM {EscapedTableName} WHERE {EscapedKeyColumnName}=@key", args))
			using (var reader = cmd.ExecuteReader())
			{
				return reader.NextResult();
			}
		}

		public void Add(TKey key, TValue value)
		{
			var args = new Dictionary<string, object>
			{
				{"key", key},
				{"value", Formatter.ToStorage(value)}
			};
			var cmd = CreateCommand($"INSERT INTO {EscapedTableName} ({EscapedKeyColumnName}, {EscapedValueColumnName}) VALUES (@key, @value)", args);
			cmd.ExecuteNonQuery();
		}

		public bool Remove(TKey key)
		{
			var args = new Dictionary<string, object>
			{
				{"key", key}
			};
			var cmd = CreateCommand($"DELETE FROM {EscapedTableName} WHERE {EscapedKeyColumnName}=@key", args);
			return cmd.ExecuteNonQuery() > 0;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var args = new Dictionary<string, object>
			{
				{"key", key}
			};
			var cmd = CreateCommand($"SELECT {EscapedValueColumnName} FROM {EscapedTableName} WHERE {EscapedKeyColumnName}=@key ORDER BY {EscapedKeyColumnName} ASC", args);
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
		
		protected virtual string ResolveType(Type keyType)
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

		protected IDbCommand CreateCommand(string query, IDictionary<string, object> parameters = null)
		{
			Requires(!string.IsNullOrWhiteSpace(query));

			var cmd = CreateCommand(Db, query, parameters);

			Assert(cmd != null);
			return cmd;
		}

		protected IDbCommand CreateCommand(IDbConnection db, string query, IDictionary<string, object> parameters = null)
		{
			Requires(db != null);
			Requires(!string.IsNullOrWhiteSpace(query));
			
			var cmd = db.CreateCommand();
			cmd.CommandText = query;
			parameters = parameters ?? new Dictionary<string, object>();
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

		public void DropTable(IDbConnection connection, DbCommandBuilder commandBuilder)
		{
			Requires(commandBuilder != null);
			
			var cmd = CreateCommand(connection, $"DROP TABLE {EscapedTableName}");
			cmd.ExecuteNonQuery();
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetRange(TKey from, TKey to)
		{
			var args = new Dictionary<string, object>
			{
				{"from", @from},
				{"to", to}
			};
			var cmd = CreateCommand($"SELECT {EscapedKeyColumnName}, {EscapedValueColumnName} FROM {EscapedTableName} WHERE {EscapedKeyColumnName} BETWEEN @from AND @to ORDER BY {EscapedKeyColumnName} ASC", args);
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