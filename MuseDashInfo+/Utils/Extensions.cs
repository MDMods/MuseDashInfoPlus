using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDIP.Utils;

public static class Extensions
{
    public static string Color(this string text, string color) => $"<color={color}>{text}</color>";
}
