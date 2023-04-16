using GLWordCount.Abstraction;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
		/// Opens a file dialog to select a text file and returns the file name.
		/// </summary>
		/// <returns>The file name of the selected text file, or null if no file was selected or an exception occurred.</returns>
		private string? GetInputFileName()
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
			catch (FileNotFoundException ex)
			{
				MessageBox.Show("File not found: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			// No file was selected or an exception occurred
			return null;
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
			var wordCountsDict = new Dictionary<string, int>();

			// using deffered execution of collection
			foreach (var word in wordProcessor.GetWords())
			{
				// Check if cancellation has been requested
				cancellationToken.ThrowIfCancellationRequested();

				if (wordCountsDict.TryGetValue(word, out int count))
				{
					wordCountsDict[word] = count + 1;
				}
				else
				{
					wordCountsDict[word] = 1;
				}
			}
			return wordCountsDict;
		}

		/// <summary>
		/// Sorts a dictionary of word counts by descending order of count and then alphabetically by word.
		/// </summary>
		/// <param name="wordCountsDict">The dictionary of word counts.</param>
		/// <returns>A list of WordOccurance objects sorted.</returns>
		public List<WordOccurance> SortWordCounts(Dictionary<string, int> wordCountsDict)
		{
			var sortedWordCounts = wordCountsDict.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
			return sortedWordCounts.Select(x => new WordOccurance(x.Key, x.Value)).ToList();
		}
	}
}
