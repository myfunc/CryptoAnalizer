using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Crypto.Walsh
{
	public class WalshMatrix : IMatrix, ICloneable
	{
		protected int realWidth;
		protected int realHeight;

		public int RealWidth { get => realWidth; }
		public int RealHeight { get => realHeight; }

		public int Width { get => realWidth + 1; }
		public int Height { get => realHeight + 1; }
		public bool[][] RealMatrix { get; set; }

		public object Tag { get; set; }

		public string TextTag { get {
				if (Tag is List<int>) {
					var arrTag = Tag as List<int>;
					if (arrTag[0] != 0) {
						arrTag.Insert(0, 0);
					}
					return string.Join("", arrTag);
				}
				return string.Empty;
			}
		}

		public bool this[int x, int y]
		{
			get {
				if (x == 0 || y == 0) {
					return false;
				}
				return RealMatrix[x - 1][y - 1];
			}
		}

		public WalshMatrix(int width, int height, bool[][] realMatrix = null) {
			Init(width, height, realMatrix);
		}

		protected void Init(int width, int height, bool[][] realMatrix) {
			realWidth = width - 1;
			realHeight = height - 1;
			InitMatrix(realMatrix);
		}

		// For example 0123456, 0213465, 6543210
		public WalshMatrix GetFriendlyByCombination(int[] combination) {
			WalshMatrix firstMatrix = Clone();

			for (int i = 0; i < realWidth; i++) {
				firstMatrix.RealMatrix[i] = RealMatrix[combination[i]].Clone() as bool[];
			}

			WalshMatrix secondMatrix = Clone();
			for (int i = 0; i < realWidth; i++) {
				for (int j = 0; j < realHeight; j++) {
					secondMatrix.RealMatrix[i][j] = firstMatrix.RealMatrix[i][combination[j]];
				}
			}

			return secondMatrix;
		}

		protected void InitMatrix(bool[][] realMatrix) {
			RealMatrix = new bool[realWidth][];
			for (int i = 0; i < realHeight; i++) {
				RealMatrix[i] = new bool[realHeight];
				if (realMatrix != null) {
					Array.Copy(realMatrix[i], RealMatrix[i], realHeight);
				}
			}
		}

		object ICloneable.Clone() {
			return Clone();
		}

		public WalshMatrix Clone() {
			return new WalshMatrix(Width, Height, RealMatrix) {
				Tag = this.Tag
			};
		}

		public bool IsSemetric() {
			for (int i = 0; i < Height; i++) {
				for (int j = 0; j < Width; j++) {
					if (this[i, j] != this[j, i]) {
						return false;
					}
				}
			}
			return true;
		}

		public override bool Equals(object obj) {
			if (this == obj) {
				return true;
			}
			WalshMatrix matrixObj = obj as WalshMatrix;
			for (int i = 0; i < RealHeight; i++) {
				for (int j = 0; j < RealWidth; j++) {
					if (RealMatrix[i][j] != matrixObj.RealMatrix[i][j]) {
						return false;
					}
				}
			}
			return true;
		}
	}

	public class WalshMatrixComparer : IEqualityComparer<WalshMatrix>
	{
		public bool Equals(WalshMatrix x, WalshMatrix y) {
			if (Object.ReferenceEquals(x, y)) return true;

			for (int i = 0; i < x.RealHeight; i++) {
				for (int j = 0; j < y.RealWidth; j++) {
					if (x.RealMatrix[i][j] != y.RealMatrix[i][j]) {
						return false;
					}
				}
			}
			return true;
		}

		public int GetHashCode(WalshMatrix obj) {
			int hashCode = 0;
			for (int i = 0; i < obj.RealHeight; i++) {
				hashCode ^= string.Join("", obj.RealMatrix[i]).GetHashCode();
			}
			return hashCode;
		}
	}
}
