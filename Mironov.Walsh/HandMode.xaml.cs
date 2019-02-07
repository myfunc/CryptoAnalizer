using Mironov.Crypto.Walsh;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// <summary>
	/// Логика взаимодействия для HandMode.xaml
	/// </summary>
	public partial class HandMode : UserControl
	{
		public HandMode() {
			InitializeComponent();
			Init();
		}

		List<WalshMatrix> _matrixList = new List<WalshMatrix>();
		List<string> _combinationList = new List<string>();
		ObservableCollection<CombinationListItem> _combinationOpenedList = new ObservableCollection<CombinationListItem>();
		int _currentMatrixIndex;

		void InitView() {
			CombinationList.ItemsSource = _combinationOpenedList;
		}

		public void Init() {
			InitView();
			InitMatrix();
		}

		void InitMatrix() {
			MatrixMaster master = new MatrixMaster();
			_matrixList = master.MatrixList;
			_combinationList = master.CombinationList;

			_currentMatrixIndex = 0;

			WalshStart.Matrix = _matrixList[_currentMatrixIndex];
			WalshCurrent.Matrix = _matrixList[_currentMatrixIndex];

			Iterate();
		}

		void Iterate() {
			if (_currentMatrixIndex == _matrixList.Count) {
				MessageBox.Show("Все комбинации сгенерированы.");
				return;
			}

			WalshCurrent.Matrix = _matrixList[_currentMatrixIndex];
			_combinationOpenedList.Add(new CombinationListItem() {
				Number = _currentMatrixIndex + 1,
				Combination = _combinationList[_currentMatrixIndex]
			});
			_currentMatrixIndex += 1;

			CombinationListScroll.ScrollToEnd();
		}

		private void IterateButton_Click(object sender, RoutedEventArgs e) {
			Iterate();
		}

		private void RestartButton_Click(object sender, RoutedEventArgs e) {
			_combinationOpenedList.Clear();
			_currentMatrixIndex = 0;
			Iterate();
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e) {
			Iterate();
			CombinationList.SelectedIndex = -1;
		}

		private void CombinationList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var listView = sender as ListView;
			if (listView.SelectedIndex < 0) {
				return;
			}
			WalshCurrent.Matrix = _matrixList[listView.SelectedIndex];
		}
	}
}
