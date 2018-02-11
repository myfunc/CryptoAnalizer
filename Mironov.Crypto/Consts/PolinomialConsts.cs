using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Crypto.Consts
{
    public static class PolinomialConsts
    {
        public static List<string> GetPolynoms(int length) {
            return _irredPolynoms[length];
        }

        public static List<int> GetAvailableLengthList() {
            return _irredPolynoms.Keys.ToList();
        }
        private static Dictionary<int, List<string>> _irredPolynoms = new Dictionary<int, List<string>>() {
            {4, new List<string>() {
                "10011",
                "11001",
                "11111"
            }},
            {5, new List<string>() {
                "100101",
                "101001",
                "101111",
                "110111",
                "111011",
                "111101"
            }},
            {6, new List<string>() {
                "1000011",
                "1001001",
                "1010111",
                "1011011",
                "1100001",
                "1100111",
                "1101101",
                "1110011",
                "1110101"
            }},
            {7, new List<string>() {
                "10000011",
                "10001001",
                "10001111",
                "10010001",
                "10011101",
                "10100111",
                "10101011",
                "10111001",
                "10111111",
                "11000001",
                "11001011",
                "11010011",
                "11010101",
                "11100101",
                "11101111",
                "11110001",
                "11110111",
                "11111101"
            }},
            {8, new List<string>() {
                "100011011",
                "100011101",
                "100101011",
                "100101101",
                "100111001",
                "100111111",
                "101001101",
                "101011111",
                "101100011",
                "101100101",
                "101101001",
                "101110001",
                "101110111",
                "101111011",
                "110000111",
                "110001011",
                "110001101",
                "110011111",
                "110100011",
                "110101001",
                "110110001",
                "110111101",
                "111000011",
                "111001111",
                "111010111",
                "111011101",
                "111100111",
                "111110011",
                "111110101",
                "111111001"
            }}
        };
    }
}
