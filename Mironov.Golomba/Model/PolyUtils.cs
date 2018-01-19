using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mironov.Golomba.Model
{
    public class PolyUtils
    {
        public static Polynomial MultV2(Polynomial p1, Polynomial p2) {
            List<ulong> partList = new List<ulong>();
            for (int i = p1.Size - 1; i >= 0; i--) {
                if (p1.Row[i]) {
                    partList.Add(p2.Value << p2.Size - 1 - i);
                }
            }
            var partsBits = partList.Select(p => new BitArray(BitConverter.GetBytes((ulong) p)).Cast<bool>().ToList()).ToList();
            int highBitNumber = CountBits(partList[partList.Count - 1]);

            List<bool> resultBits = new List<bool>();
            for (int i = 0; i < highBitNumber; i++) {
                int bistAcc = 0;
                for (int j = 0; j < partsBits.Count; j++) {
                    bistAcc += partsBits[j][i] ? 1 : 0;
                }
                resultBits.Add(bistAcc % 2 == 1);
            }
            return new CustomPolynomial(resultBits.ToArray().Reverse().ToArray());
        }
        public static Polynomial Mult(Polynomial p1, Polynomial p2)
        {
            ulong result = 0;
            for (int i = p1.Size - 1; i >= 0; i--)
            {
                if (p1.Row[i])
                {
                    result ^= p2.Value << p2.Size - 1 - i;
                }
            }
            return new CustomPolynomial(result);
        }
        public static Polynomial MultOnModule(Polynomial p1, Polynomial p2)
        {
            List<ulong> partList = new List<ulong>();
            for (int i = p1.Size - 1; i >= 0; i--)
            {
                if (p1.Row[i])
                {
                    partList.Add(p2.Value << p2.Size - 1 - i);
                }
            }
            var partsBits = partList.Select(p => new BitArray(BitConverter.GetBytes(p)).Cast<bool>().ToList()).ToList();
            int highBitNumber = CountBits(partList[partList.Count - 1]);

            List<bool> resultBits = new List<bool>();
            for (int i = 0, bistAcc = 0; i < highBitNumber || bistAcc != 0; i++)
            {
                for (int j = 0; j < partsBits.Count; j++)
                {
                    bistAcc += partsBits[j][i] ? 1 : 0;
                }
                resultBits.Add(bistAcc % 2 == 1);
                if (bistAcc > 2)
                {
                    bistAcc -= 2;
                }
                else
                {
                    bistAcc = 0;
                }
            }
            return new CustomPolynomial(resultBits.ToArray().Reverse().ToArray());
        }

        public static Polynomial Pow(Polynomial poly, int power) {
            Polynomial result = poly;
            for (int i = 1; i < power; i++) {
                result = Mult(result, poly);
            }
            return result;
        }

        public static Polynomial Module(Polynomial main, Polynomial model) {
            int limit = CountBits(model.Value);
            if (CountBits(main.Value) >= limit) {
                ulong num = main.Value;
                return Module(new CustomPolynomial(num ^ model.Value << CountBits(main.Value) - CountBits(model.Value)),
                    model);
            }
            return main;
        }

        public static Polynomial ModuleMult(Polynomial p1, Polynomial p2, Polynomial model) {
            return Module(Mult(p1, p2), model);
        }

        public static Polynomial ModulePow(Polynomial poly, int power, Polynomial model) {
            if (power % 2 != 0) {
                throw new Exception("Invalid power");
            }
            if (power > 2) {
                return ModuleMult(ModulePow(poly, power - 2, model), ModuleMult(poly, poly, model), model);
            }
            return ModuleMult(poly, poly, model);
        }

        public static Polynomial ModuleAdd(Polynomial p1, Polynomial p2, Polynomial model) {
            return Module(Add(p1, p2), model);
        }

        public static Polynomial Add(Polynomial p1, Polynomial p2) {
            return new CustomPolynomial(p1.Value ^ p2.Value);
        }

        public static int CountBits(ulong num) {
            int count = 0;
            while (num > 0) {
                num >>= 1;
                count++;
            }
            return count;
        }
    }
}