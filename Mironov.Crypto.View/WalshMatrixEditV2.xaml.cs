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
	/// Логика взаимодействия для WalshMatrixEditV2.xaml
	/// </summary>
	public partial class WalshMatrixEditEx : UserControl
	{
		bool _inited = false;

		bool _optimizationRebuild = false;

		public string ContentTrue { get; set; }
		public string ContentFalse { get; set; }

		VisualCheckBoxEdit[][] checkBoxMap;
		Label[] verticalNums;
		Label[] horizontalNums;

		public static readonly DependencyProperty IsReadOnlyProperty = 
			DependencyProperty.Register("IsReadOnly",typeof(bool),typeof(WalshMatrixEditEx),new FrameworkPropertyMetadata(false) {BindsTwoWayByDefault = true});
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

		public static readonly DependencyProperty DefaultWidthProperty =
			DependencyProperty.Register("MatrixWidth", typeof(int), typeof(WalshMatrixEditEx), new FrameworkPropertyMetadata(8) { BindsTwoWayByDefault = true });
		[Bindable(true)]
		public int MatrixWidth
		{
			get => (int)GetValue(DefaultWidthProperty);
			set => SetValue(DefaultWidthProperty, value);
		}

		public static readonly DependencyProperty DefaultHeightProperty =
			DependencyProperty.Register("MatrixHeight", typeof(int), typeof(WalshMatrixEditEx), new FrameworkPropertyMetadata(8) { BindsTwoWayByDefault = true });
		[Bindable(true)]
		public int MatrixHeight
		{
			get => (int)GetValue(DefaultHeightProperty);
			set => SetValue(DefaultHeightProperty, value);
		}


		public WalshMatrixEditEx() {
			InitializeComponent();
			Init();
		}

		WalshMatrix _matrix = null;

		public WalshMatrix Matrix
		{
			get => GetMatrix();
			set {
				SetMatrix(value);
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
			ContentTrue = "1";
			ContentFalse = "0";
		}

		void Build() {
			_inited = true;
			if (!_optimizationRebuild) {
				MainGridView.Children.Clear();
				InitMatrixIfNeeded();
				InitArrays();
				InitGridColumns();
				InitNumbers();
				InitStyleButton();
				InitGrid();
			}
			SetCellValues();
		}

		void InitMatrixIfNeeded() {
			if (_matrix == null) {
				_matrix = new WalshMatrix(MatrixWidth, MatrixHeight);
			}
		}

		void SetCellValues() {
			for (int i = 0; i < _matrix.Width; i++) {
				for (int j = 0; j < _matrix.Height; j++) {
					var checkBox = checkBoxMap[j][i];
					checkBox.ContentFalse = ContentFalse;
					checkBox.ContentTrue = ContentTrue;
					checkBox.Value = _matrix[i, j];
				}
			}
		}

		void InitGridColumns() {
			MainGridView.ColumnDefinitions.Clear();
			for (int i = 0; i < MatrixHeight + 1; i++) {
				MainGridView.ColumnDefinitions.Add(new ColumnDefinition());
			}
			MainGridView.RowDefinitions.Clear();
			for (int i = 0; i < MatrixWidth + 1; i++) {
				MainGridView.RowDefinitions.Add(new RowDefinition());
			}
		}

		void InitGrid() {
			for (int i = 0; i < MatrixWidth; i++) {
				for (int j = 0; j < MatrixHeight; j++) {
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

		void InitStyleButton() {
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
			horizontalNums = new Label[MatrixWidth];
			MainGridView.ColumnDefinitions[0].Width = new GridLength(15);
			for (int i = 0; i < MatrixWidth; i++) {
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

			verticalNums = new Label[MatrixHeight];
			MainGridView.RowDefinitions[0].Height = new GridLength(15);
			for (int i = 0; i < MatrixHeight; i++) {
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

		void InitArrays() {
			if (_optimizationRebuild) {
				return;
			}

			checkBoxMap = new VisualCheckBoxEdit[MatrixWidth][];
			for (int i = 0; i < MatrixWidth; i++) {
				checkBoxMap[i] = new VisualCheckBoxEdit[MatrixHeight];
			}
		}

		private void SetMatrix(WalshMatrix value) {
			if (_inited && MatrixWidth == value.Width && MatrixHeight == value.Height) {
				_optimizationRebuild = true;
			}
			MatrixWidth = value.Width;
			MatrixHeight = value.Height;
			_matrix = value;

			Build();
			_optimizationRebuild = false;
		}

		protected override void OnRender(DrawingContext drawingContext) {
			if (!_inited) {
				Build();
			}
			base.OnRender(drawingContext);
		}

		WalshMatrix GetMatrix() {
			if (!_inited) {
				return new WalshMatrix(MatrixWidth, MatrixHeight);
			}
			var matrix = new bool[MatrixWidth - 1][];
			for (int i = 0; i < MatrixWidth - 1; i++) {
				matrix[i] = new bool[MatrixHeight - 1];
				for (int j = 0; j < MatrixHeight - 1; j++) {
					matrix[i][j] = checkBoxMap[j + 1][i + 1].Value;
				}
			}
			return new WalshMatrix(MatrixWidth, MatrixHeight, matrix);
		}

		bool _isContentNums = true;
		private void Button_Click(object sender, RoutedEventArgs e) {
			_isContentNums = !_isContentNums;
			ContentTrue = _isContentNums ? "1" : "-";
			ContentFalse = _isContentNums ? "0" : "+";
			SetCellValues();
		}
	}
}
