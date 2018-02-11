using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mironov.Golomba.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mironov.Crypto.Polynom;
using Mironov.Crypto.Utils;

namespace Mironov.Golomba.Model.Tests
{
    [TestClass()]
    public class PolynomialTests
    {
        [TestMethod()]
        public void ValueTest() {
            Assert.AreEqual(new CustomPolynom("011").Value, (ulong) 3);
            Assert.AreEqual(new CustomPolynom("1001").Value, (ulong) 9);
            Assert.AreEqual(new CustomPolynom("0").Value, (ulong) 0);
            Assert.AreEqual(new CustomPolynom("00001").Value, (ulong) 1);
        }

        [TestMethod()]
        public void DoubleInitTest() {
            Assert.AreEqual(new CustomPolynom(9).Value, (ulong) 9);
            Assert.AreEqual(new CustomPolynom(15).Value, (ulong) 15);
            Assert.AreEqual(new CustomPolynom(252).Value, (ulong) 252);
            Assert.AreEqual(new CustomPolynom(8 ^ 9).Value, (ulong) 1);
        }
    }

    [TestClass()]
    public class PolyUtilsTests
    {
        [TestMethod()]
        public void MultTest() {
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynom("1001011"), new CustomPolynom("1001011")).Value,
                new CustomPolynom("1000001000101").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynom("11001001"), new CustomPolynom("11001001")).Value,
                new CustomPolynom("101000001000001").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynom("11001001"), new CustomPolynom("01000011")).Value,
                new CustomPolynom("11001100011011").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynom("1001100"), new CustomPolynom("1110100")).Value,
                new CustomPolynom("1111101110000").Value);
            Assert.AreEqual(
                PolyUtils.Mult(new CustomPolynom("1110100"), new CustomPolynom("1001100")).Value,
                new CustomPolynom("1111101110000").Value);
        }

        [TestMethod()]
        public void PowTest() {
            Assert.AreEqual(
                PolyUtils.Pow(new CustomPolynom("1001011"), 2).Value,
                new CustomPolynom("1000001000101").Value);
            Assert.AreEqual(
                PolyUtils.Pow(new CustomPolynom("11001001"), 2).Value,
                new CustomPolynom("101000001000001").Value);
        }

        [TestMethod()]
        public void PowMultModuleTest() {
            Assert.AreEqual(
                PolyUtils.ModulePow(new CustomPolynom("1001011"), 2, new CustomPolynom("101011111")).Value,
                new CustomPolynom("11001001").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    PolyUtils.ModulePow(new CustomPolynom("1001011"), 2, new CustomPolynom("101011111")),
                    PolyUtils.ModulePow(new CustomPolynom("1001011"), 2, new CustomPolynom("101011111")),
                    new CustomPolynom("101011111")).Value,
                new CustomPolynom("1000011").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    PolyUtils.ModulePow(new CustomPolynom("1001011"), 4, new CustomPolynom("101011111")),
                    PolyUtils.ModulePow(new CustomPolynom("1001011"), 2, new CustomPolynom("101011111")),
                    new CustomPolynom("101011111")).Value,
                new CustomPolynom("110001").Value);
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
                PolyUtils.Module(new CustomPolynom("101000001000001"), new CustomPolynom("101011111")).Value,
                new CustomPolynom("1000011").Value);
            Assert.AreEqual(
                PolyUtils.Module(new CustomPolynom("1000001000101"), new CustomPolynom("101011111")).Value,
                new CustomPolynom("11001001").Value);
            Assert.AreEqual(
                PolyUtils.Module(new CustomPolynom("01110101110000"), new CustomPolynom("101001101")).Value,
                new CustomPolynom("000011111").Value);
        }

        [TestMethod()]
        public void AddTest()
        {
            Assert.AreEqual(
                PolyUtils.Add(new CustomPolynom("00000001"), new CustomPolynom("00101101")).Value,
                new CustomPolynom("00101100").Value);
            Assert.AreEqual(
                PolyUtils.Add(new CustomPolynom("00010101"), new CustomPolynom("1010")).Value,
                new CustomPolynom("11111").Value);
        }
        [TestMethod()]
        public void ModuleMultTest()
        {
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    new CustomPolynom("01001100"),
                    new CustomPolynom("01001100"),
                    new CustomPolynom("101001101")).Value,
                new CustomPolynom("11111001").Value);
            Assert.AreEqual(
                PolyUtils.ModuleMult(
                    new CustomPolynom("11111001"),
                    new CustomPolynom("01001100"),
                    new CustomPolynom("101001101")).Value,
                new CustomPolynom("10110010").Value);
        }
    }

    [TestClass()]
    public class GaluaPolynomTests
    {
        [TestMethod()]
        public void GaluaPolynomTest() {
            GaluaPolynom galua = new GaluaPolynom(new CustomPolynom("01001100"), new CustomPolynom("101001101"));
            Assert.AreEqual(galua.Value, new CustomPolynom("00000001").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynom("01001100").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynom("11111001").Value);
            galua = galua.Next as GaluaPolynom;
            Assert.AreEqual(galua.Value, new CustomPolynom("10110010").Value);
            int lastNumber = 0;
            while (galua != null && !galua.IsComplete) {
                lastNumber = galua.Number;
                galua = galua.Next as GaluaPolynom;
            }
            Assert.AreEqual(lastNumber, 255);
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
            Assert.AreEqual(new CustomPolynom(1, 8).Size, 8);
            Assert.AreEqual(new CustomPolynom(12, 8).Value, (ulong)12);
            Assert.AreEqual(new CustomPolynom(1).Size, 64);
            Assert.AreEqual(new CustomPolynom("0011").Value, (ulong)3);
            Assert.AreEqual(new CustomPolynom("1111").Hex, "0F");
            Assert.AreEqual(new CustomPolynom(new bool[] {false, true, false, true}).Hex, "05");
        }
    }
}