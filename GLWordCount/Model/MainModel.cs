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
    internal class MainModel
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

		/// <summary>
		/// NEW!!!
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <param name="progress"></param>
		/// <exception cref="OperationCanceledException"></exception>
		public void ProcessTextFile(CancellationToken cancellationToken, IProgress<double> progress)
		{
			var wordCountsDict = new Dictionary<string, int>();
			InputFile = GetInputFileName();
			if (!string.IsNullOrEmpty(InputFile))
			{
				ILineSplitter lineSplitter = new LineSplitter(InputSplitPattern);
				IWordProcessor wordProcessor = new WordProcessor(InputFile, progress, lineSplitter);
				foreach (var word in wordProcessor.GetWords())
				{
					// Check if cancellation has been requested
					// cancellationToken.ThrowIfCancellationRequested();
					if (cancellationToken.IsCancellationRequested)
					{
						throw new OperationCanceledException(cancellationToken);
					}

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
			}

			// expected result - sort it
			var sortedWordCounts = wordCountsDict.ToList();
			sortedWordCounts.Sort((x, y) => y.Value.CompareTo(x.Value));
			WordOccurances = sortedWordCounts.Select(x => new WordOccurance(x.Key, x.Value)).ToList();
		}

	}
}
