// <copyright company="Skivent Ltda.">
// Copyright (c) 2015 All Right Reserved, http://www.skivent.com.co/
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Braidwood.Oak
{
	[Serializable]
	public class EnterpriseAccountingException : Exception
	{
		public EnterpriseAccountingException()
		{
		}

		public EnterpriseAccountingException(string message) : base(message)
		{
		}

		public EnterpriseAccountingException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnterpriseAccountingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}