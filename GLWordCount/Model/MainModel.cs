using GLWordCount.Abstraction;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace GLWordCount.Model
{
	/// <summary>
	/// Main model (business logic)
	/// </summary>
	public class MainModel
	{
		public MainModel()
		{
			WordOccurances = new List<WordOccurance>();
			InputSplitPattern = new[] { ' ', '\n', '\r' };
		}

		public char[] InputSplitPattern { get; set; }
		public string? InputFile { get; set; }
		public List<WordOccurance> WordOccurances { get; set; }

		/// <summary>
		/// Prompts the user to select a text file.
		/// </summary>
		/// <returns>The file path of the text file.</returns>
		private string GetInputFileName()
		{
			try
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Text Files|*.txt";
				openFileDialog.Title = "Select a Text File";

				if (openFileDialog.ShowDialog() == true)
				{
					return openFileDialog.FileName;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return string.Empty;
		}

		/// <summary>
		/// Processes a text file.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="progress">The progress.</param>
		public void ProcessTextFile(CancellationToken cancellationToken, IProgress<double> progress)
		{
			// open file dialog window
			var inputFile = GetInputFileName();
			if (!string.IsNullOrEmpty(inputFile))
			{
				InputFile = inputFile;

				// reusable (modular) file parser
				// split the logic into small methods SRP single responsibility principle
				ILineSplitter lineSplitter = new LineSplitter(InputSplitPattern);
				IWordProcessor wordProcessor = new WordProcessor(InputFile, progress, lineSplitter);

				// fill in helper dictionary
				Dictionary<string, int> wordCountsDict = ProcessWords(cancellationToken, wordProcessor);

				// sort the result and return it as a structure usable for UI
				WordOccurances = SortWordCounts(wordCountsDict);
			}
		}

		/// <summary>
		/// Processes the words in a text file.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="wordProcessor">The word processor.</param>
		/// <returns>A dictionary of words and their occurances.</returns>
		public Dictionary<string, int> ProcessWords(CancellationToken cancellationToken, IWordProcessor wordProcessor)
		{
			Dictionary<string, int> wordCountsDict = new Dictionary<string, int>();

			// using deffered execution of collection
			foreach (var word in wordProcessor.GetWords())
			{
				// Check if cancellation has been requested
				cancellationToken.ThrowIfCancellationRequested();

				var w = word;
				if (wordCountsDict.ContainsKey(w))
				{
					wordCountsDict[w]++;
				}
				else
				{
					wordCountsDict[w] = 1;
				}
			}

			return wordCountsDict;
		}

		/// <summary>
		/// Sorts a dictionary of words and their occurances.
		/// </summary>
		/// <param name="wordCountsDict">The dictionary of words and their occurances.</param>
		/// <returns>A list of <see cref="WordOccurance"/> objects.</returns>
		public List<WordOccurance> SortWordCounts(Dictionary<string, int> wordCountsDict)
		{
			var sortedWordCounts = wordCountsDict.ToList();
			sortedWordCounts.Sort((x, y) => y.Value.CompareTo(x.Value));
			return sortedWordCounts.Select(x => new WordOccurance(x.Key, x.Value)).ToList();
		}

	}
}
