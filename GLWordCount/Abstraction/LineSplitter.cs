using System;
using System.Collections.Generic;

namespace GLWordCount.Abstraction
{
	/// <summary>
	/// Line splitter implementation
	/// </summary>
	internal class LineSplitter : ILineSplitter
	{
		private readonly char[] _pattern;

		public LineSplitter(char[] pattern)
		{
			_pattern = pattern;
		}

		public IEnumerable<string> SplitLine(string line)
		{
			try
			{
				return line.Split(_pattern, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (ArgumentException e)
			{
				throw new ArgumentException("An error occurred while splitting the line.", e);
			}
		}
	}
}
