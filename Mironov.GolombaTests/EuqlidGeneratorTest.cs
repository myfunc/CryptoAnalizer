using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mironov.Crypto;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Walsh;

namespace Mironov.GolombaTests
{
	[TestClass]
	public class EuqlidGeneratorTest {
		string first22seed1 = "0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4545\t4693\t4894\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4545\t4695\t4892\t5368\t5450\t5651\n0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4577\t4661\t4894\t5336\t5484\t5649\n0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4577\t4695\t4860\t5336\t5450\t5683\n0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4579\t4661\t4892\t5334\t5484\t5651\n0\t1\t296\t525\t1565\t2528\t2764\t3028\t3239\t4305\t4579\t4693\t4860\t5334\t5452\t5683\n0\t1\t296\t525\t1565\t2528\t2764\t3033\t3234\t4305\t4545\t4688\t4899\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2764\t3033\t3234\t4305\t4545\t4695\t4892\t5368\t5445\t5656\n0\t1\t296\t525\t1565\t2528\t2764\t3035\t3232\t4305\t4545\t4688\t4899\t5368\t5450\t5651\n0\t1\t296\t525\t1565\t2528\t2764\t3035\t3232\t4305\t4545\t4693\t4894\t5368\t5445\t5656\n0\t1\t296\t525\t1565\t2528\t2830\t2978\t3239\t4305\t4479\t4743\t4894\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2830\t2978\t3239\t4305\t4577\t4661\t4894\t5270\t5534\t5649\n0\t1\t296\t525\t1565\t2528\t2830\t3033\t3184\t4305\t4479\t4688\t4949\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2833\t2978\t3234\t4305\t4476\t4743\t4899\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2833\t3028\t3184\t4305\t4476\t4693\t4949\t5368\t5452\t5649\n0\t1\t296\t525\t1565\t2528\t2833\t3028\t3184\t4305\t4579\t4693\t4860\t5265\t5452\t5738\n0\t1\t296\t525\t1565\t2528\t2862\t2946\t3239\t4305\t4479\t4743\t4894\t5336\t5484\t5649\n0\t1\t296\t525\t1565\t2528\t2862\t2946\t3239\t4305\t4545\t4693\t4894\t5270\t5534\t5649\n0\t1\t296\t525\t1565\t2528\t2862\t3035\t3150\t4305\t4545\t4693\t4894\t5270\t5445\t5738\n0\t1\t296\t525\t1565\t2528\t2867\t2946\t3232\t4305\t4545\t4693\t4894\t5265\t5534\t5656\n0\t1\t296\t525\t1565\t2528\t2867\t3028\t3150\t4305\t4476\t4693\t4949\t5334\t5452\t5683\n0\t1\t296\t525\t1565\t2528\t2867\t3028\t3150\t4305\t4545\t4693\t4894\t5265\t5452\t5738";

		private ChainPolynom GetMpsMatrix() {
			var formatList = new FormatPolynomRev3(16, 8);
			var resultMps = new ChainPolynom(formatList);
			resultMps.Invert(1);
			resultMps.Mirror(1);
			resultMps.PolynomList.Insert(0, new CustomPolynom(0, 16));
			return resultMps;
		}

		private ChainPolynom GetHamingPolynom(int seedNumber) {
			var mpxMatrix = GetMpsMatrix();
			var haming = new HamingPolynom(mpxMatrix, 8, seedNumber);
			return new ChainPolynom(haming.ToList());
		}

		private async Task GenerateLimitTest(int limit) {
			var generator = new EuqlidGenerator(16, 8);
			generator.Limit = limit;

			var haming = GetHamingPolynom(1);

			await generator.BeginProcess(haming);
			var result = generator.ResultRange;

			Assert.AreEqual(limit, result.Count);
		}

		[TestMethod]
		public async Task Generate8Test() {
			await GenerateLimitTest(8);
		}

		[TestMethod]
		public async Task Generate16Test() {
			await GenerateLimitTest(16);
		}

		[TestMethod]
		public async Task TruesTest() {
			var result = new List<ChainPolynom>();
			var generator = new EuqlidGenerator(16, 8);
			generator.Limit = 22;
			generator.OnAddGroup += (o, e) => {
				result.Add(e.Polynom as ChainPolynom);
				};

			var haming = GetHamingPolynom(1);

			await generator.BeginProcess(haming);

			string actual = result.GetGroupListText();
			Assert.AreEqual(first22seed1, actual);
		}

		[TestMethod]
		public async Task PauseTest() {
			var result = new List<ChainPolynom>();
			var generator = new EuqlidGenerator(16, 8);
			generator.Limit = 22;
			generator.OnAddGroup += (o, e) => {
				result.Add(e.Polynom as ChainPolynom);
			};

			var haming = GetHamingPolynom(1);

			var begin = generator.BeginProcess(haming);
			await Task.Delay(5000);
			generator.PauseProcess();
			await generator.ContinueProcess(haming);

			string actual = result.GetGroupListText();
			Assert.AreEqual(first22seed1, actual);
		}

		[TestMethod]
		public async Task SaveResetCacheTest() {
			var result = new List<ChainPolynom>();
			var generator = new EuqlidGenerator(16, 8);
			generator.Limit = 22;
			generator.IsCacheEnabled = true;
			generator.OnAddGroup += (o, e) => {
				result.Add(e.Polynom as ChainPolynom);
			};

			var haming1 = GetHamingPolynom(1);
			var haming3 = GetHamingPolynom(3);

			result.Clear();
			var _ = generator.BeginProcess(haming1);
			await Task.Delay(5000);
			generator.PauseProcess();
			await _;

			result.Clear();
			_ = generator.BeginProcess(haming3);
			await Task.Delay(5000);
			generator.PauseProcess();
			await _;

			result.Clear();
			await generator.BeginProcess(haming1);

			string actual = result.GetGroupListText();
			Assert.AreEqual(first22seed1, actual);
		}

		[TestMethod]
		public async Task SaveLoadCacheTest() {
			var result = new List<ChainPolynom>();
			var generator = new EuqlidGenerator(16, 8);
			generator.Limit = 22;
			generator.IsCacheEnabled = true;
			generator.OnAddGroup += (o, e) => {
				result.Add(e.Polynom as ChainPolynom);
			};
			generator.OnClear += (o, e) => {
				result.Clear();
			};

			var haming1 = GetHamingPolynom(1);
			var haming3 = GetHamingPolynom(3);

			var _ = generator.BeginProcess(haming1);
			await Task.Delay(5000);
			generator.PauseProcess();
			await _;

			_ = generator.BeginProcess(haming3);
			await Task.Delay(5000);
			generator.PauseProcess();
			await _;

			await generator.ContinueProcess(haming1);

			string actual = result.GetGroupListText();
			Assert.AreEqual(first22seed1, actual);
		}
	}
}
