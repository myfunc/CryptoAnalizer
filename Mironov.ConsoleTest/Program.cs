using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
using Mironov.Crypto.Walsh;

namespace Mironov.ConsoleTest
{
	class RecExercise11
	{
		public static void Main() {
			var mps = new FormatPolynomRev2(16, 9);
			var chain = new ChainPolynom(mps);
			foreach (var i in chain) {
				Console.WriteLine(i.Number);
			}
		}
	}

}