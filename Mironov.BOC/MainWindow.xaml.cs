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
using Mironov.Crypto.Consts;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;

namespace Mironov.BOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Polynomial InitialPolynom {
            get {
                var result = PolyUtils.Concat(
                    new CustomPolynom(1, 1),
                    new CustomPolynom(InitialPolynomList.SelectedItem.ToString())
                );
                return result;
            }
        }

        public int LengthPolynom {
            get { return (LengthPolynomList.SelectedItem as int?).GetValueOrDefault(); }
        }

        public MainWindow() {
            InitializeComponent();
            Init();
        }

        protected void Init() {
            InitLengthPolinomList();
            InitInitialPolynomList();
        }

        private void InitLengthPolinomList() {
            LengthPolynomList.Items.Clear();
            for (int i = 3; i <= 9; i++) {
                LengthPolynomList.Items.Add(i);
            }
        }

        private void InitInitialPolynomList() {
            InitialPolynomList.Items.Clear();
            if (LengthPolynom == 0) {
                return;
            }
            for (int i = 0; i < Math.Pow(2, LengthPolynom); i++) {
                InitialPolynomList.Items.Add(Convert.ToString(i, 2).PadLeft(LengthPolynom, '0'));
            }
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e) {
            GenerateMatrix();
        }

        private void GenerateMatrix() {
            var mps = new CheckMatrixV2Polynom(InitialPolynom, InitialPolynom,
                    PolyUtils.Concat(new CustomPolynom(1, 1), new CustomPolynom(1, LengthPolynom))
                )
                {Number = 1};
            PolynomList.GenerateMatrix(mps, LengthPolynom + 1);
        }

        private void LengthPolynomList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            InitInitialPolynomList();
        }

        private void TestButton_OnClick(object sender, RoutedEventArgs e) {
            
        }
    }
}