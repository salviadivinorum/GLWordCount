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
		private MainModel model;
		private ICommand updater;
		private ICommand canceler;
		private string inputFile;
		private bool selectFileBtnEnabled;
		private List<WordOccurance> outputSortedWords;
		private string progressLevel;
		private CancellationTokenSource cancellationTokenSource;

		public MainVM()
		{
			model = new MainModel();
			ProgressLevel = "0%";
			SelectFileBtnEnabled = true;
		}

		
		public ICommand CalculateCommand
		{
			get
			{
				if (updater == null)
				{
					updater = new Relaycommand(param => CanCalculate(param), param => Calculate(param));
				}
				return updater;
			}
			set
			{
				updater = value;
			}
		}


		public ICommand CancelCommand
		{
			get
			{
				if (canceler == null)
				{
					canceler = new Relaycommand(param => CanCancel(param), param => Cancel(param));
				}
				return canceler;
			}
			set
			{
				canceler = value;
			}
		}

		
		public bool SelectFileBtnEnabled
		{
			get
			{
				return selectFileBtnEnabled;
			}
			set
			{
				if (selectFileBtnEnabled != value)
				{
					selectFileBtnEnabled = value;
					OnPropertyChanged();
				}
			}
		}

		
		public List<WordOccurance> OutputSortedWords
		{
			get
			{
				return outputSortedWords;
			}
			set
			{
				if (outputSortedWords != value)
				{
					outputSortedWords = value;
					OnPropertyChanged();
				}
			}
		}

		
		public string ProgressLevel
		{
			get => progressLevel;
			set
			{
				if (progressLevel != value)
				{
					progressLevel = value;
					OnPropertyChanged();
				}
			}
		}

		
		public string InputFile
		{
			get => inputFile;
			set
			{
				if (inputFile != value)
				{
					inputFile = value;
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
			// Create a new CancellationTokenSource
			cancellationTokenSource = new CancellationTokenSource();

			var progress = new Progress<double>();
			progress.ProgressChanged += (sender, value) =>
			{
				// Update the UI control label with the progress value
				ProgressLevel = $"{value}%";
			};

			SelectFileBtnEnabled = false;

			try
			{
				// Run the ProcessTextFile method on a separate thread
				await Task.Run(() =>
				{
					model.ProcessTextFile(cancellationTokenSource.Token, progress);

				});

				InputFile = model.InputFile;
				OutputSortedWords = model.WordOccurances;

			}
			catch (OperationCanceledException)
			{
				// The operation was cancelled					
				ProgressLevel = "Cancelled";
			}
			finally
			{
				// Re-enable the Analyze button
				SelectFileBtnEnabled = true;
				//ProgressLevel = "100%";
			}
		}


		private bool CanCancel(object param)
		{
			return cancellationTokenSource == null ? false : true;
			// return true;
		}

		private void Cancel(object param)
		{
			// Cancel the operation
			cancellationTokenSource.Cancel();

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


			public bool CanExecute(object parameter)
			{
				return _canExecute(parameter);
			}

			public event EventHandler CanExecuteChanged
			{
				add => CommandManager.RequerySuggested += value;
				remove => CommandManager.RequerySuggested -= value;
			}

			public void Execute(object parameter)
			{
				_execute(parameter);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
