﻿using Microsoft.Win32;
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
		public List<ChainPolynom> EuqlidGroupList { get; set; } = new List<ChainPolynom>();
		public ChainPolynom MpsMatrix { get; set; }
		public const int VectorLength = 16;
		public const int HemingDiff = 8;
		public const int VectorWeight = 8;

		ObservableCollection<CombinationListItem> MatrixSequenceList = new ObservableCollection<CombinationListItem>();

		public ChainPolynom LastHamingPolynom { get; set; } = null;

		CancellationTokenSource cancelTokenSource;
		CancellationToken token;

		public MainWindow() {
			InitializeComponent();
			Init();
		}

		protected void Init() {
			AnotherCombinationList.ItemsSource = MatrixSequenceList;
			GenerateMatrix();
			DisableFullVectorBlock();
			SubscribeEvents();
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

		private void OnMpsGenerated(object sender, PolynomEventArgs args) {
			ProcessDiffPair(GetMpsMatrix(), new HamingPolynom(GetMpsMatrix(), HemingDiff, 1));
		}

		private void OnHamingChanged(object sender, PolynomEventArgs args) {
			LastHamingPolynom = new ChainPolynom((args.Polynom as ChainPolynom).ToList());
			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;
			ProcessFullVectors_Restart();
		}

		private void ProcessDiffPair(ChainPolynom source, HamingPolynom firstHaming) {
			var haming = firstHaming.Next.Next as HamingPolynom;
			IncidentPairs.GenerateMatrix(new ChainPolynom(new List<Polynomial>() {
				new CustomPolynom(source.PolynomList[haming.CustomNumber - 1].Row){CustomNumber=haming.CustomNumber - 1 },
				new CustomPolynom(haming.Row){CustomNumber=haming.CustomNumber}
			}), 16);
		}

		private List<ChainPolynom> resultRange = new List<ChainPolynom>();

		private void ProcessFullVectors(ChainPolynom chainPoly) {
			int limit = chainPoly.Size;
			int upperLimit = 2;
			int hemingLength = HemingDiff;
			var chainPolyList = chainPoly.PolynomList;

			var group = new ChainPolynom();
			group.PolynomList.Add(new CustomPolynom(0, VectorLength));
			
			while (true) {
				int ignorPoly = 0;
				if (resultRange.Count > 0) {
					group = resultRange.Last().Clone();
					ignorPoly = group.PolynomList.Last().Number;
					group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
				}
				while (true) {
					if (token.IsCancellationRequested) {
						return;
					}
					for (int i = ignorPoly + 1; i < chainPoly.PolynomList.Count; i++) {
						if (group.All(p => PolyUtils.GetHemingDiff(p, chainPolyList[i]) == hemingLength)) {
							var poly = new CustomPolynom(chainPolyList[i].Row);
							poly.CustomNumber = chainPolyList[i].GetCustomNumberOrDefault();
							poly.Number = i;
							group.PolynomList.Add(poly);
						}
					}
					if (group.PolynomList.Count == upperLimit) {
						break;
					}
					if (group.PolynomList.Count != limit) {
						ignorPoly = group.PolynomList.Last().Number;
						group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
						continue;
					}
					resultRange.Add(group);
					ProcessFullVectors_AddItem(group);
					break;
				}
				if (group.PolynomList.Count == upperLimit) {
					break;
				}
			}
		}

		private void ProcessFullVectors_Restart() {
			Dispatcher.Invoke(() => {
				resultRange.Clear();
				FullVectorsList.Clear();
				EuqlidGroupList.Clear();
				EuqlidGroupCountLabel.Content = "Кол-во групп: " + 0;
			});
		}

		private void ProcessFullVectors_AddItem(ChainPolynom item) {
			Dispatcher.Invoke(() => {
				EuqlidGroupList.Add(item);
				FullVectorsList.AddGroup(item);
				EuqlidGroupCountLabel.Content = "Кол-во групп: " + EuqlidGroupList.Count;
			});
		}

		private void EuqlidStopGeneration_Click(object sender, RoutedEventArgs e) {
			if (cancelTokenSource != null) {
				cancelTokenSource.Cancel();
			}
		}

		private async void EuqlidGenerateButton_Click(object sender, RoutedEventArgs e) {
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;

			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			ProcessFullVectors_Restart();
			await Task.Factory.StartNew(()=>ProcessFullVectors(LastHamingPolynom));

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;

			GeneratorProgress.IsIndeterminate = false;
		}

		private async void EuqlidContinueGenerationButton_Click(object sender, RoutedEventArgs e) {
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;
			GeneratorProgress.IsIndeterminate = true;
			EuqlidGenerateButton.IsEnabled = false;
			EuqlidStopGenerationButton.IsEnabled = true;
			EuqlidContinueGenerationButton.IsEnabled = false;

			await Task.Factory.StartNew(() => ProcessFullVectors(LastHamingPolynom));

			EuqlidGenerateButton.IsEnabled = true;
			EuqlidStopGenerationButton.IsEnabled = false;
			EuqlidContinueGenerationButton.IsEnabled = true;
			GeneratorProgress.IsIndeterminate = false;
		}
	}
}