using System.Collections.Generic;

namespace GLWordCount.Abstraction
{
	/// <summary>
	/// Defines the contract for a word processor.
	/// </summary>
	public interface IWordProcessor
	{
		/// <summary>
		/// Gets the words.
		/// </summary>
		/// <returns>A collection of words.</returns>
		IEnumerable<string> GetWords();
	}
}
