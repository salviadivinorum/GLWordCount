using GLWordCount.Abstraction;
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
			File.WriteAllText(_testFilePath, "This is a test.\nThis is only a test.");

			// Create substitute objects for the dependencies
			_substituteProgress = Substitute.For<IProgress<double>>();
			_substituteLineSplitter = Substitute.For<ILineSplitter>();

			// Set up the substitute LineSplitter to split lines on spaces
			_substituteLineSplitter.SplitLine(Arg.Any<string>())
				.Returns(args => ((string)args[0]).Split(' '));
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
			Assert.That(words[0], Is.EqualTo("This"));
			Assert.That(words[1], Is.EqualTo("is"));
			Assert.That(words[2], Is.EqualTo("a"));
			Assert.That(words[3], Is.EqualTo("test."));
			Assert.That(words[4], Is.EqualTo("This"));
			Assert.That(words[5], Is.EqualTo("is"));
			Assert.That(words[6], Is.EqualTo("only"));
			Assert.That(words[7], Is.EqualTo("a"));
			Assert.That(words[8], Is.EqualTo("test."));
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