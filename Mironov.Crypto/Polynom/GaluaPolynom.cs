using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mironov.Crypto.Utils;

namespace Mironov.Crypto.Polynom
{
	public class GaluaPolynom : Polynomial, ICloneable
    {
        protected bool[] row = null;
        protected Polynomial omega = null;
        protected Polynomial irred = null;

        public GaluaPolynom(int size) {
            row = new bool[size];
            Number = 1;
        }

        public GaluaPolynom(Polynomial omegaPolynomial, Polynomial irreduciblePolynomial) {
            this.omega = omegaPolynomial;
            this.irred = irreduciblePolynomial;
            row = new bool[irreduciblePolynomial.Size - 1];
            row[Size - 1] = true;
            Number = 1;
        }

        public bool IsComplete {
            get {
                return Number != 1 && Value == 1;
            }
        }

        public override Polynomial Next {
            get {
                if (IsComplete) {
                    return null;
                }
                Polynomial moduleMult = new CustomPolynom(PolyUtils.ModuleMult(this, omega, irred).Value, Size);
                GaluaPolynom next = this.Clone();
                next.Number += 1;
                Array.Copy(moduleMult.Row, next.Row, next.Row.Length);

                if (!IsComplete && next.Value == 1) {
                    return null;
                }
                return next;
            }
        }

        object ICloneable.Clone() {
            var clone = new GaluaPolynom(Size) {
                Number = this.Number,
                omega = this.omega,
                irred = this.irred
            };
            Array.Copy(this.row, clone.row, row.Length);
            return clone;
        }

        public GaluaPolynom Clone() {
            return (this as ICloneable).Clone() as GaluaPolynom;
        }

        public override bool[] Row {
            get { return row; }
        }

        public override int Size {
            get { return row.Length; }
        }
    }
}