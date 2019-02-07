using Mironov.Crypto.View;
using Mironov.Crypto.Walsh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mironov.Walsh
{
	/* Парень, который будет редактировать эту программу для А.Я.
	 * 1. Не удаляй копирайт с программы. Авторское право всё таки.
	 * 2. В проекте дофига всего из предыдущих заданий. Не распространяй.
	 * Давай пообщаемся (Telegram/Viber) +380634198501
	 * */
	public partial class MainWindow : Window
	{
		public MainWindow() {
			InitializeComponent();
		}

		private void AuthoModeButton_Click(object sender, RoutedEventArgs e) {
			MainGridView.Children.Clear();
			MainGridView.Children.Add(new AutoMode());
			Width = 805;
			Height = 535;
		}

		private void HandModeButton_Click(object sender, RoutedEventArgs e) {
			MainGridView.Children.Clear();
			MainGridView.Children.Add(new HandMode());
			Width = 805;
			Height = 465;
		}

		private void ExitButton_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
