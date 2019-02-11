using Microsoft.Win32;
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

namespace Mironov.PolynomView
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public List<ChainPolynom> EuqlidGroupList { get; set; } = new List<ChainPolynom>();
		public ChainPolynom MpsMatrix { get; set; }

		public MainWindow() {
			InitializeComponent();
			Init();
		}

		protected void Init() {

		}

		private void StartButton_OnClick(object sender, RoutedEventArgs e) {
			GenerateMatrix();
		}

		private void GenerateMatrix() {
			PolynomList.GenerateMatrix(GetMpsMatrix(), 16);

			var haming = new HamingPolynom(GetMpsMatrix(), 8, 1);
			HemingList.GenerateMatrix(haming, 16);
			ProcessDiffPair(GetMpsMatrix(), haming);
		}

		private ChainPolynom GetMpsMatrix() {
			if (MpsMatrix == null) {
				var mps = new FormatPolynomRev3(16, 8);
				MpsMatrix = new ChainPolynom(mps);
				MpsMatrix.Invert(1);
				MpsMatrix.Mirror(1);
				MpsMatrix.PolynomList.Insert(0, new CustomPolynom("0000000000000000"));
			}
			return MpsMatrix;

		}

		private void ProcessDiffPair(ChainPolynom source, HamingPolynom firstHaming) {
			var haming = firstHaming.Next as HamingPolynom;
			IncidentPairs.GenerateMatrix(new ChainPolynom(new List<Polynomial>() {
				new CustomPolynom(source.PolynomList[haming.CustomNumber - 1].Row){CustomNumber=haming.CustomNumber - 1 },
				new CustomPolynom(haming.Row){CustomNumber=haming.CustomNumber}
			}), 16);
		}

		private List<ChainPolynom> ProcessFullVectors(ChainPolynom chainPoly, int upperLimit) {
			ProcessFullVectors_Restart();
			int hemingLength = 8;
			var chainPolyList = chainPoly.PolynomList;

			var resultPoly = new ChainPolynom();
			resultPoly.PolynomList.Add(new CustomPolynom("0000000000000000"));

			var resultRange = new List<ChainPolynom>();

			ChainPolynom group = resultPoly.Clone();
			
			while (true) {
				int ignorPoly = 0;
				if (resultRange.Count > 0) {
					group = resultRange.Last().Clone();
					ignorPoly = group.Last().GetCustomNumberOrDefault();
					group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
				}
				while (true) {
					for (int i = ignorPoly + 1; i < chainPoly.PolynomList.Count; i++) {
						if (group.All(p => PolyUtils.GetHemingDiff(p, chainPolyList[i]) == hemingLength)) {
							var poly = new CustomPolynom(chainPolyList[i].Row);
							poly.CustomNumber = i;
							group.PolynomList.Add(poly);
						}
					}
					if (group.PolynomList.Count == upperLimit) {
						break;
					}
					if (group.PolynomList.Count != 16) {
						ignorPoly = group.Last().GetCustomNumberOrDefault();
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

			return resultRange;
		}

		private void ProcessFullVectors_Restart() {
			Dispatcher.Invoke(() => {
				GeneratorProgress.IsIndeterminate = true;
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

		private async void EuqlidGenerateButton_Click(object sender, RoutedEventArgs e) {
			if (!int.TryParse(UpperLimit.Text, out int upperLimit) && upperLimit > 16 && upperLimit < 2) {
				MessageBox.Show("Неверный верхний лимит. Должно быть число от 2 до 16");
			}
			var result = await Task.Factory.StartNew(()=>ProcessFullVectors(GetMpsMatrix(), upperLimit));
			GeneratorProgress.IsIndeterminate = false;
			MessageBox.Show($"Сгенерировано {EuqlidGroupList.Count} групп");
		}
	}
}