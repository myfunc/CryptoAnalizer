using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Mironov.Crypto.Polynom;

namespace Mironov.Crypto.View
{
    public partial class PolynomListBox : UserControl
    {
        protected const int CellWidth = 25;
		protected Polynomial InitialPolynom { get; set; }
        protected List<Polynomial> polynomList = new List<Polynomial>();
        public int LengthPolynom { get; protected set; }
		public bool IsHexVisible { get; set; }
		public bool IsCustomNumerable { get; set; }
		public bool IsReplaceNumbersByCustom { get; set; }
        public bool IsReverted { get; set; }
		public bool IsShowHeader { set => IsShowHeaderValue = value ? Visibility.Visible : Visibility.Collapsed; }
		public bool IsShowOptions { set => IsShowOptionsValue = value ? Visibility.Visible : Visibility.Collapsed; }
		public int PolynomCountToShow { get; set; } = 0;
		public bool IsAutoRun { get; set; } = false;


		public static readonly DependencyProperty IsShowHeaderValueProperty =
			DependencyProperty.Register("IsShowHeaderValue", typeof(Visibility), typeof(PolynomListBox), new FrameworkPropertyMetadata(Visibility.Visible) { BindsTwoWayByDefault = true });
		[Bindable(true)]
		public Visibility IsShowHeaderValue
		{
			get => (Visibility)GetValue(IsShowHeaderValueProperty);
			set => SetValue(IsShowHeaderValueProperty, value);
		}

		public static readonly DependencyProperty IsShowOptionsValueProperty =
			DependencyProperty.Register("IsShowOptionsValue", typeof(Visibility), typeof(PolynomListBox), new FrameworkPropertyMetadata(Visibility.Visible) { BindsTwoWayByDefault = true });
		[Bindable(true)]
		public Visibility IsShowOptionsValue
		{
			get => (Visibility)GetValue(IsShowOptionsValueProperty);
			set => SetValue(IsShowOptionsValueProperty, value);
		}

		public string ListName {
            get { return PolynomListName.Content.ToString(); }
            set { PolynomListName.Content = value; }
        }

		public int SkipCount { get {
				if (int.TryParse(SkipCountText.Text, out int resultNumber)) {
					if (resultNumber >= 0) {
						return resultNumber;
					}
				}
				MessageBox.Show("Введите число от 0 и выше.");
				return -1;
			}
		}

		public List<Polynomial> Polynoms {
            get { return polynomList; }
        }

        public PolynomListBox() {
            InitializeComponent();
            InitProperties();
        }

        protected void InitProperties() {
			this.DataContext = this;
            IsReverted = false;
			IsHexVisible = true;
			IsCustomNumerable = false;
			IsShowHeader = true;
			IsReplaceNumbersByCustom = false;
		}

		public void Clear() {
			PolynomListHeader.Items.Clear();
			PolynomListBody.ItemsSource = new List<Grid>();
		}

        public void GenerateMatrix(Polynomial polynom, int lengthPolynom) {
            LengthPolynom = lengthPolynom;
			InitialPolynom = polynom;
			if (IsAutoRun) {
				ShowMatrixButton_Click(this, new RoutedEventArgs());
			}
		}

		private void ShowMatrixButton_Click(object sender, RoutedEventArgs e) {
			PolynomListHeader.Items.Clear();
			PolynomListHeader.Items.Add(CreateHeader());
			if (IsShowHeaderValue == Visibility.Visible) {
				ListStatus.Content = "Кол-во: " + InitialPolynom.Count();
			}
			var polyList = PolynomList(InitialPolynom);
			if (IsReverted) {
				polyList = polyList.Reverse();
			}
			PolynomListBody.ItemsSource = polyList;
			if (IsReverted) {
				PolynomBodyScroll.ScrollToBottom();
			}
		}

		private Polynomial SkipPolynoms(Polynomial initialPolynom, int skipCount) {
			Polynomial temp = initialPolynom;
			for (int i = 0; i < skipCount && temp != null; i++) {
				temp = temp.Next;
			}
			return temp;
		}

		protected IEnumerable<Grid> PolynomList(Polynomial poly) {
            polynomList.Clear();
			int counter = 0;
            do {
                if (PolynomCountToShow > 0 && counter++ > PolynomCountToShow) {
                    break;
                }

                polynomList.Add(poly);
                Grid gi = CreateItem();
                gi.Tag = poly;

				int gridColumn = 0;

				if (IsCustomNumerable) {
					var customNumPoly = poly as ICustomNumberable;
					if (customNumPoly != null) {
						gi.Children.Add(CreateLabelCell(customNumPoly.CustomNumber, 0, gridColumn++, Brushes.WhiteSmoke));
					}
				}
				if (IsReplaceNumbersByCustom && poly is ICustomNumberable) {
					var customNumPoly = poly as ICustomNumberable;
					gi.Children.Add(CreateLabelCell(customNumPoly.CustomNumber, 0, gridColumn++, Brushes.WhiteSmoke));
				} else {
					gi.Children.Add(CreateLabelCell(poly.Number, 0, gridColumn++, Brushes.WhiteSmoke));
				}
                for (int j = 0; j < poly.Size; j++) {
                    gi.Children.Add(CreateLabelCell(poly.Row[j] ? 1 : 0, 0, gridColumn++,
                        poly.Row[j] ? Brushes.LightGray : Brushes.White));
                }
				if (IsHexVisible) {
					gi.Children.Add(CreateLabelCell(poly.Hex, 0, gridColumn++, Brushes.Azure));
				}
                yield return gi;
            } while ((poly = poly.Next) != null);
        }

        protected Label CreateLabelCell(object content, int row, int col, Brush background = null) {
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

        protected Grid CreateHeader() {
            Grid gi = CreateItem();
			int gridColumn = 0;
			if (IsCustomNumerable) {
				gi.Children.Add(CreateLabelCell("#", 0, gridColumn++, Brushes.WhiteSmoke));
			}
            gi.Children.Add(CreateLabelCell("№", 0, gridColumn++, Brushes.WhiteSmoke));
            for (int i = 0; i < LengthPolynom; i++) {
                gi.Children.Add(CreateLabelCell(i, 0, gridColumn++, Brushes.WhiteSmoke));
            }
			if (IsHexVisible) {
				gi.Children.Add(CreateLabelCell("0x", 0, gridColumn++, Brushes.WhiteSmoke));
			}
            return gi;
        }

        protected Grid CreateItem() {
            Grid gi = new Grid();
			if (IsCustomNumerable) {
				gi.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(2, GridUnitType.Star)
				});
			}
			gi.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(2, GridUnitType.Star)
            });
            for (int i = 0; i < LengthPolynom; i++) {
                gi.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = new GridLength(0.8, GridUnitType.Star)
                });
            }
			if (IsHexVisible) {
				gi.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(3, GridUnitType.Star)
				});
			}
            return gi;
        }

		private void CopyButtonClick(object sender, RoutedEventArgs e) {
			var sb = new StringBuilder();
			foreach (var poly in polynomList) {
				for (int i = 0; i < poly.Size; i++) {
					sb.Append(poly.Row[i] == true ? "1" : "0");
					if (i != poly.Size - 1) {
						sb.Append("\t");
					} else {
						sb.Append(Environment.NewLine);
					}
				}
            }
            Clipboard.SetText(sb.ToString());
        }

		private void CopyMetaButtonClick(object sender, RoutedEventArgs e) {
			var sb = new StringBuilder();
			foreach (var poly in polynomList) {
				sb.Append(poly.Number);
				sb.Append("\t");
				for (int i = 0; i < poly.Size; i++) {
					sb.Append(poly.Row[i] == true ? "1" : "0");
					sb.Append("\t");
				}
				sb.Append(poly.Hex);
				sb.Append("\t");
				sb.Append(new CustomPolynom(poly.Row.Reverse().ToArray()).Hex);
				sb.Append(Environment.NewLine);
			}
			Clipboard.SetText(sb.ToString());
		}
	}
}