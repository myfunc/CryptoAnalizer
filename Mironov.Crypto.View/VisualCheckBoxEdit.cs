using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mironov.Crypto.View
{
	class VisualCheckBoxEdit : Label
	{
		public VisualCheckBoxEdit() {
			ColorTrue = Brushes.White;
			ColorFalse = Brushes.LightGray;
			ContentTrue = "1";
			ContentFalse = "0";
			ColorOver = Brushes.LightBlue;
			Padding = new Thickness(0);
			Value = false;
		}
		bool _value = false;
		public bool Value {
			get => _value;
			set {
				_value = value;
				Background = _value ? ColorTrue : ColorFalse;
				Content = _value ? ContentTrue : ContentFalse;
			}
		}
		public Brush ColorTrue { get; set; }
		public Brush ColorFalse { get; set; }
		public string ContentTrue { get; set; }
		public string ContentFalse { get; set; }
		public Brush ColorOver { get; set; }
		public bool IsReadOnly { get; set; }

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			if (!IsReadOnly) {
				Value = !Value;
			}
		}

		protected override void OnMouseEnter(MouseEventArgs e) {
			if (IsReadOnly) {
				return;
			}
			Background = ColorOver;
		}

		protected override void OnMouseLeave(MouseEventArgs e) {
			if (IsReadOnly) {
				return;
			}
			Background = _value ? ColorTrue : ColorFalse;
		}
	}
}
