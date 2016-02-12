using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayGame
{
    public static class Extensions
    {
        public static string FormatBy(this string input, params string[] values)
        {
            // because string.Format("{0}", "hello") feels more unnatural than "{0}".FormatBy("hello").
            // c#6 introduces string interpolation ($"{variablename}"), so that's nice for
            // the future, I guess.
            return string.Format(input, values);
        }
        public static T Pick<T>(this Random rand, IList<T> Values)
        {
            // pick a random object and return it.
            return Values[rand.Next(Values.Count())];
        }

        public static T RandomWeighted<T>(this Random rand, Dictionary<T, int> Values)
        {
            // first, check if it adds up to 100.
            int total = 0;
            foreach (KeyValuePair<T, int> v in Values)
                total += v.Value;

            // if not, throw an exception.
            if (total != 100)
                throw new ArgumentOutOfRangeException("Values: total do not add up to 100.");

            // generate a random number between 0 and 99.
            int r = rand.Next(100);
            total = 0;

            // add by each value until we reach what's wanted.
            foreach (KeyValuePair<T, int> v in Values)
            {
                total += v.Value;

                if (r <= total)
                    return v.Key;
            }
            
            // shouldn't reach here, but eh, failsafes. Plus the compiler requires this.
            // I suppose I should throw an exception in the event it does get here?
            return Values.First().Key;
        }

        // because there isn't a numerical constraint, IComparable will have to do.
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}
