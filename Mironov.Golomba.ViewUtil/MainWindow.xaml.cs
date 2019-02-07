using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
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

namespace Mironov.Golomba.ViewUtil
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow() {
			InitializeComponent();
			Init();
		}
		public Polynomial WordPolynom
		{
			get {
				return new CustomPolynom(WordText.Text);
			}
		}

		public Polynomial FormatPolynom
		{
			get {
				return new CustomPolynom(FormatText.Text);
			}
		}

		protected void Init() {
	
		}

		private void StartButton_OnClick(object sender, RoutedEventArgs e) {
			GenerateMatrix();
		}

		private void GenerateMatrix() {
			var mps = new CheckMatrixV2Polynom(PolyUtils.Concat(new CustomPolynom(0, 2),WordPolynom), FormatPolynom) { Number = 1 };
			PolynomList.GenerateMatrix(mps, 8);
		}
	}
}
