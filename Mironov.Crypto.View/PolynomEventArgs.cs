using System;
using Mironov.Crypto.Polynom;

namespace Mironov.Crypto.View
{
	public class PolynomEventArgs : EventArgs
	{
		public Polynomial Polynom { get; set; }
	}
}