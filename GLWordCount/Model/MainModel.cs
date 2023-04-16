using GLWordCount.Abstraction;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace GLWordCount.Model
{
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

		public void ProcessTextFile(CancellationToken cancellationToken, IProgress<double> progress)
		{
			WordOccurances = GetWordsOccurences(cancellationToken, progress);
		}

		private List<WordOccurance> GetWordsOccurences(CancellationToken cancellationToken, IProgress<double> progress)
		{
			List<WordOccurance> wordOccurances = new List<WordOccurance>();

			// open file dialog window
			InputFile = GetInputFileName();
			if (!string.IsNullOrEmpty(InputFile))
			{
				// reusable (modular) file parser
				ILineSplitter lineSplitter = new LineSplitter(InputSplitPattern);
				IWordProcessor wordProcessor = new WordProcessor(InputFile, progress, lineSplitter);

				// fill in helper dictionary
				Dictionary<string, int> wordCountsDict = ProcessWords(cancellationToken, wordProcessor);

				// sort the result and return it as a structure usable for UI
				wordOccurances = SortWordCounts(wordCountsDict);
			}
			return wordOccurances;
		}

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

		public List<WordOccurance> SortWordCounts(Dictionary<string, int> wordCountsDict)
		{
			var sortedWordCounts = wordCountsDict.ToList();
			sortedWordCounts.Sort((x, y) => y.Value.CompareTo(x.Value));
			return sortedWordCounts.Select(x => new WordOccurance(x.Key, x.Value)).ToList();
		}

	}
}
