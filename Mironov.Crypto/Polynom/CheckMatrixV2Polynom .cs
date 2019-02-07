using System;
using Mironov.Crypto.Utils;

namespace Mironov.Crypto.Polynom
{
    public class CheckMatrixV2Polynom : Polynomial, ICloneable
    {
        protected bool[] row = null;
        protected Polynomial root = null;
        protected Polynomial stopFactor = null;

        public CheckMatrixV2Polynom(int size) {
            row = new bool[size];
            Number = 0;
        }

        public CheckMatrixV2Polynom(Polynomial poly, Polynomial root, Polynomial stopFactor = null) {
            if (stopFactor == null) {
                stopFactor = new CustomPolynom(1,1);
            }
            this.stopFactor = stopFactor;
            this.root = root;
            row = new bool[poly.Size];
            Number = 0;
            Array.Copy(poly.Row, row, row.Length);
        }

        public bool IsComplete {
            get { return Number != 1 && Value == stopFactor.Value; }
        }

        public override Polynomial Next {
            get {
                CheckMatrixV2Polynom next = this.Clone();
                next.Number++;
                if (!row[1]) {
                    for (int j = 1; j < next.Size - 1; j++) {
                        Swap(next, j);
                    }
                }
                else {
                    for (int j = 1; j < next.Size - 1; j++) {
                        Swap(next, j);
                    }
                    for (int j = 1; j < next.Size - 1; j++) {
                        next.Row[j] ^= root.Row[j];
                    }
                }
                if (next.IsComplete) {
                    return null;
                }
                return next;
            }
        }

        private static void Swap(CheckMatrixV2Polynom next, int j) {
            var tmp = next.Row[j];
            next.Row[j] = next.Row[j + 1];
            next.Row[j + 1] = tmp;
        }

        object ICloneable.Clone() {
            var clone = new CheckMatrixV2Polynom(Size) {
                Number = this.Number,
                root = this.root,
                stopFactor = this.stopFactor
            };
            Array.Copy(this.row, clone.row, row.Length);
            return clone;
        }

        public CheckMatrixV2Polynom Clone() {
            return (this as ICloneable).Clone() as CheckMatrixV2Polynom;
        }

        public override bool[] Row {
            get { return row; }
        }

        public override int Size {
            get { return row.Length; }
        }
    }
}