using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mironov.PolynomView
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App() {
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
		}

		void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			string errorMessage = string.Format("An unhandled exception occurred: {0}\n\nStack trace: {1}", e.Exception.Message, e.Exception.StackTrace);
			MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}
	}
}
