using System.Text;
using System.Text.RegularExpressions;


namespace HK_Rando_4_Log_Display.Extensions
{
    public static class Extensions
    {
        public static string AsObjectName(this string s) =>
            Regex.Replace(s, @"[^\w\d]", "");

        public static string WithoutUnderscores(this string s) =>
            Regex.Replace(s, "_", " ");

        public static string AddSpacesBeforeCapitals(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
