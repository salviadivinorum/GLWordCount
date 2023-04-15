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
			OutputSortedWords = new List<Tuple<string, int>>();
			InputSplitPattern = new[] { ' ', '\n', '\r' };
		}

		public char[] InputSplitPattern { get; set; }
		public string InputFile { get; set; }
		public string[]? InputWords { get; set; }
		public List<Tuple<string, int>> OutputSortedWords { get; set; }
		public List<WordOccurance> WordOccurances { get; set; }

		private string GetInputFile()
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

		private string[] SplitLine(string line, char[] pattern)
		{
			var words = line.Split(pattern, StringSplitOptions.RemoveEmptyEntries);
			return words;
		}

		IEnumerable<string> GetWords(string filePath, char[] pattern, IProgress<double> progress)
		{
			var fileInfo = new FileInfo(filePath);
			long fileLength = fileInfo.Length;

			double smallest = 0;

			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fileStream))
			{
				string? line;
				while ((line = reader.ReadLine()) != null)
				{
					var words = SplitLine(line, pattern);
					foreach (var word in words)
					{
						yield return word;
					}

					double percentComplete = (double)((double)fileStream.Position / fileLength) * 100;
					var rounded = Math.Floor(percentComplete);
					if(rounded > smallest)
					{
						progress.Report(rounded);
						smallest = rounded;
					}
				}
			}
		}


		public void ProcessTextFile(CancellationToken cancellationToken, IProgress<double> progress)
		{
			var wordCountsDict = new Dictionary<string, int>();
			InputFile = GetInputFile();
			if (!string.IsNullOrEmpty(InputFile))
			{
				foreach (var word in GetWords(InputFile, InputSplitPattern, progress))
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

			// expected result
			var sortedWordCounts = wordCountsDict.ToList();
			sortedWordCounts.Sort((x, y) => y.Value.CompareTo(x.Value));
			WordOccurances = sortedWordCounts.Select(x => new WordOccurance(x.Key, x.Value)).ToList();
		}
	}
}
