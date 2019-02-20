using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Crypto.Walsh
{
	public class WalshSemetricFinder
	{
		public List<List<bool>> Matrix { get; set; }
		public int Size { get; set; }

		public List<WalshMatrix> ResultMatrix { get; set; }

		public WalshSemetricFinder(WalshMatrix matrix) {
			Matrix = matrix.RealMatrix.Select(p=>p.ToList()).ToList();
			Size = matrix.Height;
		}

		private void TagMatrixes() {
			ResultMatrix.ForEach(matrix => {
				var tagList = new List<int>() { 0 };
				for (int i = 0; i < matrix.RealMatrix.Length; i++) {
					for (int j = 0; j < Matrix.Count; j++) {
						if (String.Join(",", Matrix[j]) == String.Join(",", matrix.RealMatrix[i])) {
							tagList.Add(j + 1);
							break;
						}
					}
				}
				matrix.Tag = tagList;
			});
		}

		public void Process() {
			ResultMatrix = GetSequenceMatrixList(new List<List<bool>>(), 0, Matrix.ToList());
			TagMatrixes();
		}

		public List<WalshMatrix> GetSequenceMatrixList(List<List<bool>> current, int level, List<List<bool>> pool) {
			var result = new List<WalshMatrix>();
			var suitablePool = pool.Where(p => ArrayBeginsFrom(p, GetStart(current, level))).ToList();
			for (int i = 0; i < suitablePool.Count; i++) {
				if (pool.Count == 1) {
					var finalCurrent = current.ToList();
					finalCurrent.Add(suitablePool[i]);
					result.Add(new WalshMatrix(Size, Size, finalCurrent.Select(p => p.ToArray()).ToArray()));
					break;
				}
				var nextCurrent = current.ToList();
				nextCurrent.Add(suitablePool[i]);
				var nextPool = pool.ToList();
				nextPool.Remove(suitablePool[i]);
				result.AddRange(GetSequenceMatrixList(nextCurrent, level + 1, nextPool));
			}
			return result;
		}

		public List<bool> GetStart(List<List<bool>> halfMatrix, int level) {
			var start = new List<bool>();
			for (int i = 0; i < level; i++) {
				start.Add(halfMatrix[i][level]);
			}
			return start;
		}

		public bool ArrayBeginsFrom(List<bool> target, List<bool> start) {
			for (int i = 0; i < start.Count; i++) {
				if (start[i] != target[i]) {
					return false;
				}
			}
			return true;
		}
	}
}
