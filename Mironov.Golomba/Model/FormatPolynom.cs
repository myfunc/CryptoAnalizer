using System;

namespace Mironov.Golomba.Model
{
    public class FormatPolynom : Polynomial, ICloneable
    {
        protected bool[] row = null;
        protected bool[] publicRow = null;
        public int Length { get; protected set; }

        public override bool[] Row {
            get {
                if (publicRow == null) {
                    publicRow = new bool[Size];
                    publicRow[0] = true;
                    publicRow[publicRow.Length - 1] = true;
                    Array.Copy(row, 0, publicRow, 1, row.Length);
                }
                return publicRow;
            }
        }

        public override int Size => row.Length + 2;

        public FormatPolynom(int width, int distance = 0) {
            Length = distance;
            width -= 2;
            distance -= 2;
            Number = 1;
            row = new bool[width];
            for (int i = 0; i < width; i++) {
                row[i] = i >= width - distance;
            }
        }

        public override Polynomial Next {
            get {
                FormatPolynom next = this.Clone();
                next.Number++;

                int keyBit = -1;
                for (int i = next.row.Length - 1; i > 0; i--) {
                    if (next.row[i] && !next.row[i - 1]) {
                        keyBit = i;
                        next.row[i - 1] = true;
                        next.row[i] = false;
                        break;
                    }
                }
                if (keyBit == -1) {
                    return null;
                }
                int bitCounter = 0;
                for (int i = keyBit; i < next.row.Length; i++) {
                    if (next.row[i]) {
                        bitCounter++;
                    }
                }

                for (int i = keyBit; i < next.row.Length; i++) {
                    next.row[i] = !(i < next.row.Length - bitCounter);
                }

                return next;
            }
        }

        object ICloneable.Clone() {
            var clone = new FormatPolynom(Size);
            clone.Number = this.Number;
            Array.Copy(this.row, clone.row, row.Length);
            return clone;
        }

        public FormatPolynom Clone() {
            return (this as ICloneable).Clone() as FormatPolynom;
        }
    }
}