namespace GLWordCount.Model
{
	/// <summary>
	/// Structure to represent a word occurance.
	/// </summary>
	public class WordOccurance
	{
		/// <summary>
		/// Gets or sets the word.
		/// </summary>
		public string Word { get; set; }

		/// <summary>
		/// Gets or sets the word occurance.
		/// </summary>
		public int Occurance { get; set; }


		public WordOccurance(string word, int occurance)
		{
			Word = word;
			Occurance = occurance;
		}
	}
}
