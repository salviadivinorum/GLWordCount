using System.Collections.Generic;

namespace GLWordCount.Abstraction
{
	/// <summary>
	/// Defines the contract for a line splitter.
	/// </summary>
	public interface ILineSplitter
	{
		/// <summary>
		/// Splits the line.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <returns>A collection of words.</returns>
		IEnumerable<string> SplitLine(string line);
	}
}
