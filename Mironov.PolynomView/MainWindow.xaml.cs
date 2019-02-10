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
			var mps = new FormatPolynomRev3(16, 8);
			var mpsChain = new ChainPolynom(mps);
			mpsChain.Invert(1);
			mpsChain.Mirror(1);
			mpsChain.PolynomList.Insert(0, new CustomPolynom("0000000000000000"));
			//PolynomList.GenerateMatrix(mpsChain, 16);

			//var haming = new HamingPolynom(mpsChain, 8, 1);
			//HemingList.GenerateMatrix(haming, 16);
			//ProcessDiffPair(mpsChain, haming);
			//ProcessFullVectors(mpsChain);
			ProcessFullVectors2(mpsChain);

			//EuqlidGroupList = GetEuqlidGroups(mpsChain.PolynomList, new List<Polynomial>() { new CustomPolynom("0000000000000000") }, 8);
			//for (int i = 0; i < EuqlidGroupList.Count; i++) {
			//	EuqlidGroups.Items.Add(i);
			//}

			var haming = new HamingPolynom(mpsChain, 8, 1);
			//HemingList.GenerateMatrix(haming, 16);
			ProcessDiffPair(mpsChain, haming);
		}

		private void ProcessDiffPair(ChainPolynom source, HamingPolynom firstHaming) {
			var haming = firstHaming.Next as HamingPolynom;
			IncidentPairs.GenerateMatrix(new ChainPolynom(new List<Polynomial>() {
				new CustomPolynom(source.PolynomList[haming.CustomNumber - 1].Row){CustomNumber=haming.CustomNumber - 1 },
				new CustomPolynom(haming.Row){CustomNumber=haming.CustomNumber}
			}), 16);
		}

		//private ChainPolynom CreateEuqlidGroup() {
		//	var resultPoly = new ChainPolynom();
		//	resultPoly.PolynomList.Add(new CustomPolynom("00000000000000000"));
		//	return resultPoly;
		//}

		private List<ChainPolynom> GetEuqlidGroups(List<Polynomial> polynoms, List<Polynomial> groupPolynoms, int hemingLength, int ignoringPolynomIndex = 0) {
			var resultRange = new List<ChainPolynom>();
			var resultPoly = new ChainPolynom();
			resultPoly.PolynomList.AddRange(groupPolynoms);
			int lastIndex = ignoringPolynomIndex;
			for (int i = lastIndex + 1; i < polynoms.Count; i++) {
				if (groupPolynoms.All(p => PolyUtils.GetHemingDiff(p, polynoms[i]) == hemingLength)) {
					var poly = new CustomPolynom(polynoms[i].Row);
					poly.CustomNumber = i;
					resultPoly.PolynomList.Add(poly);
					groupPolynoms.Add(poly);
				}
			}
			if (groupPolynoms.Count > 8) {
				resultRange.AddRange(GetEuqlidGroups(polynoms, groupPolynoms.GetRange(0, groupPolynoms.Count - 1), hemingLength, groupPolynoms.Last().GetCustomNumberOrDefault()));
			}
			if (groupPolynoms.Count == 16) {
				resultRange.Add(resultPoly);
			}
			return resultRange;
		}

		private void ProcessFullVectors2(ChainPolynom chainPoly) {
			int upperLimit = 6;
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
					break;
				}
				if (group.PolynomList.Count == upperLimit) {
					break;
				}
			}

			EuqlidGroupList = resultRange;
			for (int i = 0; i < EuqlidGroupList.Count; i++) {
				EuqlidGroups.Items.Add(i);
			}
		}

		private void ProcessFullVectors(ChainPolynom chainPoly) {
			int hemingLength = 8;
			var resultPoly = new ChainPolynom();
			resultPoly.PolynomList.Add(new CustomPolynom("00000000000000000"));
			var polinomList = chainPoly.PolynomList;
			var savedPos = new List<int>() { };
			for (int i = 1; i < polinomList.Count; i++) {
				if (savedPos.All(p=> PolyUtils.GetHemingDiff(polinomList[p], polinomList[i]) == hemingLength)) {
					var poly = new CustomPolynom(polinomList[i].Row);
					poly.CustomNumber = i;
					resultPoly.PolynomList.Add(poly);
					savedPos.Add(i);
					i = 1;
				}
			}
			FullVectorsList.GenerateMatrix(resultPoly, 16);
		}

		private void EuqlidGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			FullVectorsList.GenerateMatrix(EuqlidGroupList[(EuqlidGroups.SelectedItem as int?).GetValueOrDefault()], 16);
			FullVectorsDockPanel.Visibility = Visibility.Visible;
			FullVectorGroupPanel.Visibility = Visibility.Hidden;
		}

		private void ClearEuqlidGroup_Click(object sender, RoutedEventArgs e) {
			FullVectorsDockPanel.Visibility = Visibility.Hidden;
			FullVectorGroupPanel.Visibility = Visibility.Visible;
		}
	}
}