using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Crypto.Polynom
{
	[Serializable]
    public abstract class Polynomial : IEnumerable<Polynomial>, IDisposable
    {
        public abstract bool[] Row { get; }
        public abstract int Size { get; }
        public abstract Polynomial Next { get; }
        public virtual int Number { get; set; }

        public int Weight {
            get { return Row.Count((p) => p); }
        }

        public virtual string Hex {
            get {
                int result = 0;
                for (int i = 0; i < Size; i++) {
                    result <<= 1;
                    result |= Row[i] ? 1 : 0;
                }
                return string.Format(result.ToString("X2"));
            }
        }

        public virtual ulong Value {
            get { return Convert.ToUInt64(Hex, 16); }
        }

        public virtual IEnumerator<Polynomial> GetEnumerator() {
            return new PolynomialEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new PolynomialEnumerator(this);
        }

        public virtual void Dispose() {}
    }

    public class PolynomialEnumerator : IEnumerator<Polynomial>
    {
        protected Polynomial Start { get; set; }
        protected Polynomial Iterator { get; set; }

        public PolynomialEnumerator(Polynomial polynom) {
            Start = polynom;
            Iterator = null;
        }

        public Polynomial Current {
            get { return Iterator; }
        }

        object IEnumerator.Current {
            get { return Iterator; }
        }

        public void Dispose() {
            Start.Dispose();
        }

        public bool MoveNext() {
            if (Iterator == null) {
                Iterator = Start;
            } else {
                Iterator = Iterator.Next;
            }
            return Iterator != null;
        }

        public void Reset() {
            Iterator = Start;
        }
    }

	public interface ICustomNumberable
	{
		int CustomNumber { get; }
	}

	public static class PolynomExtensions
	{
		public static int GetCustomNumberOrDefault(this Polynomial obj) {
			if (obj is ICustomNumberable)
				return ((ICustomNumberable)obj).CustomNumber;
			return obj.Number;
		}
	}
}