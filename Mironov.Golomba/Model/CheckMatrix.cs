using System;

namespace Mironov.Golomba.Model
{
    public class CheckMatrix : Polynomial, ICloneable
    {
        protected bool[] row = null;
        protected Polynomial root = null;

        public CheckMatrix(int size) {
            row = new bool[size];
            Number = 0;
        }

        public CheckMatrix(Polynomial poly, Polynomial root) {
            this.root = root;
            row = new bool[poly.Size];
            Number = 0;
            Array.Copy(poly.Row, row, row.Length);
        }

        public bool IsComplete {
            get {
                for (int i = 1; i < Size; i++) {
                    if (!Row[i]) {
                        return false;
                    }
                }
                return true;
            }
        }

        public override Polynomial Next {
            get {
                if (Number == 255) {
                    return null;
                }
                CheckMatrix next = this.Clone();
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

                return next;
            }
        }

        private static void Swap(CheckMatrix next, int j) {
            var tmp = next.Row[j];
            next.Row[j] = next.Row[j + 1];
            next.Row[j + 1] = tmp;
        }

        object ICloneable.Clone() {
            var clone = new CheckMatrix(Size) {
                Number = this.Number,
                root = this.root,
            };
            Array.Copy(this.row, clone.row, row.Length);
            return clone;
        }

        public CheckMatrix Clone() {
            return (this as ICloneable).Clone() as CheckMatrix;
        }

        public override bool[] Row {
            get { return row; }
        }

        public override int Size {
            get { return row.Length; }
        }
    }
}