using System.Collections.Generic;
using System.Linq;

namespace Mironov.Crypto.Walsh
{
	public class MatrixMaster
	{
		public List<WalshMatrix> MatrixList;
		public List<WalshMatrix> FriendlyMatrixList;
		public List<string> CombinationList;
		public MatrixMaster() {

		}

		public void InitCombinations() {
			var shuffler = new FactorialShuffler<int>();
			var shuffleResult = shuffler.Generate(new int[] { 1, 2, 3, 4, 5, 6, 7 });
			CombinationList = shuffleResult.Select(p => {
				var res = new List<int>() { 0 };
				res.AddRange(p);
				return string.Join("", res);
			}).ToList();
		}
		public void InitMatrixList() {
			var shufflerComb = new FactorialShuffler<int>();
			var shuffleResult = shufflerComb.Generate(new int[] { 1, 2, 3, 4, 5, 6, 7 });

			var shufflerMatrix = new FactorialShuffler<bool[]>();
			bool[][] beginMatrix = GetInitialMatrix();
			var result = shufflerMatrix.Generate(beginMatrix);
			MatrixList = result.Select((p, ind) => {
				bool[][] matrix = p.ToArray();
				var newMatrix = new WalshMatrix(8, 8, matrix);
				newMatrix.Tag = shuffleResult[ind];
				return newMatrix;
			}).ToList();
		}

		public static WalshMatrix GetWalshMatrix() {
			return new WalshMatrix(8, 8, GetInitialMatrix());
		}

		public void InitFriendlyMatrixList() {
			var shuffler = new FactorialShuffler<int>();
			var shuffleResult = shuffler.Generate(new int[] { 0, 1, 2, 3, 4, 5, 6 });
			WalshMatrix startMatrix = new WalshMatrix(8, 8, GetInitialMatrix());
			FriendlyMatrixList = new List<WalshMatrix>();
			shuffleResult.ForEach(p => {
				var frMatrix = startMatrix.GetFriendlyByCombination(p.ToArray());
				var matrixTag = p.Select(k => k + 1).ToList();
				matrixTag.Insert(0, 0);
				frMatrix.Tag = matrixTag;
				FriendlyMatrixList.Add(frMatrix);
			});
		}

		static bool[][] GetInitialMatrix() {
			var initArray = new bool[][] {
				new bool[] {true,false,true,false,true,false,true},
				new bool[] {false,true,true,false,false,true,true},
				new bool[] {true,true,false,false,true,true,false},
				new bool[] {false,false,false,true,true,true,true},
				new bool[] {true,false,true,true,false,true,false},
				new bool[] {false,true,true,true,true,false,false},
				new bool[] {true,true,false,true,false,false,true}
			};
			return initArray;
		}
	}
}
