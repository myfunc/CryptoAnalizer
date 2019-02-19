using Mironov.Crypto.Walsh;

namespace Mironov.PolynomView
{
	public class CombinationListItem
	{
		public int Number { get; set; }
		public string Combination { get; set; }
		public WalshMatrix Matrix { get; set; }
	}
}
