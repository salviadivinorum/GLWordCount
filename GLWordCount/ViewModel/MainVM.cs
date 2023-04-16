using GLWordCount.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace GLWordCount.ViewModel
{
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

		
		public ICommand CalculateCommand
		{
			get
			{
				if (_calculator == null)
				{
					_calculator = new Relaycommand(param => CanCalculate(param), param => Calculate(param));
				}
				return _calculator;
			}
			set
			{
				_calculator = value;
			}
		}


		public ICommand CancelCommand
		{
			get
			{
				if (_cancelor == null)
				{
					_cancelor = new Relaycommand(param => CanCancel(param), param => Cancel(param));
				}
				return _cancelor;
			}
			set
			{
				_cancelor = value;
			}
		}

		
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

		
		public double? ProgressValue
		{
			get { return _progressValue; }
			set
			{
				_progressValue = value;
				OnPropertyChanged(nameof(ProgressValue));
			}
		}


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

		private bool CanCalculate(object param)
		{
			// some logic
			return true;
		}


		private async void Calculate(object param)
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

			try
			{
				// Run the ProcessTextFile method on a separate thread
				await Task.Run(() =>
				{
					_model.ProcessTextFile(_cancellationTokenSource.Token, progress);
				});

				InputFile = _model.InputFile;
				OutputSortedWords = _model.WordOccurances;

			}
			catch (OperationCanceledException)
			{
				// The operation was cancelled
				ProgressLevel = "Cancelled";
			}
			finally
			{
				SelectFileBtnEnabled = true;
				ProgressValue = 0;
			}
		}


		private bool CanCancel(object param)
		{
			return _cancellationTokenSource == null ? false : true;
			// return true;
		}

		private void Cancel(object param)
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
