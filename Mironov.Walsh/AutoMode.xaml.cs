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
	/// Логика взаимодействия для AutoMode.xaml
	/// </summary>
	public partial class AutoMode : UserControl
	{
		public AutoMode() {
			InitializeComponent();
			Init();
		}

		List<WalshMatrix> _matrixList;
		List<string> _combinationList = new List<string>();
		ObservableCollection<CombinationListItem> _combinationOpenedList = new ObservableCollection<CombinationListItem>();
		int _currentMatrixIndex;

		public void Init() {
			//WalshPreview.IsReadOnly = true;
			CombinationList.ItemsSource = _combinationOpenedList;
			InitMatrixList();
		}

		void InitMatrixList() {
			GenerateAllButton_Click(null, new RoutedEventArgs());
		}

		public void SetMatrixCountLabel(int count) {
			MatrixCountLabel.Content = string.Format("Кол-во матриц: {0}", count);
		}

		public void SetCurrentMatrix(int index) {
			_currentMatrixIndex = index;
			CurrentMatrixEdit.Text = (index + 1).ToString();
		}

		void Next() {
			_currentMatrixIndex++;
			if (_currentMatrixIndex == _matrixList.Count) {
				_currentMatrixIndex = 0;
			}
			SetCurrentMatrix(_currentMatrixIndex);
		}

		void Prev() {
			_currentMatrixIndex--;
			if (_currentMatrixIndex < 0) {
				_currentMatrixIndex = _matrixList.Count - 1;
			}
			SetCurrentMatrix(_currentMatrixIndex);
		}

		private void NextMatrixButton_Click(object sender, RoutedEventArgs e) {
			Next();
		}

		private void PrevMatrixButton_Click(object sender, RoutedEventArgs e) {
			Prev();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Right) {
				Next();
			}
			if (e.Key == Key.Left) {
				Prev();
			}
		}

		private void CurrentMatrixEdit_TextChanged(object sender, TextChangedEventArgs e) {
			var textBox = sender as TextBox;
			try {
				int index = Int32.Parse(textBox.Text);
				WalshPreview.Matrix = _matrixList[index - 1];

				int[] columns = _combinationList[index - 1].Select(p => Int32.Parse(p.ToString())).ToArray();
				WalshPreview.ColumnNums = columns;
				WalshPreview.RowNums = columns;
				_currentMatrixIndex = index - 1;
				textBox.Background = Brushes.White;
			} catch (Exception ex) {
				textBox.Background = Brushes.Red;
			}
		}

		private void SearchButton_Click(object sender, RoutedEventArgs e) {
			WalshMatrix searchMatrix = WalshSearch.Matrix;
			var result = _matrixList.Where(p => {
				for (int i = 0; i < p.Width; i++) {
					for (int j = 0; j < p.Height; j++) {
						if (searchMatrix[i, j] != p[i, j]) {
							return false;
						}
					}
				}
				return true;
			}).ToList();
			if (result.Count > 0) {
				int findIndex = _matrixList.FindIndex(p => p == result[0]);
				SearchResultLabel.Content = "Найдена матрица №" + (findIndex + 1);
				SetCurrentMatrix(findIndex);
			} else {
				SearchResultLabel.Content = "Матрица не найдена";
			}
		}

		private void GenerateSimButton_Click(object sender, RoutedEventArgs e) {
			MatrixName.Content = "Симметричные матрицы Уолша-Адамара (8 х 8):";
			MatrixMaster master = new MatrixMaster();
			master.InitMatrixList();
			master.InitCombinations();
			_combinationList = new List<string>();
			_matrixList = master.MatrixList.Where((p, ind) => {
				for (int i = 0; i < p.Height; i++) {
					for (int j = 0; j < p.Width; j++) {
						if (p[i, j] != p[j, i]) {
							return false;
						}
					}
				}
				_combinationList.Add(master.CombinationList[ind]);
				return true;
			}).ToList();
			ResetPreviewMatrix();
			ResetCombinationList();
		}

		private void GenerateAllButton_Click(object sender, RoutedEventArgs e) {
			MatrixName.Content = "Все варианты матрицы Уолша-Адамара (8 х 8):";
			MatrixMaster master = new MatrixMaster();
			master.InitMatrixList();
			master.InitCombinations();
			_matrixList = master.MatrixList;
			_combinationList = master.CombinationList;
			ResetPreviewMatrix();
			ResetCombinationList();
		}

		private void ResetPreviewMatrix() {
			_currentMatrixIndex = 0;
			WalshPreview.Matrix = _matrixList[_currentMatrixIndex];
			SetMatrixCountLabel(_matrixList.Count);
			SetCurrentMatrix(_currentMatrixIndex);
		}

		private void ResetCombinationList() {
			_combinationOpenedList.Clear();
			int counter = 1;
			_combinationList.ForEach(p => {
				_combinationOpenedList.Add(new CombinationListItem() {
					Number = counter++,
					Combination = p
				});
			});
		}

		private void SearchClearButton_Click(object sender, RoutedEventArgs e) {
			WalshSearch.Matrix = new WalshMatrix(8, 8);
		}

		private void CombinationList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var listView = sender as ListView;
			if (listView.SelectedIndex < 0) {
				return;
			}
			CurrentMatrixEdit.Text = (listView.SelectedIndex + 1).ToString();
		}
	}
}
