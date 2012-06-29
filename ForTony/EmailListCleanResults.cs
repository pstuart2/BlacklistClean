using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForTony
{
	public class EmailListCleanResults
	{
		private StringBuilder __Log = null;
		public StringBuilder Log
		{
			get
			{
				if (__Log == null) __Log = new StringBuilder();
				return __Log;
			}
		}

		public bool WasSuccess { get; set; }
		public int FilesCleaned { get; set; }
		public int TotalEmailsRemoved { get; set; }
		public int TotalUniqueEmailsRemoved { get; set; }
		public int TotalEmailsKept { get; set; }
		public int TotalUniqueEmailsKept { get; set; }
	}
}
