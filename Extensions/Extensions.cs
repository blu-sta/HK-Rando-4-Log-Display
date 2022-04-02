using System;
using System.Text.RegularExpressions;


namespace HK_Rando_4_Log_Display.Extensions
{
    public static class Extensions
    {
        public static string AsObjectName(this string s) =>
            Regex.Replace(s, @"[^\w\d]", "");

        public static string WithoutUnderscores(this string s) =>
            Regex.Replace(s, "_", " ");

    }
}
