using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuffixTreeSharp.Test
{
    public static class Utils
    {
        /**
         * Normalize an input string
         *
         * @param input the input string to normalize
         * @return <tt>input</tt> all lower-case, withoutput any non alphanumeric character
         */
        public static string Normalize(this string input)
        {
            var output = new StringBuilder();
            var l = input.ToLower();
            foreach (var c in l.Where(c => c >= 'a' && c <= 'z' || c >= '0' && c <= '9'))
            {
                output.Append(c);
            }
            return output.ToString();
        }

        /**
         * Computes the set of all the substrings contained within the <tt>str</tt>
         *
         * It is fairly inefficient, but it is used just in tests ;)
         * @param str the string to compute substrings of
         * @return the set of all possible substrings of str
         */
        public static HashSet<string> GetSubstrings(this string str)
        {
            var ret = new HashSet<string>();
            // compute all substrings
            for (var len = 1; len <= str.Length; ++len)
            {
                for (var start = 0; start + len <= str.Length; ++start)
                {
                    var itstr = str.Substring(start, len);
                    ret.Add(itstr);
                }
            }

            return ret;
        }
    }
}
