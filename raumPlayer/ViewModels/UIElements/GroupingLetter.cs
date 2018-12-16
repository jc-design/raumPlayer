using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace roomZone.Models
{
    static class ElementGrouping
    {
        public static string GetGroupKey(this string title)
        {
            if ((title?.Length ?? 0) >= 1)
            {
                string k = title.Substring(0, 1);

                if (Regex.IsMatch(k, @"^[a-zA-Z]+$")) { return k.ToUpper(); }
                else if (Regex.IsMatch(k, @"^[0-9]+$")) { return "~"; }
                else return "?";
            }
            else { return "?"; }
        }
    }
}
