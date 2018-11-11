using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SexiLove.Extensions.Helpers
{

    public static class Invertor
    {

        public static bool IsRussian(this string text)
        {
            var rx = new Regex("[А-Яа-я]+");
            return rx.Matches(text).Count > 0;
        }

        public static bool IsEnglish(this string text)
        {
            var rx = new Regex("[A-Za-z]+");
            return rx.Matches(text).Count > 0;
        }

        private static Dictionary<string, string> toRus = new Dictionary<string, string>(); 
        private static Dictionary<string, string> toEng = new Dictionary<string, string>(); 

        public static string ToRussian(this string text)
        {
            string output = toRus.Aggregate(text, (current, key) => current.Replace(key.Key, key.Value));
            return output.Replace("__", "_").Replace("__", "_").Trim(new[] { ' ', '_' });
        }

        static Invertor()
        {
            toRus.Add("Q", "Й");
            toRus.Add("W", "Ц");
            toRus.Add("E", "У");
            toRus.Add("R", "К");
            toRus.Add("T", "Е");
            toRus.Add("Y", "Н");
            toRus.Add("U", "Г");
            toRus.Add("I", "Ш");
            toRus.Add("O", "Щ");
            toRus.Add("P", "З");
            toRus.Add("{", "Х");
            toRus.Add("}", "Ъ");
            toRus.Add("A", "Ф");
            toRus.Add("S", "Ы");
            toRus.Add("D", "В");
            toRus.Add("F", "А");
            toRus.Add("G", "П");
            toRus.Add("H", "Р");
            toRus.Add("J", "О");
            toRus.Add("K", "Л");
            toRus.Add("L", "Д");
            toRus.Add(":", "Ж");
            toRus.Add("\"", "Э");
            toRus.Add("Z", "Я");
            toRus.Add("X", "Ч");
            toRus.Add("C", "С");
            toRus.Add("V", "М");
            toRus.Add("B", "И");
            toRus.Add("N", "Т");
            toRus.Add("M", "Ь");
            toRus.Add("<", "Б");
            toRus.Add(">", "Ю");
            toRus.Add("?", ",");

            toRus.Add("q", "й");
            toRus.Add("w", "ц");
            toRus.Add("e", "у");
            toRus.Add("r", "к");
            toRus.Add("t", "е");
            toRus.Add("y", "н");
            toRus.Add("u", "г");
            toRus.Add("i", "ш");
            toRus.Add("o", "щ");
            toRus.Add("p", "з");
            toRus.Add("[", "х");
            toRus.Add("]", "ъ");
            toRus.Add("a", "ф");
            toRus.Add("s", "ы");
            toRus.Add("d", "в");
            toRus.Add("f", "а");
            toRus.Add("g", "п");
            toRus.Add("h", "р");
            toRus.Add("j", "о");
            toRus.Add("k", "л");
            toRus.Add("l", "д");
            toRus.Add(";", "ж");
            toRus.Add("'", "э");
            toRus.Add("z", "я");
            toRus.Add("x", "ч");
            toRus.Add("c", "с");
            toRus.Add("v", "м");
            toRus.Add("b", "и");
            toRus.Add("n", "т");
            toRus.Add("m", "ь");
            toRus.Add(",", "б");
            toRus.Add(".", "ю");
            toRus.Add("/", ".");

        }
    }
}