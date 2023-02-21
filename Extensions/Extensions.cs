using System.Text;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.Extensions
{
    public static class Extensions
    {
        public static string AsObjectName(this string s) =>
            "_" + Regex.Replace(s, @"[^\w]", "");

        public static string WithoutUnderscores(this string s) =>
            Regex.Replace(s, "_", " ");

    }
}
