﻿using System;
using System.Collections.Generic;
using System.IO;
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
using Mironov.Golomba.Model;

namespace Mironov.Golomba
{
    public partial class MainWindow : Window
    {
        private const int CellWidth = 25;
        private List<Polynomial> golombaPolynomList = new List<Polynomial>();

        public Polynomial OmegaPolynom {
            get { return new CustomPolynomial(OmegaPolynomList.SelectedItem.ToString()); }
        }

        public Polynomial IrredPolynom {
            get { return new CustomPolynomial(IrredPolynomList.SelectedItem.ToString()); }
        }

        public int LengthPolynom
        {
            get { return (LengthPolynomList.SelectedItem as int?).GetValueOrDefault(); }
        }

        public MainWindow() {
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
                foreach (var polynom in PolinomialConsts.GetPolynoms(LengthPolynom))
                {
                    IrredPolynomList.Items.Add(polynom);
                }
            }
        }

        private void InitOmegaList() {
            for (int i = 0; i < 256; i++) {
                OmegaPolynomList.Items.Add(Convert.ToString(i, 2).PadLeft(8, '0'));
            }
        }

        IEnumerable<Grid> PolynomList(Polynomial poly) {
            golombaPolynomList.Clear();
            yield return CreateHeader();
            do {
                if (poly.Number > 256) {
                    new Task(() =>
                            MessageBox.Show(
                                "При указанных параметрах регистр Галуа не имеет 01 в последовательности. Генерация прервана на 256 элементе."))
                        .Start();
                    break;
                }

                golombaPolynomList.Add(poly);
                Grid gi = CreateItem();
                gi.Tag = poly;

                gi.Children.Add(CreateLabelCell(poly.Number, 0, 0, Brushes.WhiteSmoke));
                for (int j = 0; j < poly.Size; j++) {
                    gi.Children.Add(CreateLabelCell(poly.Row[j] ? 1 : 0, 0, j + 1,
                        poly.Row[j] ? Brushes.LightGray : Brushes.White));
                }
                gi.Children.Add(CreateLabelCell(poly.Hex, 0, poly.Size + 1, Brushes.Azure));
                yield return gi;
            } while ((poly = poly.Next) != null);
            yield return CreateHeader();
        }

        Label CreateLabelCell(object content, int row, int col, Brush background = null) {
            if (background == null) {
                background = Brushes.White;
            }
            var cell = new Label();
            cell.Content = content;
            cell.Background = background;
            Grid.SetColumn(cell, col);
            Grid.SetRow(cell, row);
            return cell;
        }

        Grid CreateHeader() {
            Grid gi = CreateItem();
            gi.Children.Add(CreateLabelCell("№", 0, 0, Brushes.WhiteSmoke));
            for (int i = 0; i < LengthPolynom; i++) {
                gi.Children.Add(CreateLabelCell(LengthPolynom - i, 0, i + 1, Brushes.WhiteSmoke));
            }
            gi.Children.Add(CreateLabelCell("0x", 0, LengthPolynom + 1, Brushes.WhiteSmoke));
            return gi;
        }

        Grid CreateItem() {
            Grid gi = new Grid();
            gi.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(1, GridUnitType.Star)
            });
            for (int i = 0; i < LengthPolynom; i++) {
                gi.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = new GridLength(0.8, GridUnitType.Star)
                });
            }
            gi.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(3, GridUnitType.Star)
            });
            return gi;
        }

        void GenerateMatrix() {
            GaluaPolynom cm = new GaluaPolynom(OmegaPolynom, IrredPolynom);
            GaluaList.ItemsSource = PolynomList(cm);
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
            bool[] bits = golombaPolynomList.Select(p => p.Row[LengthPolynom - rank - 1]).ToArray();
            GolombaBitCount bitCount = GolombaUtils.GetBitCount(bits);
            List<GolombaBitInfo> bitInfos = GolombaUtils.GetPostulat1(bits);
            List<GolombaRangesInfo> rangeInfos = GolombaUtils.GetPostulat2(bitInfos);

            RankAnaliticCountList.ItemsSource = new List<GolombaBitCount>() {bitCount};
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
            countResult.Result = golombaPolynomList.Count.ToString();
            GolombaPostulatResultList.Items.Add(countResult);
        }

        private void StartRankAnalisis_OnClick(object sender, RoutedEventArgs e) {
            if (RankAnaliticComboBox.SelectedItem != null && golombaPolynomList.Count > 0) {
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