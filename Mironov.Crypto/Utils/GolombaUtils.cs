using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mironov.Crypto.Utils
{
    public class AnalisisResult
    {
        public string Description { get; set; }
        public string Result { get; set; }
    }
    public class GolombaBitInfo
    {
        public bool Bit { get; set; }
        public int Count { get; set; }
    }

    public class GolombaRangesInfo
    {
        public int Number { get; set; }
        public int Range0 { get; set; }
        public int Range1 { get; set; }
    }

    public class GolombaBitCount
    {
        public int Bits0 { get; set; }
        public int Bits1 { get; set; }
    }

    public class GolombaUtils
    {
        public static GolombaBitCount GetBitCount(bool[] arr) {
            return new GolombaBitCount() {
                Bits0 = arr.Where(p => !p).Count(),
                Bits1 = arr.Where(p => p).Count()
            };
        }

        public static List<GolombaBitInfo> GetPostulat1(bool[] arr) {
            bool current = arr[0];
            var result = new List<GolombaBitInfo>();
            int counter = 1;
            for (int i = 1; i < arr.Length; i++) {
                if (current != arr[i]) {
                    result.Add(new GolombaBitInfo() {
                        Bit = current,
                        Count = counter
                    });
                    counter = 1;
                }
                else {
                    counter++;
                }
                current = arr[i];
            }
            result.Add(new GolombaBitInfo()
            {
                Bit = current,
                Count = counter
            });
            return result;
        }

        public static List<GolombaRangesInfo> GetPostulat2(List<GolombaBitInfo> input) {
            List<GolombaRangesInfo> result = new List<GolombaRangesInfo>();
            for (int i = 1; i < 10; i++)
            {
                var lres = input.Where(p => p.Count == i).ToList();
                result.Add(new GolombaRangesInfo() {
                    Number = i,
                    Range0 = lres.Count(p => !p.Bit),
                    Range1 = lres.Count(p => p.Bit)
                });
            }
            return result;
        }
    }
}