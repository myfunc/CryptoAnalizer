using Mironov.Crypto.Walsh;
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

namespace Mironov.Crypto.View
{
	/// <summary>
	/// Логика взаимодействия для WalshMatrixEdit.xaml
	/// </summary>
	public partial class WalshMatrixEdit : UserControl
	{
		const int _width = 8;
		const int _height = 8;

		bool _inited = false;

		public string ContentTrue { get; set; }
		public string ContentFalse { get; set; }

		VisualCheckBoxEdit[][] checkBoxMap;
		Label[] verticalNums;
		Label[] horizontalNums;

		public static readonly DependencyProperty IsReadOnlyProperty
			= DependencyProperty.Register(
				  "IsReadOnly",
				  typeof(bool),
				  typeof(WalshMatrixEdit),
				  new FrameworkPropertyMetadata(false) {
					  BindsTwoWayByDefault = true
				  }
			  );

		[Bindable(true)]
		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set {
				for (int i = 0; i < _matrix.Width; i++) {
					for (int j = 0; j < _matrix.Height; j++) {
						if (j != 0 && i != 0) {
							checkBoxMap[i][j].IsReadOnly = value;
						}
					}
				}
				SetValue(IsReadOnlyProperty, value);
			}
		}

		public WalshMatrixEdit() {
			InitializeComponent();
			Init();
		}

		WalshMatrix _matrix = new WalshMatrix(8, 8);

		public WalshMatrix Matrix
		{
			get => GetMatrix();
			set {
				if (value == null) {
					_matrix = new WalshMatrix(8, 8);
				} else {
					_matrix = value;
				}
				Render();
			}
		}

		public int[] ColumnNums
		{
			set {
				if (value != null) {
					for (int i = 0; i < verticalNums.Length; i++) {
						verticalNums[i].Content = value[i].ToString();
					}
				}
			}
		}

		public int[] RowNums
		{
			set {
				if (value != null) {
					for (int i = 0; i < verticalNums.Length; i++) {
						horizontalNums[i].Content = value[i].ToString();
					}
				}
			}
		}

		void Init() {
			checkBoxMap = new VisualCheckBoxEdit[_width][];
			for (int i = 0; i < _width; i++) {
				checkBoxMap[i] = new VisualCheckBoxEdit[_height];
			}
			ContentTrue = "1";
			ContentFalse = "0";
		}

		void AfterInit() {
			_inited = true;
			InitNumbers();
			InitButton();
			for (int i = 0; i < _width; i++) {
				for (int j = 0; j < _height; j++) {
					VisualCheckBoxEdit label = new VisualCheckBoxEdit();
					Grid.SetColumn(label, j + 1);
					Grid.SetRow(label, i + 1);
					label.HorizontalContentAlignment = HorizontalAlignment.Center;
					label.VerticalContentAlignment = VerticalAlignment.Center;
					label.IsReadOnly = this.IsReadOnly;
					label.ContentFalse = ContentFalse;
					label.ContentTrue = ContentTrue;
					label.Value = label.Value;
					if (j == 0 || i == 0) {
						label.IsReadOnly = true;
					}

					MainGridView.Children.Add(label);
					checkBoxMap[i][j] = label;
				}
			}
		}

		void InitButton() {
			var button = new Button();
			button.Padding = new Thickness(0);
			button.Margin = new Thickness(0);
			button.Content = "~";
			button.Click += Button_Click;
			Grid.SetColumn(button, 0);
			Grid.SetRow(button, 0);
			MainGridView.Children.Add(button);
		}

		void InitNumbers() {
			horizontalNums = new Label[_width];
			MainGridView.ColumnDefinitions[0].Width = new GridLength(15);
			for (int i = 0; i < 8; i++) {
				Label label = new Label();
				Grid.SetColumn(label, 0);
				Grid.SetRow(label, i+1);
				label.Padding = new Thickness(0);
				label.Background = Brushes.WhiteSmoke;
				label.HorizontalContentAlignment = HorizontalAlignment.Center;
				label.VerticalContentAlignment = VerticalAlignment.Center;
				label.Content = (i).ToString();
				horizontalNums[i] = label;
				MainGridView.Children.Add(label);
			}

			verticalNums = new Label[_height];
			MainGridView.RowDefinitions[0].Height = new GridLength(15);
			for (int i = 0; i < 8; i++) {
				Label label = new Label();
				Grid.SetColumn(label, i+1);
				Grid.SetRow(label, 0);
				label.Background = Brushes.WhiteSmoke;
				label.Padding = new Thickness(0);
				label.HorizontalContentAlignment = HorizontalAlignment.Center;
				label.VerticalContentAlignment = VerticalAlignment.Center;
				label.Content = (i).ToString();
				verticalNums[i] = label;
				MainGridView.Children.Add(label);
			}
		}

		protected override void OnRender(DrawingContext drawingContext) {
			if (!_inited) {
				AfterInit();
			}
			base.OnRender(drawingContext);
		}

		void Render() {
			if (!_inited) {
				AfterInit();
			}
			if (_matrix == null) {
				return;
			}
			for (int i = 0; i < _matrix.Width; i++) {
				for (int j = 0; j < _matrix.Height; j++) {
					var checkBox = checkBoxMap[j][i];
					checkBox.ContentFalse = ContentFalse;
					checkBox.ContentTrue = ContentTrue;
					checkBox.Value = _matrix[i, j];
				}
			}
		}

		WalshMatrix GetMatrix() {
			if (!_inited) {
				AfterInit();
			}
			var matrix = new bool[_width - 1][];
			for (int i = 0; i < _width - 1; i++) {
				matrix[i] = new bool[_height - 1];
				for (int j = 0; j < _height - 1; j++) {
					matrix[i][j] = checkBoxMap[j + 1][i + 1].Value;
				}
			}
			return new WalshMatrix(8, 8, matrix);
		}

		bool _isContentNums = true;
		private void Button_Click(object sender, RoutedEventArgs e) {
			_isContentNums = !_isContentNums;
			ContentTrue = _isContentNums ? "1" : "-";
			ContentFalse = _isContentNums ? "0" : "+";
			Render();
		}
	}
}
