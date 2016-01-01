// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Data.SqlClient;
using Braidwood.Tristany;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace Braidwood.Oak.Test
{
	[TestClass]
	public class UnitTest1
	{
		private SqlConnection _db;
		private SqlCommandBuilder _commandBuilder;
		private string _tableName;
		private TristanySortedDictionary<string, string> _uut;

		[TestInitialize]
		public void Setup()
		{
			_commandBuilder = new SqlCommandBuilder();
			_tableName = "TD" + Guid.NewGuid().ToString().Replace("-", "");
			_db = new SqlConnection("Server=(localdb)\\ProjectsV12;Database=TestDictionary;Trusted_Connection=True");
			_db.Open();
			var formatter = new ProtobufValueFormatter();
			TristanySortedDictionary<string, string>.CreateTable(_db, _tableName, _commandBuilder, false);
			_uut = new TristanySortedDictionary<string, string>(_tableName, _db, formatter, _commandBuilder);
		}

		[TestCleanup]
		public void Teardown()
		{
			TristanySortedDictionary<string, string>.DropTable(_db, _tableName, _commandBuilder);
			_db.Close();
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