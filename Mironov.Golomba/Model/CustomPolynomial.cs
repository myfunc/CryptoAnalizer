using System;
using System.Collections;
using System.Linq;

namespace Mironov.Golomba.Model
{
    public class CustomPolynomial : Polynomial
    {
        protected bool[] row = null;
        public bool IsError { get; private set; }

        public CustomPolynomial(string bitsText) {
            IsError = bitsText.Any(p => p != '1' && p != '0');
            bool[] poly = bitsText.Select(p => p == '1').ToArray();
            Init(poly);
        }

        public CustomPolynomial(ulong polynom, int size = 0) {
            if (size == 0) {
                size = 64;
            }
            var bits = new BitArray(BitConverter.GetBytes(polynom));
            Init(bits.Cast<bool>().Take(size).Reverse().ToArray());
        }

        public CustomPolynomial(bool[] polynom) {
            Init(polynom);
        }

        void Init(bool[] polynom) {
            row = new bool[polynom.Length];
            Array.Copy(polynom, row, row.Length);
        }

        public override Polynomial Next {
            get { return null; }
        }

        public override bool[] Row {
            get { return row; }
        }

        public override int Size {
            get { return row.Length; }
        }
    }
}