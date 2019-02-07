namespace Mironov.Crypto.Walsh
{
	public interface IMatrix
	{
		int Width { get; }
		int Height { get; }
		bool this[int i, int j]{get;}
	}
}
