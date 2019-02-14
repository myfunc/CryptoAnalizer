using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class GroupListBox : UserControl
    {
        protected const int CellWidth = 25;
		protected List<Polynomial> polynomList = new List<Polynomial>();
		protected ObservableCollection<Grid> gridList = new ObservableCollection<Grid>();
		public bool IsCustomNumerable { get; set; }
		public bool IsReplaceNumbersByCustom { get; set; }

		public string ListName {
            get { return PolynomListName.Content.ToString(); }
            set { PolynomListName.Content = value; }
        }

        public List<Polynomial> Polynoms {
            get { return polynomList; }
        }

        public GroupListBox() {
            InitializeComponent();
            InitProperties();
        }

        protected void InitProperties() {
			this.DataContext = this;
			IsCustomNumerable = false;
			IsReplaceNumbersByCustom = false;
			PolynomListBody.ItemsSource = gridList;
		}

		public void Clear() {
			PolynomListHeader.Items.Clear();
			gridList.Clear();
			polynomList.Clear();
		}

        public void GenerateMatrix(List<Polynomial> polynomList) {
			this.polynomList = polynomList.ToList();
			PolynomListHeader.Items.Add(CreateHeader(this.polynomList[0]));
			GenerateGroupList(polynomList);
        }

		public void AddGroup(Polynomial group) {
			if (PolynomListHeader.Items.Count == 0) {
				PolynomListHeader.Items.Add(CreateHeader(group));
			}

			this.polynomList.Add(group);
			Grid gi = CreateItem(group);
			gi.Tag = group;

			int gridColumn = 0;
			gi.Children.Add(CreateLabelCell(gridList.Count, 0, gridColumn++, Brushes.WhiteSmoke));
			foreach (var poly in group) {
				gi.Children.Add(CreateLabelCell(poly.GetCustomNumberOrDefault().ToString().PadLeft(4, '0'), 0, gridColumn++));
			}
			gridList.Add(gi);
		}

        protected void GenerateGroupList(List<Polynomial> polynomList) {
			Clear();
			int groupCount = 0;
			foreach (var group in polynomList) {
				Grid gi = CreateItem(group);
				gi.Tag = group;

				int gridColumn = 0;
				gi.Children.Add(CreateLabelCell(gridList.Count, 0, gridColumn++, Brushes.WhiteSmoke));
                foreach (var poly in group) {
					gi.Children.Add(CreateLabelCell(poly.GetCustomNumberOrDefault(), 0, gridColumn++));
				}
				gridList.Add(gi);
				groupCount++;
			}
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

        protected Grid CreateItem(Polynomial group) {
            Grid gi = new Grid();

			gi.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(2, GridUnitType.Star)
			});

			int length = group.Count();
			for (int i = 0; i < length; i++) {
				gi.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(0.8, GridUnitType.Star)
				});
			}
            return gi;
        }

		protected Grid CreateHeader(Polynomial prototype) {
			Grid gi = CreateItem(prototype);
			int gridColumn = 0;
			gi.Children.Add(CreateLabelCell("№", 0, gridColumn++, Brushes.WhiteSmoke));
			int length = prototype.Count();
			for (int i = 0; i < length; i++) {
				gi.Children.Add(CreateLabelCell(i + 1, 0, gridColumn++, Brushes.WhiteSmoke));
			}
			return gi;
		}

		private void CopyButtonClick(object sender, RoutedEventArgs e) {
			Clipboard.SetText(String.Join("\n", polynomList.Select(p => String.Join("\t", p.Select(a => a.GetCustomNumberOrDefault())))));
			MessageBox.Show("Скопировано в буфер обмена.");
		}
	}
}