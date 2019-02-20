using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
using Mironov.Crypto.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mironov.Crypto
{
	public class EuqlidGenerator
	{
		private CancellationTokenSource cancelTokenSource;
		private CancellationToken token;
		public event EventHandler<PolynomEventArgs> OnAddGroup;
		public event EventHandler OnClear;

		public readonly int VectorLength;
		public readonly int HemingDiff;

		public EuqlidGenerator(int vectorLength, int hemingDiff) {
			VectorLength = vectorLength;
			HemingDiff = hemingDiff;
		}

		private void OnAddGroupEmit(Polynomial group) {
			Volatile.Read(ref OnAddGroup)?.Invoke(this, new PolynomEventArgs() {
				Polynom = group
			});
		}

		private void OnClearEmit() {
			Volatile.Read(ref OnClear)?.Invoke(this, new EventArgs());
		}

		private List<ChainPolynom> resultRange = new List<ChainPolynom>();

		public async Task BeginProcess(ChainPolynom poly) {
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;
			Restart();
			await Task.Factory.StartNew(() => ProcessFullVectors(poly, HemingDiff, VectorLength));
		}
		public async Task ContinueProcess(ChainPolynom poly) {
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;
			await Task.Factory.StartNew(() => ProcessFullVectors(poly, HemingDiff, VectorLength));
		}
		public void PauseProcess() {
			if (cancelTokenSource != null) {
				cancelTokenSource.Cancel();
			}
		}

		public void Restart() {
			resultRange.Clear();
			OnClearEmit();
		}

		private void ProcessFullVectors(ChainPolynom chainPoly, int hemingDiff, int vectorLength) {
			int limit = chainPoly.Size;
			int upperLimit = 2;
			int hemingLength = hemingDiff;
			var chainPolyList = chainPoly.PolynomList;

			var group = new ChainPolynom();
			group.PolynomList.Add(new CustomPolynom(0, vectorLength));

			while (true) {
				int ignorPoly = 0;
				if (resultRange.Count > 0) {
					group = resultRange.Last().Clone();
					ignorPoly = group.PolynomList.Last().Number;
					group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
				}
				while (true) {
					if (token.IsCancellationRequested) {
						return;
					}
					for (int i = ignorPoly + 1; i < chainPoly.PolynomList.Count; i++) {
						if (group.All(p => PolyUtils.GetHemingDiff(p, chainPolyList[i]) == hemingLength)) {
							var poly = new CustomPolynom(chainPolyList[i].Row);
							poly.CustomNumber = chainPolyList[i].GetCustomNumberOrDefault();
							poly.Number = i;
							group.PolynomList.Add(poly);
						}
					}
					if (group.PolynomList.Count == upperLimit) {
						break;
					}
					if (group.PolynomList.Count != limit) {
						ignorPoly = group.PolynomList.Last().Number;
						group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
						continue;
					}
					resultRange.Add(group);
					OnAddGroupEmit(group);
					break;
				}
				if (group.PolynomList.Count == upperLimit) {
					break;
				}
			}
		}
	}
}
