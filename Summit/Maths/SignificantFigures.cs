using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Maths;

public static class SignificantFigures
{
    public static double RoundToSignificantFigures(double num, int n)
    {
        if (num == 0) return 0;
        double d = Math.Ceiling(Math.Log10(num < 0 ? -num : num));
        int power = n - (int)d;
        double magnitude = Math.Pow(10, power);
        double shifted = Math.Round(num * magnitude);
        return shifted / magnitude;
    }

    public static double RoundSigFigDecimals(double num, int n)
    {
        if (num % 1 == 0) return num;

        return Math.Round(num, n);
    }

    public static double Round(double num, int n, bool decimalsOnly = true)
    {
        return decimalsOnly ? RoundSigFigDecimals(num, n) : RoundToSignificantFigures(num, n);
    }

    public static string ToString(this double value, int significantFigures, bool decimalsOnly = true)
    {
        return Round(value, significantFigures, decimalsOnly).ToString();
    }

    public static string ToString(this float value, int significantFigures, bool decimalsOnly = true)
    {
        return Round(value, significantFigures, decimalsOnly).ToString();
    }

    public static string ToString(this float? value, int sigFig, bool decimalsOnly = true)
    {
        if (!value.HasValue) return "ERR";

        return value.Value.ToString(sigFig, decimalsOnly);
    }
}
