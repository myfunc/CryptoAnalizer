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
			mpsChain.PolinomList.Insert(0, new CustomPolynom("00000000000000000"));
			PolynomList.GenerateMatrix(mpsChain, 16);

			var haming = new HamingPolynom(mpsChain, 8, 1);
			HemingList.GenerateMatrix(haming, 16);
			ProcessDiffPair(mpsChain, haming);
			ProcessFullVectors(mpsChain);
		}

		private void ProcessDiffPair(ChainPolynom source, HamingPolynom firstHaming) {
			var haming = firstHaming.Next as HamingPolynom;
			IncidentPairs.GenerateMatrix(new ChainPolynom(new List<Polynomial>() {
				new CustomPolynom(source.PolinomList[haming.CustomNumber - 1].Row){CustomNumber=haming.CustomNumber - 1 },
				new CustomPolynom(haming.Row){CustomNumber=haming.CustomNumber}
			}), 16);
		}

		//private ChainPolynom CreateEuqlidGroup() {
		//	var resultPoly = new ChainPolynom();
		//	resultPoly.PolinomList.Add(new CustomPolynom("00000000000000000"));
		//	return resultPoly;
		//}

		private void ProcessFullVectors(ChainPolynom chainPoly) {
			int hemingLength = 8;
			var resultPoly = new ChainPolynom();
			resultPoly.PolinomList.Add(new CustomPolynom("00000000000000000"));
			var polinomList = chainPoly.PolinomList;
			var savedPos = new List<int>() { };
			for (int i = 1; i < polinomList.Count; i++) {
				if (savedPos.All(p=> PolyUtils.GetHemingDiff(polinomList[p], polinomList[i]) == hemingLength)) {
					var poly = new CustomPolynom(polinomList[i].Row);
					poly.CustomNumber = i;
					resultPoly.PolinomList.Add(poly);
					savedPos.Add(i);
					i = 1;
				}
			}

			FullVectorsList.GenerateMatrix(resultPoly, 16);
		}

		private void EuqlidGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}
	}
}