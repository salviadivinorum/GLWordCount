using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GLWordCount.Model;

namespace GLWordCount.ViewModel
{
	/// <summary>
	/// Main view model (intermediary between View and Model)
	/// </summary>
	internal class MainVM : INotifyPropertyChanged
	{
		private readonly MainModel _model;
		private ICommand? _calculator;
		private ICommand? _cancelor;
		private string? _inputFile;
		private bool _selectFileBtnEnabled;
		private List<WordOccurance>? _outputSortedWords;
		private string? _progressLevel;
		private double? _progressValue;
		private CancellationTokenSource? _cancellationTokenSource;

		public MainVM()
		{
			_model = new MainModel();
			ProgressLevel = "0%";
			SelectFileBtnEnabled = true;
		}

		/// <summary>
		/// Gets or sets the calculate command
		/// </summary>
		public ICommand CalculateCommand
		{
			get
			{
				if (_calculator == null)
				{
					_calculator = new Relaycommand(param => CanCalculate(), param => Calculate());
				}
				return _calculator;
			}
			set
			{
				_calculator = value;
			}
		}


		/// <summary>
		/// Gets or sets the cancel command
		/// </summary>
		public ICommand CancelCommand
		{
			get
			{
				if (_cancelor == null)
				{
					_cancelor = new Relaycommand(param => CanCancel(), param => Cancel());
				}
				return _cancelor;
			}
			set
			{
				_cancelor = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the select file button is enabled.
		/// </summary>
		/// <value><c>true</c> if select file button is enabled; otherwise, <c>false</c>.</value>
		public bool SelectFileBtnEnabled
		{
			get
			{
				return _selectFileBtnEnabled;
			}
			set
			{
				if (_selectFileBtnEnabled != value)
				{
					_selectFileBtnEnabled = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the output sorted words.
		/// </summary>
		public List<WordOccurance>? OutputSortedWords
		{
			get
			{
				return _outputSortedWords;
			}
			set
			{
				if (_outputSortedWords != value)
				{
					_outputSortedWords = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the progress level as string
		/// </summary>
		public string? ProgressLevel
		{
			get => _progressLevel;
			set
			{
				if (_progressLevel != value)
				{
					_progressLevel = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the progress level as double
		/// </summary>
		public double? ProgressValue
		{
			get { return _progressValue; }
			set
			{
				_progressValue = value;
				OnPropertyChanged(nameof(ProgressValue));
			}
		}


		/// <summary>
		/// Gets or sets the file path of Input file
		/// </summary>
		public string? InputFile
		{
			get => _inputFile;
			set
			{
				if (_inputFile != value)
				{
					_inputFile = value;
					OnPropertyChanged();
				}
			}
		}

		private bool CanCalculate()
		{
			return true;
		}

		/// <summary>
		/// Calculates the word occurances in a text file and updates the UI with the progress and results.
		/// </summary>
		private async void Calculate()
		{
			// create a new CancellationTokenSource
			_cancellationTokenSource = new CancellationTokenSource();

			var progress = new Progress<double>();
			progress.ProgressChanged += (sender, value) =>
			{
				// Update the UI controls with the progress value
				ProgressLevel = $"{value}%";
				ProgressValue = value;
			};

			SelectFileBtnEnabled = false;
			var inputFileName_current = _model.InputFile;

			try
			{
				// Run the ProcessTextFile method on a separate thread
				await Task.Run(() => _model.ProcessTextFile(_cancellationTokenSource.Token, progress));

				// inform view
				InputFile = _model.InputFile;
				OutputSortedWords = _model.WordOccurances;

			}
			catch (OperationCanceledException)
			{
				// The operation was cancelled
				InputFile = inputFileName_current;
				ProgressLevel = "Cancelled";
			}
			finally
			{
				SelectFileBtnEnabled = true;
				ProgressValue = 0;
			}
		}


		private bool CanCancel()
		{
			return _cancellationTokenSource != null;
		}

		private void Cancel()
		{
			// Cancel the operation
			_cancellationTokenSource?.Cancel();
		}

		/// <summary>
		/// ICommand implementation for execute, can execute
		/// </summary>
		private class Relaycommand : ICommand
		{
			private readonly Predicate<object> _canExecute;
			private readonly Action<object> _execute;

			public Relaycommand(Predicate<object> canExecute, Action<object> execute)
			{
				_canExecute = canExecute;
				_execute = execute;
			}


			public bool CanExecute(object? parameter)
			{
				return _canExecute(parameter!);
			}

			public event EventHandler? CanExecuteChanged
			{
				add => CommandManager.RequerySuggested += value;
				remove => CommandManager.RequerySuggested -= value;
			}

			public void Execute(object? parameter)
			{
				_execute(parameter!);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
