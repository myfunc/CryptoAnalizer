using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Mironov.Crypto.Polynom;

namespace Mironov.Crypto.View
{
    public partial class GroupListBox : UserControl
    {
		protected const int GroupCapacityLength = 4752;
		protected int groupPage = 0;
		public int GroupCount
		{
			get => (int)Math.Ceiling((decimal)polynomList.Count / (decimal)GroupCapacityLength);
		}
		protected int GroupPage
		{
			get => groupPage;
			set {
				groupPage = value;
				if (groupPage < 0) {
					groupPage = 0;
				} else if (polynomList.Count == 0) {
					groupPage = 0;
				} else if (value > GroupCount - 1) {
					groupPage = GroupCount - 1;
				}
				UpdateStatusBar();
			}
		}

		protected const int CellWidth = 25;
		protected List<Polynomial> polynomList = new List<Polynomial>();
		protected ObservableCollection<Grid> gridList = new ObservableCollection<Grid>();
		public bool IsCustomNumerable { get; set; }
		public bool IsReplaceNumbersByCustom { get; set; }

		public event EventHandler<PolynomEventArgs> OnSelectedChanged;

		private void OnSelectedChangedEmit(Polynomial group, int groupNumber) {
			Volatile.Read(ref OnSelectedChanged)?.Invoke(this, new PolynomEventArgs() {
				Polynom = group,
				Tag = groupNumber
			});
		}

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

		public void Clean() {
			PolynomListHeader.Items.Clear();
			gridList.Clear();
		}

		public void SetGroupRange(List<Polynomial> polynomList) {
			this.polynomList = polynomList.ToList();
        }

		public void AddGroup(Polynomial group) {
			this.polynomList.Add(group);
			if (polynomList.Count > 0 && polynomList.Count % GroupCapacityLength == 0) {
				Task.Factory.StartNew(()=>MessageBox.Show("Сгенерирована полная группа векторов.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information));
			}
		}

		protected void UpdateStatusBar() {
			StatusBar.Content = $"Массив на {GroupCapacityLength}, {GroupPage+1} из {GroupCount}";
		}

		public void Render() {
			Clean();
			if (this.polynomList.Count == 0) {
				return;
			}
			UpdateStatusBar();
			PolynomListHeader.Items.Add(CreateHeader(this.polynomList[0]));
			GenerateGroupList(this.polynomList.Skip(GroupCapacityLength * GroupPage).Take(GroupCapacityLength).ToList());
		}

        protected void GenerateGroupList(List<Polynomial> polynomList) {	
			foreach (var group in polynomList) {
				Grid gi = CreateItem(group);
				gi.Tag = group;

				int gridColumn = 0;
				gi.Children.Add(CreateLabelCell(gridList.Count + GroupPage * GroupCapacityLength, 0, gridColumn++, Brushes.WhiteSmoke));
				foreach (var poly in group) {
					gi.Children.Add(CreateLabelCell(poly.GetCustomNumberOrDefault().ToString().PadLeft(4, '0'), 0, gridColumn++));
				}
				gridList.Add(gi);
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
				var iterNumber = CreateLabelCell(i, 0, gridColumn++, Brushes.WhiteSmoke);
				iterNumber.HorizontalContentAlignment = HorizontalAlignment.Center;
				gi.Children.Add(iterNumber);
			}
			return gi;
		}

		private void CopyButtonClick(object sender, RoutedEventArgs e) {
			Clipboard.SetText(polynomList.GetGroupListText());
			MessageBox.Show("Скопировано в буфер обмена.");
		}

		private void PolynomListBody_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var list = sender as ListBox;
			if (list.SelectedItem != null) {
				int groupNumber = GroupCapacityLength * GroupPage + list.SelectedIndex;
				OnSelectedChangedEmit(polynomList[groupNumber], groupNumber);
			}
		}

		private void Next_Button_Click(object sender, RoutedEventArgs e) {
			GroupPage++;
			UpdateStatusBar();
			Clean();
		}

		private void Prev_Button_Click(object sender, RoutedEventArgs e) {
			GroupPage--;
			UpdateStatusBar();
			Clean();
		}
		private void Render_Button_Click(object sender, RoutedEventArgs e) {
			Render();
		}
	}
}