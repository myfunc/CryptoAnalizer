using System.Collections.Generic;

namespace Mironov.Golomba.Model
{
    public class FormatUnit
    {
        public List<Polynomial> Polynoms { get; set; }

        public void Generate(int wordLength, int distance) {
            Polynoms = new List<Polynomial>();
            Polynomial firstPoly = new FormatPolynom(wordLength, distance);
            for (Polynomial poly = firstPoly; poly != null; poly = poly.Next) {
                Polynoms.Add(poly);
            }
        }
    }
}