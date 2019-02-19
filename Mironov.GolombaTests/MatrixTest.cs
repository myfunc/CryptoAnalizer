using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mironov.Crypto.Walsh;

namespace Mironov.GolombaTests
{
	[TestClass]
	public class WalshSemetricFinderTest
	{
		
		[TestMethod]
		public void MatrixCreateTest() {
			var sampleMatrix = new bool[][] {
				new bool[] { false, false, false, false, false, false, false, false},
				new bool[] { false, true, false,true,false,true,false,true},
				new bool[] { false, false, true,true,false,false,true,true},
				new bool[] { false, true, true,false,false,true,true,false},
				new bool[] { false, false, false,false,true,true,true,true},
				new bool[] { false, true, false,true,true,false,true,false},
				new bool[] { false, false, true,true,true,true,false,false},
				new bool[] { false ,true, true,false,true,false,false,true}
			};
			var testMatrix = MatrixMaster.GetWalshMatrix().Matrix;

			bool isEqual = true;
			for (int i = 0; i < sampleMatrix.Length; i++) {
				for (int j = 0; j < sampleMatrix.Length; j++) {
					if (sampleMatrix[i][j] != testMatrix[i][j]) {
						Assert.Fail("Matrixes is not equals");
					}
				}
			}

		}

		[TestMethod]
		public void CountSemetricTest() {
			var finder = new WalshSemetricFinder(MatrixMaster.GetWalshMatrix());
			finder.Process();
			Assert.AreEqual(28, finder.ResultMatrix.Count);
		}

		[TestMethod]
		public void ValidSemetricTest() {
			var finder = new WalshSemetricFinder(MatrixMaster.GetWalshMatrix());
			finder.Process();
			Assert.AreEqual(28, finder.ResultMatrix.Distinct(new WalshMatrixComparer()).Where(p => p.IsSemetric()).Count());
		}
	}
}
