using Microsoft.Win32;
using Mironov.Crypto;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
using Mironov.Crypto.View;
using Mironov.Crypto.Walsh;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			euqlidGenerator = new EuqlidGenerator(VectorLength, HemingDiff);
			euqlidGenerator.OnAddGroup += ProcessFullVectors_AddGroup;
			euqlidGenerator.OnClear += ProcessFullVectors_Clear;
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
			LastHamingPolynom = new ChainPolynom((args.Polynom as ChainPolynom).ToList());
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
		}

		private async void EuqlidGenerateButton_Click(object sender, RoutedEventArgs e) {
			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			await euqlidGenerator.BeginProcess(LastHamingPolynom);

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;

			GeneratorProgress.IsIndeterminate = false;
		}

		private async void EuqlidContinueGenerationButton_Click(object sender, RoutedEventArgs e) {
			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			await euqlidGenerator.ContinueProcess(LastHamingPolynom);

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;
			GeneratorProgress.IsIndeterminate = false;
		}
	}
}