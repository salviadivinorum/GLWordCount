﻿using GLWordCount.Abstraction;
using NSubstitute;
using NUnit.Framework;

namespace WordProcessorTests
{
	[TestFixture]
	public class WordProcessorTests
	{
		private string _testFilePath;
		private IProgress<double> _substituteProgress;
		private ILineSplitter _substituteLineSplitter;

		[SetUp]
		public void SetUp()
		{
			// Create a test file with some sample text
			_testFilePath = Path.GetTempFileName();
			File.WriteAllText(_testFilePath, "This is a test.\nThis is		only a test.");

			// Create substitute objects for the dependencies
			_substituteProgress = Substitute.For<IProgress<double>>();
			_substituteLineSplitter = Substitute.For<ILineSplitter>();

			var pattern = new[] { ' ', '\t', '\n', '\v', '\f', '\r' };

			// Set up the substitute LineSplitter to split lines with white space pattern
			_substituteLineSplitter.SplitLine(Arg.Any<string>())
				.Returns(args => ((string)args[0]).Split(pattern, StringSplitOptions.RemoveEmptyEntries));
		}

		[TearDown]
		public void TearDown()
		{
			// Delete the test file
			File.Delete(_testFilePath);
		}

		[Test]
		public void GetWords_ReturnsWordsInFile()
		{
			// Arrange
			var wordProcessor = new WordProcessor(_testFilePath, _substituteProgress, _substituteLineSplitter);

			// Act
			var words = wordProcessor.GetWords().ToList();

			// Assert
			Assert.That(words.Count, Is.EqualTo(9));
			Assert.That(words, Is.EqualTo(new List<string> { "This", "is", "a", "test.", "This", "is", "only", "a", "test." }));
		}

		[Test]
		public void GetWords_ReportsProgress()
		{
			// Arrange
			var wordProcessor = new WordProcessor(_testFilePath, _substituteProgress, _substituteLineSplitter);

			// Act
			var words = wordProcessor.GetWords().ToList();

			// Assert
			_substituteProgress.Received().Report(Arg.Any<double>());
		}
	}
}