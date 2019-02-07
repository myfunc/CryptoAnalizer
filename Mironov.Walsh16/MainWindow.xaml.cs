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
using System.Windows.Threading;

namespace Mironov.Walsh16
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

		List<WalshMatrix> _matrixList;
		ObservableCollection<CombinationListItem> _combinationOpenedList = new ObservableCollection<CombinationListItem>();
		ObservableCollection<CombinationListItem> _combinationFriendList = new ObservableCollection<CombinationListItem>();
		int _currentMatrixIndex;

		public void Init() {
			//WalshPreview.IsReadOnly = true;
			CombinationList.ItemsSource = _combinationOpenedList;
			AnotherCombinationList.ItemsSource = _combinationFriendList;
			InitMatrixList();
		}

		void InitMatrixList() {
			GenerateSimButton_Click(null, new RoutedEventArgs());
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

				var colList = (List<int>)_matrixList[index - 1].Tag;
				if (colList[0] != 0) {
					colList.Insert(0, 0);
				}
				int[] columns = colList.ToArray();

				WalshPreview.ColumnNums = columns;
				WalshPreview.RowNums = columns;
				_currentMatrixIndex = index - 1;
				textBox.Background = Brushes.White;

				UpdateAnotherMatrixes();
			} catch (Exception ex) {
				textBox.Background = Brushes.Red;
			}
		}

		private void UpdateAnotherMatrixes() {
			_combinationFriendList.Clear();
			MatrixMaster master = new MatrixMaster();
			master.InitFriendlyMatrixList();
			master.InitMatrixList();
			var friendlyIndexes = master.FriendlyMatrixList.Where((p, ind) => {
				if (p.Equals(WalshPreview.Matrix)) {
					_combinationFriendList.Add(new CombinationListItem() {
						Matrix = master.FriendlyMatrixList[ind].Clone(),
						Number = ind + 1,
						Combination = p.TextTag
					});
					return true;
				}
				return false;
			}).ToList();
			if (_combinationFriendList.Count > 0) {
				AnotherCombinationList.SelectedIndex = 0;
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
			ShowMask();
			ActivateMainButton();
			MenuHeader.Header = "Режим: ПС - СП";
			MatrixMaster master = new MatrixMaster();
			master.InitCombinations();
			master.InitMatrixList();
			List<int> combList = new List<int>();
			_matrixList = master.MatrixList.Where(p => p.IsSemetric()).ToList();
			ResetPreviewMatrix();
			ResetCombinationList();
			HideMask();
		}

		private void GenerateAllButton_Click(object sender, RoutedEventArgs e) {
			ShowMask();
			ActivateMainButton();
			MenuHeader.Header = "Режим: ПС";
			MatrixMaster master = new MatrixMaster();
			master.InitCombinations();
			master.InitMatrixList();
			_matrixList = master.MatrixList.ToList();
			ResetPreviewMatrix();
			ResetCombinationList();
			HideMask();
		}

		private void GenerateFriendSimButton_Click(object sender, RoutedEventArgs e) {
			ShowMask();
			ActivateFriendlyButton();
			MenuHeader.Header = "Режим: ДП - СП";
			MatrixMaster master = new MatrixMaster();
			master.InitCombinations();
			master.InitFriendlyMatrixList();
			_matrixList = master.FriendlyMatrixList.Where((p, ind) => {
				return p.IsSemetric();
			})
				.Distinct(new WalshMatrixComparer())
				.ToList();
			ResetPreviewMatrix();
			ResetCombinationList();
			HideMask();
		}

		private void GenerateFriendAllButton_Click(object sender, RoutedEventArgs e) {
			ShowMask();
			ActivateFriendlyButton();
			MenuHeader.Header = "Режим: ДП";
			MatrixMaster master = new MatrixMaster();
			master.InitCombinations();
			master.InitFriendlyMatrixList();
			_matrixList = master.FriendlyMatrixList
				.ToList();
			ResetPreviewMatrix();
			ResetCombinationList();
			HideMask();
		}

		private void ResetPreviewMatrix() {
			_currentMatrixIndex = 0;
			WalshPreview.Matrix = _matrixList[_currentMatrixIndex];
			SetMatrixCountLabel(_matrixList.Count);
			SetCurrentMatrix(_currentMatrixIndex);
		}

		private void ResetCombinationList(List<int> combinationList) {
			_combinationOpenedList.Clear();
			int counter = 0;
			_matrixList.ForEach(p => {
				_combinationOpenedList.Add(new CombinationListItem() {
					Number = combinationList[counter++]+1,
					Combination = p.TextTag,
					Matrix = p
				});
			});
		}

		private void ResetCombinationList() {
			_combinationOpenedList.Clear();
			int counter = 1;
			_matrixList.ForEach(p => {
				_combinationOpenedList.Add(new CombinationListItem() {
					Number = counter++,
					Combination = p.TextTag,
					Matrix = p
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

		private void AnotherCombinationList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			CombinationListItem item = ((ListView)sender).SelectedItem as CombinationListItem;
			if (item == null) {
				return;
			}
			WalshAnother.Matrix = item.Matrix;
			if (item.Matrix.Tag != null) {
				List<int> tag = item.Matrix.Tag as List<int>;
				if (tag[0] != 0) {
					tag.Insert(0, 0);
				}
				WalshAnother.ColumnNums = tag.ToArray();
				WalshAnother.RowNums = tag.ToArray();
			}
		}

		private void ActivateMainButton() {
			MainSimButton.IsEnabled = true;
			FriendlySimButton.IsEnabled = false;
		}

		private void ActivateFriendlyButton() {
			MainSimButton.IsEnabled = false;
			FriendlySimButton.IsEnabled = true;
		}

		private void ShowMask() {
			Action EmptyDelegate = delegate () { };
			LoadingMask.Visibility = Visibility.Visible;
			Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
		}

		private void HideMask() {
			LoadingMask.Visibility = Visibility.Hidden;
		}
	}
}
