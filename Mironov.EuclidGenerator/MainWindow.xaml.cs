using Microsoft.Win32;
using Mironov.Crypto;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
using Mironov.Crypto.View;
using Mironov.Crypto.Walsh;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Mironov.PolynomView
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private EuqlidGenerator euqlidGenerator;
		public List<ChainPolynom> EuqlidGroupList { get; set; } = new List<ChainPolynom>();
		public ChainPolynom MpsMatrix { get; set; }
		public const int VectorLength = 16;
		public const int HemingDiff = 8;
		public const int VectorWeight = 8;

		OpenFileDialog openFileDialog = new OpenFileDialog() {
			FileName = "full_euclid_tables.bin",
			Multiselect = false
		};
		SaveFileDialog saveFileDialog = new SaveFileDialog() {
			FileName = "full_euclid_tables.bin",
		};
		SaveFileDialog saveExcelFileDialog = new SaveFileDialog() {
			FileName = "table.csv",
		};

		ObservableCollection<CombinationListItem> MatrixSequenceList = new ObservableCollection<CombinationListItem>();

		public ChainPolynom LastHamingPolynom { get; set; } = null;

		public MainWindow() {
			InitializeComponent();
			Init();
		}

		protected void Init() {
			AnotherCombinationList.ItemsSource = MatrixSequenceList;
			InitEuqlidGenerator();
			GenerateMatrix();
			DisableFullVectorBlock();
			SubscribeEvents();
		}

		private void InitEuqlidGenerator() {
			euqlidGenerator = new EuqlidGenerator(VectorLength, HemingDiff) {
				IsCacheEnabled = true,
				Limit = 0
			};
			euqlidGenerator.OnAddGroup += ProcessFullVectors_AddGroup;
			euqlidGenerator.OnClear += ProcessFullVectors_Clear;
			euqlidGenerator.OnException += EuclidExceptionHandler;
		}

		private void EuclidExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
			Dispatcher.Invoke(() => {
				var ex = e.ExceptionObject as Exception;
				string errorMessage = string.Format("An unhandled exception occurred: {0}\n\nStack trace: {1}", ex.Message, ex.StackTrace);
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			});
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
				WalshAnother.RowNums = tag.ToArray();
			}
		}

		private void DisableFullVectorBlock() {
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = false;
		}

		private void GenerateMatrix() {
			var mpxMatrix = GetMpsMatrix();
			PolynomList.GenerateMatrix(mpxMatrix, VectorLength);
			HemingList.GenerateMatrix(mpxMatrix, VectorLength, HemingDiff);
		}

		private ChainPolynom GetMpsMatrix() {
			if (MpsMatrix == null) {
				var mps = new FormatPolynomRev3(VectorLength, HemingDiff);
				MpsMatrix = new ChainPolynom(mps);
				MpsMatrix.Invert(1);
				MpsMatrix.Mirror(1);
				MpsMatrix.PolynomList.Insert(0, new CustomPolynom(0, VectorLength));
			}
			return MpsMatrix;

		}

		private void SubscribeEvents() {
			PolynomList.OnGenerate += OnMpsGenerated;
			HemingList.OnGenerate += OnHamingChanged;
			FullVectorsList.OnSelectedChanged += OnEuclidGroupSelected;
		}

		private void OnEuclidGroupSelected(object sender, PolynomEventArgs args) {
			UpdateEqualitionList(args.Polynom);

			var group = args.Polynom.ToList().GetRange(1, VectorLength-1).Select(p=>p.Row.ToList().GetRange(1,VectorLength-1).ToArray()).ToArray();
			var invertedGroup = group.Select(p => p.ToArray()).ToArray();
			for (int i = 0; i < VectorLength -1; i++) {
				for (int j = 0; j < VectorLength - 1; j++) {
					invertedGroup[j][i] = group[i][j];
				}
			}
			var initialMatrix = new WalshMatrix(VectorLength, VectorLength, invertedGroup);
			WalshAnother.Matrix = new WalshMatrix(VectorLength, VectorLength);

			var finder = new WalshSemetricFinder(initialMatrix);
			finder.Process();
			int counter = 0;
			MatrixSequenceList.Clear();
			AnotherCombinationCount.Content = $"Группа: {args.Tag.ToString()}.                                Число сим. м-ц: {finder.ResultMatrix.Count}";
			finder.ResultMatrix.ForEach(p => {
				MatrixSequenceList.Add(new CombinationListItem() {
					Number = counter++,
					Combination = p.TextTagV2,
					Matrix = p,
				});
			});
		}

		private void UpdateEqualitionList(Polynomial group) {
			EqualitionList.Items.Clear();
			group.ToList().ForEach(p => {
				EqualitionList.Items.Add(new { Number = p.Number, CustomNumber = p.GetCustomNumberOrDefault().ToString().PadLeft(4, '0') });
			});
		}

		private void OnMpsGenerated(object sender, PolynomEventArgs args) {
			ProcessDiffPair(GetMpsMatrix(), new HamingPolynom(GetMpsMatrix(), HemingDiff, 1));
		}

		private void OnHamingChanged(object sender, PolynomEventArgs args) {
			EuqlidStopGeneration_Click(this, new RoutedEventArgs());
			ProcessFullVectors_Clear(this, new RoutedEventArgs());

			LastHamingPolynom = new ChainPolynom(args.Polynom.ToList());
			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;
			euqlidGenerator.Restart();
		}

		private void ProcessDiffPair(ChainPolynom source, HamingPolynom firstHaming) {
			var haming = firstHaming.Next.Next as HamingPolynom;
			IncidentPairs.GenerateMatrix(new ChainPolynom(new List<Polynomial>() {
				new CustomPolynom(source.PolynomList[haming.CustomNumber - 1].Row){CustomNumber=haming.CustomNumber - 1 },
				new CustomPolynom(haming.Row){CustomNumber=haming.CustomNumber}
			}), 16);
		}

		private void ProcessFullVectors_Clear(object sender, EventArgs args) {
			Dispatcher.Invoke(() => {
				FullVectorsList.Clear();
				EuqlidGroupList.Clear();
				EuqlidGroupCountLabel.Content = "Кол-во групп: " + 0;
			});
		}

		private void ProcessFullVectors_AddGroup(object sender, PolynomEventArgs args) {
			Dispatcher.Invoke(() => {
				EuqlidGroupList.Add(args.Polynom as ChainPolynom);
				FullVectorsList.AddGroup(args.Polynom as ChainPolynom);
				EuqlidGroupCountLabel.Content = "Кол-во групп: " + EuqlidGroupList.Count;
			});
		}

		private void EuqlidStopGeneration_Click(object sender, RoutedEventArgs e) {
			euqlidGenerator.PauseProcess();
			//FullVectorsList.Render();
		}

		private void EuqlidClearButton_Click(object sender, RoutedEventArgs e) {
			ProcessFullVectors_Clear(this, new RoutedEventArgs());
			euqlidGenerator.Restart();
		}

		private Task euclidGeneratorInProgress = Task.Factory.StartNew(()=> { });

		private async void EuqlidGenerateButton_Click(object sender, RoutedEventArgs e) {
			await euclidGeneratorInProgress;

			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			euclidGeneratorInProgress = euqlidGenerator.BeginProcess(LastHamingPolynom);
			await euclidGeneratorInProgress;

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;

			GeneratorProgress.IsIndeterminate = false;
		}

		private async void EuqlidContinueGenerationButton_Click(object sender, RoutedEventArgs e) {
			await euclidGeneratorInProgress;

			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			euclidGeneratorInProgress = euqlidGenerator.ContinueProcess(LastHamingPolynom);
			await euclidGeneratorInProgress;

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;
			GeneratorProgress.IsIndeterminate = false;
		}

		private void SaveAllButton_Click(object sender, RoutedEventArgs e) {
			try {
				if (saveFileDialog.ShowDialog().GetValueOrDefault()) {
					euqlidGenerator.SaveCache(saveFileDialog.FileName);
				}
			}
			catch (Exception ex) {
				string errorMessage = string.Format("Не удалось сохранить файл: {0}\n\nStack trace: {1}", ex.Message, ex.StackTrace);
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private async void LoadAllButton_Click(object sender, RoutedEventArgs e) {
			try {
				if (openFileDialog.ShowDialog().GetValueOrDefault()) {
					euqlidGenerator.LoadCache(openFileDialog.FileName);
				}
			} catch (Exception ex) {
				string errorMessage = string.Format("Не удалось загрузить файл: {0}\n\nStack trace: {1}", ex.Message, ex.StackTrace);
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void SaveToExcelButton_Click(object sender, RoutedEventArgs e) {
			string table = EuqlidGroupList.GetGroupListText(";");
			try {
				if (saveExcelFileDialog.ShowDialog().GetValueOrDefault()) {
					using (FileStream fstream = new FileStream(saveExcelFileDialog.FileName, FileMode.Create)) {
						byte[] array = System.Text.Encoding.Default.GetBytes(table);
						fstream.Write(array, 0, array.Length);
						fstream.Close();
					}
				}
			} catch (Exception ex) {
				string errorMessage = string.Format("Не удалось сохранить файл: {0}\n\nStack trace: {1}", ex.Message, ex.StackTrace);
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}