using Mironov.Crypto.Utils;
using System.Collections.Generic;
using System;

namespace Mironov.Crypto.Polynom
{
	public class HamingPolynom : ChainPolynom, ICustomNumberable, ICloneable
	{
		protected int hemingLength;
		protected int checkPolynomIndex;

		public HamingPolynom(Polynomial rootPolynom, int hemingLength, int checkPolynomIndex) : base(rootPolynom) {
			this.hemingLength = hemingLength;
			this.checkPolynomIndex = checkPolynomIndex;
		}

		public HamingPolynom(List<Polynomial> polinomList, int hemingLength, int checkPolynomIndex) : base(polinomList) {
			this.hemingLength = hemingLength;
			this.checkPolynomIndex = checkPolynomIndex;
		}

		public HamingPolynom(List<Polynomial> polinomList, int currentIndex, int hemingLength, int checkPolynomIndex) : base(polinomList, currentIndex) {
			this.hemingLength = hemingLength;
			this.checkPolynomIndex = checkPolynomIndex;
		}

		public override Polynomial Next
		{
			get {
				if (IsComplete) return null;
				for (int i = currentIndex + 1; i < polinomList.Count; i++) {
					if (i == checkPolynomIndex || (i > checkPolynomIndex && PolyUtils.GetHemingDiff(polinomList[checkPolynomIndex], polinomList[i]) == hemingLength)) {
						return new HamingPolynom(polinomList, i, hemingLength, checkPolynomIndex) {
							Number = this.Number + 1
						};
					}
				}
				return null;
			}
		}

		object ICloneable.Clone() {
			return new HamingPolynom(polinomList, currentIndex, hemingLength, checkPolynomIndex) {
				Number = this.Number
			};
		}

		public HamingPolynom Clone() {
			return (this as ICloneable).Clone() as HamingPolynom;
		}

		public override int CustomNumber => currentIndex;
	}
}