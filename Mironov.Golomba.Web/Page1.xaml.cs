using Mironov.Crypto.Consts;
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

namespace Mironov.Golomba.Web
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
		private const int CellWidth = 25;

		public Polynomial OmegaPolynom
		{
			get { return new CustomPolynom(OmegaPolynomList.SelectedItem.ToString()); }
		}

		public Polynomial IrredPolynom
		{
			get { return new CustomPolynom(IrredPolynomList.SelectedItem.ToString()); }
		}

		public int LengthPolynom
		{
			get { return (LengthPolynomList.SelectedItem as int?).GetValueOrDefault(); }
		}

		public Page1() {
			InitializeComponent();
			Init();
		}

		protected void Init() {
			InitLengthPolinomList();
			InitOmegaList();
			InitIrredList();
			InitRankAnalisComboBox();
		}

		private void InitRankAnalisComboBox() {
			RankAnaliticComboBox.Items.Clear();
			for (int i = 1; i <= LengthPolynom; i++) {
				RankAnaliticComboBox.Items.Add(i);
			}
		}

		private void InitLengthPolinomList() {
			LengthPolynomList.Items.Clear();
			foreach (var rank in PolinomialConsts.GetAvailableLengthList()) {
				LengthPolynomList.Items.Add(rank);
			}
		}

		private void InitIrredList() {
			IrredPolynomList.Items.Clear();
			if (LengthPolynom > 0) {
				foreach (var polynom in PolinomialConsts.GetPolynoms(LengthPolynom)) {
					IrredPolynomList.Items.Add(polynom);
				}
			}
		}

		private void InitOmegaList() {
			for (int i = 0; i < 256; i++) {
				OmegaPolynomList.Items.Add(Convert.ToString(i, 2).PadLeft(8, '0'));
			}
		}

		void GenerateMatrix() {
			GaluaPolynom cm = new GaluaPolynom(OmegaPolynom, IrredPolynom);
			PolynomList.GenerateMatrix(cm, LengthPolynom);
		}

		private void StartButton_OnClick(object sender, RoutedEventArgs e) {
			ClearAnalisisInfo();
			if (OmegaPolynomList.SelectedItem == null || IrredPolynomList.SelectedItem == null) {
				MessageBox.Show("Пожалуйста, выберите непереводимый и образующий полином");
				return;
			}
			GenerateMatrix();
			InfoGaluaLength();
		}

		public void ClearAnalisisInfo() {
			InfoClear();
			RankAnaliticCountList.ItemsSource = new List<object>();
			RankAnaliticRangeList.ItemsSource = new List<object>();
		}

		private void PostulatCalculate(int rank) {
			bool[] bits = PolynomList.Polynoms.Select(p => p.Row[LengthPolynom - rank - 1]).ToArray();
			GolombaBitCount bitCount = GolombaUtils.GetBitCount(bits);
			List<GolombaBitInfo> bitInfos = GolombaUtils.GetPostulat1(bits);
			List<GolombaRangesInfo> rangeInfos = GolombaUtils.GetPostulat2(bitInfos);

			RankAnaliticCountList.ItemsSource = new List<GolombaBitCount>() { bitCount };
			RankAnaliticRangeList.ItemsSource = rangeInfos;
			PostulatTest(bitCount, rangeInfos);
		}

		private void PostulatTest(GolombaBitCount bitCount, List<GolombaRangesInfo> rangeInfos) {
			InfoClear();
			InfoGaluaLength();
			//InfoPostulat1(bitCount);
			//InfoPostulat2(rangeInfos);
		}

		private void InfoClear() {
			GolombaPostulatResultList.Items.Clear();
		}

		private void InfoPostulat2(List<GolombaRangesInfo> rangeInfos) {
			var secondResult = new AnalisisResult();
			secondResult.Description = "Второй постулат Голомба";
			secondResult.Result = rangeInfos.All(p => p.Range0 >= p.Range1 - 1 && p.Range0 <= p.Range1 + 1)
				? "Соблюдается"
				: "Не соблюдается";
			GolombaPostulatResultList.Items.Add(secondResult);
		}

		private void InfoPostulat1(GolombaBitCount bitCount) {
			var firstResult = new AnalisisResult();
			firstResult.Description = "Первый постулат Голомба";
			firstResult.Result = (bitCount.Bits0 >= bitCount.Bits1 - 1 && bitCount.Bits0 <= bitCount.Bits1 + 1)
				? "Соблюдается"
				: "Не соблюдается";
			GolombaPostulatResultList.Items.Add(firstResult);
		}

		private void InfoGaluaLength() {
			var countResult = new AnalisisResult();
			countResult.Description = "Кол-во элементов";
			countResult.Result = PolynomList.Polynoms.Count.ToString();
			GolombaPostulatResultList.Items.Add(countResult);
		}

		private void StartRankAnalisis_OnClick(object sender, RoutedEventArgs e) {
			if (RankAnaliticComboBox.SelectedItem != null && PolynomList.Polynoms.Count > 0) {
				int rank = (RankAnaliticComboBox.SelectedItem as int?).GetValueOrDefault();
				PostulatCalculate(rank - 1);
			}
		}

		private void LengthPolynomList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			InitIrredList();
			InitRankAnalisComboBox();
		}
	}
}
