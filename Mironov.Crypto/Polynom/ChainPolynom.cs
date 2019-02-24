using System;
using Mironov.Crypto.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Mironov.Crypto.Polynom
{
	public class ChainPolynom : Polynomial, ICloneable, ICustomNumberable
	{
		public virtual List<Polynomial> PolynomList { get => polinomList; set => polinomList = value; }

		protected List<Polynomial> polinomList;
		protected int currentIndex;

		public ChainPolynom(Polynomial rootPolynom) {
			polinomList = new List<Polynomial>();
			do {
				polinomList.Add(rootPolynom);
			} while ((rootPolynom = rootPolynom.Next) != null);

			currentIndex = 0;
			Init();
		}

		public ChainPolynom() {
			polinomList = new List<Polynomial>();
			currentIndex = 0;
			Init();
		}

		public ChainPolynom(List<Polynomial> polinomList) {
			this.polinomList = polinomList;
			currentIndex = 0;
			Init();
		}

		public ChainPolynom(List<Polynomial> polinomList, int currentIndex) {
			this.polinomList = polinomList;
			this.currentIndex = currentIndex;
			Init();
		}

		protected virtual void Init() {
			Number = 0;
		}

		public virtual bool IsComplete { get => currentIndex >= polinomList.Count - 1; }

		public override Polynomial Next
		{
			get {
				if (IsComplete) return null;

				return new ChainPolynom(polinomList, currentIndex + 1) {
					Number = this.Number + 1
				};
			}
		}

		public virtual int CustomNumber => polinomList[currentIndex] is ICustomNumberable ? ((ICustomNumberable)polinomList[currentIndex]).CustomNumber: Number ;

		public void Mirror(int skipFirst = 0) {
			List<Polynomial> resultList = new List<Polynomial>();
			foreach (var polynom in polinomList) {
				var row = polynom.Row.ToArray();
				for (int i = skipFirst; i < row.Length; i++) {
					row[i] = polynom.Row[row.Length - i];
				}
				resultList.Add(new CustomPolynom(row));
			}
			polinomList = resultList;
		}

		public void Invert(int skipFirst = 0) {
			List<Polynomial> resultList = new List<Polynomial>();
			foreach (var polynom in polinomList) {
				var row = polynom.Row;
				for (int i = skipFirst; i < row.Length; i++) {
					row[i] = !row[i];
				}
				resultList.Add(new CustomPolynom(row));
			}
			polinomList = resultList;
		}

		object ICloneable.Clone() {
			return new ChainPolynom(polinomList.ToList(), currentIndex) {
				Number = this.Number
			};
		}

		public virtual ChainPolynom Clone() {
			return (this as ICloneable).Clone() as ChainPolynom;
		}

		public virtual ChainPolynom First() {
			return new ChainPolynom(polinomList, 0);
		}

		public virtual void SetIndex(int index) {
			this.currentIndex = index;
			this.Number = index;
		}

		public override bool[] Row
		{
			get { return polinomList[currentIndex].Row; }
		}

		public override int Size
		{
			get { return polinomList[currentIndex].Row.Length; }
		}

		
	}
}