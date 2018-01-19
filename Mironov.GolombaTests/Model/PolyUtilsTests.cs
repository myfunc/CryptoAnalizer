using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mironov.Golomba.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Golomba.Model.Tests
{
    [TestClass()]
    public class PolynomialTests
    {
        [TestMethod()]
        public void ValueTest() {
            Assert.AreEqual(new CustomPolynomial("011").Value, (ulong) 3);
            Assert.AreEqual(new CustomPolynomial("1001").Value, (ulong) 9);
            Assert.AreEqual(new CustomPolynomial("0").Value, (ulong) 0);
            Assert.AreEqual(new CustomPolynomial("00001").Value, (ulong) 1);
        }

        [TestMethod()]
        public void DoubleInitTest() {
            Assert.AreEqual(new CustomPolynomial(9).Value, (ulong) 9);
            Assert.AreEqual(new CustomPolynomial(15).Value, (ulong) 15);
            Assert.AreEqual(new CustomPolynomial(252).Value, (ulong) 252);
            Assert.AreEqual(new CustomPolynomial(8 ^ 9).Value, (ulong) 1);
        }
    }

    [TestClass()]
    public class PolyUtilsTests
    {
        [TestMethod()]
        public void MultTest() {
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynomial("1001011"), new CustomPolynomial("1001011")).Value,
                new CustomPolynomial("1000001000101").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynomial("11001001"), new CustomPolynomial("11001001")).Value,
                new CustomPolynomial("101000001000001").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynomial("11001001"), new CustomPolynomial("01000011")).Value,
                new CustomPolynomial("11001100011011").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynomial("1001100"), new CustomPolynomial("1110100")).Value,
                new CustomPolynomial("1111101110000").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynomial("1110100"), new CustomPolynomial("1001100")).Value,
                new CustomPolynomial("1111101110000").Value);
        }

        [TestMethod()]
        public void PowTest() {
            Assert.AreEqual(
                PolyUtils.Pow(new CustomPolynomial("1001011"), 2).Value,
                new CustomPolynomial("1000001000101").Value);
            Assert.AreEqual(
                PolyUtils.Pow(new CustomPolynomial("11001001"), 2).Value,
                new CustomPolynomial("101000001000001").Value);
        }

        [TestMethod()]
        public void PowMultModuleTest() {
            Assert.AreEqual(
                PolyUtils.ModulePow(new CustomPolynomial("1001011"), 2, new CustomPolynomial("101011111")).Value,
                new CustomPolynomial("11001001").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    PolyUtils.ModulePow(new CustomPolynomial("1001011"), 2, new CustomPolynomial("101011111")),
                    PolyUtils.ModulePow(new CustomPolynomial("1001011"), 2, new CustomPolynomial("101011111")),
                    new CustomPolynomial("101011111")).Value,
                new CustomPolynomial("1000011").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    PolyUtils.ModulePow(new CustomPolynomial("1001011"), 4, new CustomPolynomial("101011111")),
                    PolyUtils.ModulePow(new CustomPolynomial("1001011"), 2, new CustomPolynomial("101011111")),
                    new CustomPolynomial("101011111")).Value,
                new CustomPolynomial("110001").Value);
        }

        [TestMethod()]
        public void CountBitsTest() {
            Assert.AreEqual(PolyUtils.CountBits(255), 8);
            Assert.AreEqual(PolyUtils.CountBits(127), 7);
            Assert.AreEqual(PolyUtils.CountBits(1), 1);
            Assert.AreEqual(PolyUtils.CountBits(15), 4);
            Assert.AreEqual(PolyUtils.CountBits(16), 5);
            Assert.AreEqual(PolyUtils.CountBits(260), 9);
        }

        [TestMethod()]
        public void ModuleTest() {
            Assert.AreEqual(
                PolyUtils.Module(new CustomPolynomial("101000001000001"), new CustomPolynomial("101011111")).Value,
                new CustomPolynomial("1000011").Value);
            Assert.AreEqual(
                PolyUtils.Module(new CustomPolynomial("1000001000101"), new CustomPolynomial("101011111")).Value,
                new CustomPolynomial("11001001").Value);
            Assert.AreEqual(
                PolyUtils.Module(new CustomPolynomial("01110101110000"), new CustomPolynomial("101001101")).Value,
                new CustomPolynomial("000011111").Value);
        }

        [TestMethod()]
        public void AddTest()
        {
            Assert.AreEqual(
                PolyUtils.Add(new CustomPolynomial("00000001"), new CustomPolynomial("00101101")).Value,
                new CustomPolynomial("00101100").Value);
            Assert.AreEqual(
                PolyUtils.Add(new CustomPolynomial("00010101"), new CustomPolynomial("1010")).Value,
                new CustomPolynomial("11111").Value);
        }
        [TestMethod()]
        public void ModuleMultTest()
        {
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    new CustomPolynomial("01001100"),
                    new CustomPolynomial("01001100"),
                    new CustomPolynomial("101001101")).Value,
                new CustomPolynomial("11111001").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    new CustomPolynomial("11111001"),
                    new CustomPolynomial("01001100"),
                    new CustomPolynomial("101001101")).Value,
                new CustomPolynomial("10110010").Value);
        }
    }

    [TestClass()]
    public class GaluaPolynomTests
    {
        [TestMethod()]
        public void GaluaPolynomTest() {
            Test1();
        }

        protected void Test1() {
            GaluaPolynom galua = new GaluaPolynom(new CustomPolynomial("01001100"), new CustomPolynomial("101001101"));
            Assert.AreEqual(galua.Value, new CustomPolynomial("00000001").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynomial("01001100").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynomial("11111001").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynomial("10110010").Value);
            while (!galua.IsComplete) {
                galua = galua.Next as GaluaPolynom;
            }
            Assert.AreEqual(galua.Value, (ulong)1);
            Assert.AreEqual(galua.Number, 255);
        }
    }

    [TestClass()]
    public class CustomPolynomTests
    {
        [TestMethod()]
        public void CustomPolynomTest()
        {
            Test1();
        }

        protected void Test1()
        {
            Assert.AreEqual(new CustomPolynomial(1, 8).Size, 8);
            Assert.AreEqual(new CustomPolynomial(12, 8).Value, (ulong)12);
            Assert.AreEqual(new CustomPolynomial(1).Size, 64);
            Assert.AreEqual(new CustomPolynomial("0011").Value, (ulong)3);
            Assert.AreEqual(new CustomPolynomial("1111").Hex, "0F");
            Assert.AreEqual(new CustomPolynomial(new bool[] {false, true, false, true}).Hex, "05");
        }
    }
}