using System;
using System.Collections.Generic;
using System.IO;

namespace GLWordCount.Abstraction
{
	public class WordProcessor : IWordProcessor
	{
		private readonly string _filePath;
		private readonly IProgress<double> _progress;
		private readonly ILineSplitter _lineSplitter;

		// used Dependency Injection pattern (constructor injection)
		// dependencies 'progress' and 'lineSplitter' are injected into the construtor,
		// rather than being created within the class itself
		public WordProcessor(string filePath, IProgress<double> progress, ILineSplitter lineSplitter)
		{
			_filePath = filePath;
			_progress = progress;
			_lineSplitter = lineSplitter;
		}

		public IEnumerable<string> GetWords()
		{
			var fileInfo = new FileInfo(_filePath);
			long fileLength = fileInfo.Length;

			double smallest = 0;

			using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fileStream))
			{
				string? line;
				while ((line = reader.ReadLine()) != null)
				{
					if (string.IsNullOrEmpty(line))
					{
						// do not process empty lines
						continue;
					}

					var words = _lineSplitter.SplitLine(line);
					foreach (var word in words)
					{
						// perform the lazy (deffered) evaluation in time of need
						yield return word;
					}

					double percentComplete = (double)((double)fileStream.Position / fileLength) * 100;
					var rounded = Math.Floor(percentComplete);
					if (rounded > smallest)
					{
						// report only 1% increments to UI
						_progress?.Report(rounded);
						smallest = rounded;
					}
				}
			}
		}
	}
}
