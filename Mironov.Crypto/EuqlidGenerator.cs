using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;
using Mironov.Crypto.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mironov.Crypto
{
	[Serializable]
	public class EuqlidGeneratorCacheItem
	{
		public int GroupNumber { get; set; }
		public bool[][][] Groups { get; set; }
		public bool IsFinished { get; set; }
		public int[][] CustomNumbers { get; set; }
		public int[][] Numbers { get; set; }
	}

	[Serializable]
	public class EuqlidGeneratorCache
	{
		public List<EuqlidGeneratorCacheItem> Items { get; set; } = new List<EuqlidGeneratorCacheItem>();
		public void AddToCache(List<ChainPolynom> matrix, int groupNumber, bool isFinished) {
			Items.Where(p => p.GroupNumber == groupNumber).ToList().ForEach(p => Items.Remove(p));
			Items.Add(new EuqlidGeneratorCacheItem() {
				GroupNumber = groupNumber,
				IsFinished = isFinished,
				Groups = matrix.Select(p => p.PolynomList.Select(k => k.Row.ToArray()).ToArray()).ToArray(),
				CustomNumbers = matrix.Select(p => p.PolynomList.Select(k => k.GetCustomNumberOrDefault()).ToArray()).ToArray(),
				Numbers = matrix.Select(p => p.PolynomList.Select(k => k.Number).ToArray()).ToArray()
			});
		}

		public List<ChainPolynom> RestoreCache(int groupNumber, out bool isFinished) {
			isFinished = false;
			var suitable = Items.Where(p => p.GroupNumber == groupNumber);
			if (suitable.Count() > 0) {
				var restored = suitable.First();
				isFinished = restored.IsFinished;
				var result = restored.Groups.Select(p => {
					return new ChainPolynom(p.Select(k => new CustomPolynom(k) as Polynomial).ToList());
					}).ToList();
				for (int i = 0; i < restored.CustomNumbers.Length; i++) {
					for (int j = 0; j < result[i].PolynomList.Count; j++) {
						var current = result[i].PolynomList[j] as CustomPolynom;
						current.CustomNumber = restored.CustomNumbers[i][j];
						current.Number = restored.Numbers[i][j];
					}
				}
				return result;
			}
			return new List<ChainPolynom>();
		}

		public void SaveToDisk(string path, int groupNumber) {
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = new FileStream(path, FileMode.Create)) {
				formatter.Serialize(fstream, Items[groupNumber]);
			}
		}

		public void LoadFromDisk(string path, int groupNumber) {
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = File.OpenRead(path)) {
				EuqlidGeneratorCacheItem item = formatter.Deserialize(fstream) as EuqlidGeneratorCacheItem;
				Items.Where(p => p.GroupNumber == groupNumber).ToList().ForEach(p => Items.Remove(p));
				Items.Add(item);
			}
		}

		public void SaveToDiskAll(string path) {
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = new FileStream(path, FileMode.Create)) {
				formatter.Serialize(fstream, Items);
			}
		}

		public void LoadFromDiskAll(string path) {
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = File.OpenRead(path)) {
				Items = formatter.Deserialize(fstream) as List<EuqlidGeneratorCacheItem>;
			}
		}
	}

	public class EuqlidGenerator
	{
		private EuqlidGeneratorCache cache = new EuqlidGeneratorCache();
		private CancellationTokenSource cancelTokenSource;
		private CancellationToken token;
		public event EventHandler<PolynomEventArgs> OnAddGroup;
		public event EventHandler OnClear;
		public event UnhandledExceptionEventHandler OnException;

		public int Limit { get; set; } = 0;
		public bool IsCacheEnabled { get; set; } = false;

		private ChainPolynom HamingPolynom { get; set; }
		public bool IsFinished { get; private set; } = false;

		public readonly int VectorLength;
		public readonly int HemingDiff;

		public EuqlidGenerator(int vectorLength, int hemingDiff) {
			VectorLength = vectorLength;
			HemingDiff = hemingDiff;
		}

		private int GetGroupNumber() {
			return HamingPolynom.PolynomList[1].GetCustomNumberOrDefault();
		}

		private void OnExceptionEmit(Exception e) {
			Volatile.Read(ref OnException)?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
		}

		private void OnAddGroupEmit(Polynomial group) {
			Volatile.Read(ref OnAddGroup)?.Invoke(this, new PolynomEventArgs() {
				Polynom = group
			});
		}

		private void OnClearEmit() {
			Volatile.Read(ref OnClear)?.Invoke(this, new EventArgs());
		}

		public void SaveCache(string filePath) {
			cache.SaveToDiskAll(filePath);
		}

		public void LoadCache(string filePath) {
			cache.LoadFromDiskAll(filePath);
		}

		void TryRestoreCache() {
			if (!IsCacheEnabled) return;

			bool isFinished;
			ResultRange = cache.RestoreCache(GetGroupNumber(), out isFinished);
			ResultRange.ForEach(p => {
				OnAddGroupEmit(p);
			});
		}

		void SaveToCache() {
			if (!IsCacheEnabled) return;

			cache.AddToCache(ResultRange, GetGroupNumber(), IsFinished);
		}

		public List<ChainPolynom> ResultRange { get; private set; } = new List<ChainPolynom>();

		public async Task BeginProcess(ChainPolynom poly) {
			HamingPolynom = poly;
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;
			Restart();

			bool isFinished = await Task.Factory.StartNew(() => ProcessFullVectors(HamingPolynom, HemingDiff, VectorLength));
		}
		public async Task ContinueProcess(ChainPolynom poly) {
			if (poly != HamingPolynom) {
				HamingPolynom = poly;
				Restart();
			} else if (IsFinished) {
				return;
			}
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;

			if (ResultRange.Count == 0) {
				TryRestoreCache();
			}

			IsFinished = await Task.Factory.StartNew(() => ProcessFullVectors(HamingPolynom, HemingDiff, VectorLength));
		}
		public void PauseProcess() {
			if (cancelTokenSource != null) {
				cancelTokenSource.Cancel();
			}
		}

		public void Restart() {
			ResultRange.Clear();
			OnClearEmit();
		}

		private bool ProcessFullVectors(ChainPolynom chainPoly, int hemingDiff, int vectorLength) {
			try {
				int limit = chainPoly.Size;
				int upperLimit = 2;
				int hemingLength = hemingDiff;
				var chainPolyList = chainPoly.PolynomList;

				var group = new ChainPolynom();
				group.PolynomList.Add(new CustomPolynom(0, vectorLength));

				while (true) {
					int ignorPoly = 0;
					if (ResultRange.Count > 0) {
						group = ResultRange.Last().Clone();
						ignorPoly = group.PolynomList.Last().Number;
						group.PolynomList.RemoveAt(group.PolynomList.Count - 1);
					}
					while (true) {
						if (token.IsCancellationRequested || (Limit > 0 && ResultRange.Count >= Limit)) {
							return false;
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
						ResultRange.Add(group);
						SaveToCache();
						OnAddGroupEmit(group);
						break;
					}
					if (group.PolynomList.Count == upperLimit) {
						break;
					}
				}
				return true;
			} catch (Exception e) {
				OnExceptionEmit(e);
				return true;
			}
		}
	}
}
