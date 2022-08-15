using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringFormatter
{
    public static string FormatKeyboardName(string name)
    {
        return name.Replace("-", "");
    }
}
