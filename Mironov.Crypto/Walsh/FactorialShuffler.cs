using System.Collections.Generic;

namespace Mironov.Crypto.Walsh
{
	public class FactorialShuffler<T>
	{
		public FactorialShuffler() {
			Result = new List<List<T>>();
		}

		protected List<List<T>> Result { get; set; }
		protected void PrnPermut(T[] list, int iter = 0) {
			int i;
			List<T> subList;
			int length = list.Length - 1;
			if (list.Length - 1 == iter) {
				subList = new List<T>();
				for (i = 0; i <= length; i++)
					subList.Add(list[i]);
				Result.Add(subList);
			} else
				for (i = iter; i <= length; i++) {
					Swap(ref list[iter], ref list[i]);
					PrnPermut(list, iter + 1);
					Swap(ref list[iter], ref list[i]);
				}
		}

		protected void Swap(ref T a, ref T b) {
			T temp = a;
			a = b;
			b = temp;
		}
		public List<List<T>> Generate(T[] arr) {
			Result.Clear();
			PrnPermut(arr);
			return Result;
		}
	}
}
