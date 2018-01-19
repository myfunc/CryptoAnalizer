using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Golomba.Model
{
    public abstract class Polynomial
    {
        public abstract bool[] Row { get; }
        public abstract int Size { get; }
        public abstract Polynomial Next { get; }
        public int Number { get; protected set; }

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
    }
}