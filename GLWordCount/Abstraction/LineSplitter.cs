using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLWordCount.Abstraction
{
	internal class LineSplitter : ILineSplitter
	{
		private readonly char[] _pattern;

		public LineSplitter(char[] pattern) 
		{
			_pattern = pattern;
		}

		public IEnumerable<string> SplitLine(string line)
		{
			var words = line.Split(_pattern, StringSplitOptions.RemoveEmptyEntries);
			return words;
		}
	}
}
