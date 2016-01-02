// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Data.SqlClient;
using Braidwood.Tristany;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;

namespace Braidwood.Oak.Test
{
	[TestClass]
	public class UnitTest1
	{
		private IDbConnection _db;
		private DbCommandBuilder _commandBuilder;
		private string _tableName;
		private ISortedDictionary<string, string> _uut;

		[TestInitialize]
		public void Setup()
		{
			_tableName = "TD" + Guid.NewGuid().ToString().Replace("-", "");
			SetupSqlCe();
		}

		private void SetupSql()
		{
			_commandBuilder = new SqlCommandBuilder();
			_db = new SqlConnection("Server=(localdb)\\ProjectsV12;Database=TestDictionary;Trusted_Connection=True");
			_db.Open();
			var formatter = new ProtobufValueFormatter();
			_uut = new SqlSortedDictionary<string, string>(_tableName, _db, formatter, _commandBuilder, false);
		}

		private void SetupSqlCe()
		{
			_commandBuilder = new SqlCeCommandBuilder();
			if (File.Exists("TestingData.sdf"))
			{
				File.Delete("TestingData.sdf");
			}
			var engine = new SqlCeEngine("Data Source=TestingData.sdf;Persist Security Info=False;");
			engine.CreateDatabase();
			_db = new SqlCeConnection(engine.LocalConnectionString);
			_db.Open();
			var formatter = new ProtobufValueFormatter();
			_uut = new SqlCeSortedDictionary<string, string>(_tableName, _db, formatter, _commandBuilder);
		}

		[TestCleanup]
		public void Teardown()
		{
			_db.Close();
			if (File.Exists("TestingData.sdf"))
			{
				File.Delete("TestingData.sdf");
			}
		}

		[TestMethod]
		public void TristanySortedDictionary_Add()
		{
			const int n = 1000;
			for (int i = 0; i < n; i++)
			{
				_uut.Add("hola" + i, "mundo" + i);
			}

			Assert.AreEqual(n, _uut.Count);
		}

		[TestMethod]
		public void TristanySortedDictionary_Get()
		{
			_uut.Add("hola", "mundo");
			_uut.Add("jairo", "velasco");
			Assert.AreEqual("mundo", _uut["hola"]);
			Assert.AreEqual("velasco", _uut["jairo"]);
		}

		[TestMethod]
		public void TristanySortedDictionary_Clear()
		{
			_uut.Add("hola", "mundo");
			_uut.Add("jairo", "velasco");
			_uut.Clear();
			Assert.AreEqual(0, _uut.Count);
		}

		[TestMethod]
		public void TristanySortedDictionary_GetRange()
		{
			var lst = new List<string>();
			var rnd = new Random(20);
			for (int i = 0; i < 100; i++)
			{
				var value = rnd.Next().ToString();
				_uut.Add(value, value);
				lst.Add(value);
			}
			lst.Sort();
			var min = lst.Min();
			var max = lst.Max();
			var rt = _uut.GetRange(min, max);
			var e1 = rt.GetEnumerator();
			var e2 = lst.GetEnumerator();
			while (true)
			{
				var m1 = e1.MoveNext();
				var m2 = e2.MoveNext();
				Assert.AreEqual(m1, m2);
				if (!m1 || !m2) break;
				Assert.AreEqual(e1.Current.Key, e2.Current);
			}
		}
	}
}