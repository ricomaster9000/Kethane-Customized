using System;
using UnityEngine;

namespace Kethane.Particles;

public static class Utils
{
    private static readonly char[] comma = { ',' };

    public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue)) return defaultValue;

        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }

    public static string[] ParseArray(string text)
    {
        var array = text.Split(comma, StringSplitOptions.RemoveEmptyEntries);
        for (var i = array.Length; i-- > 0;) array[i] = array[i].Trim();
        return array;
    }

    public static float[] ParseFloatArray(string text)
    {
        var elements = ParseArray(text);
        var values = new float[elements.Length];
        for (var i = elements.Length; i-- > 0;)
            if (!float.TryParse(elements[i], out values[i]))
                return null;
        return values;
    }

    public static bool ParseBool(ConfigNode node, string name, bool defaultValue)
    {
        if (node.HasValue(name))
        {
            bool val;
            bool.TryParse(node.GetValue(name), out val);
            return val;
        }

        return defaultValue;
    }

    public static float ParseFloat(ConfigNode node, string name, float defaultValue)
    {
        if (node.HasValue(name))
        {
            float val;
            float.TryParse(node.GetValue(name), out val);
            return val;
        }

        return defaultValue;
    }

    public static int ParseInt(ConfigNode node, string name, int defaultValue)
    {
        if (node.HasValue(name))
        {
            int val;
            int.TryParse(node.GetValue(name), out val);
            return val;
        }

        return defaultValue;
    }

    public static LayerMask ParseLayerMask(ConfigNode node, string name, LayerMask defaultValue)
    {
        if (node.HasValue(name))
        {
            int val;
            if (!int.TryParse(node.GetValue(name), out val)) return LayerMask.NameToLayer(node.GetValue(name));
            return val;
        }

        return defaultValue;
    }

    public static Vector2 ParseVector2(ConfigNode node, string name, Vector2 defaultValue)
    {
        if (node.HasValue(name))
        {
            var values = ParseFloatArray(node.GetValue(name));
            return new Vector2(values[0], values[1]);
        }

        return defaultValue;
    }

    public static Vector3 ParseVector3(ConfigNode node, string name, Vector2 defaultValue)
    {
        if (node.HasValue(name))
        {
            var values = ParseFloatArray(node.GetValue(name));
            return new Vector3(values[0], values[1], values[2]);
        }

        return defaultValue;
    }

    public static void CalcAxes(Vector3 norm, out Vector3 X, out Vector3 Y)
    {
        var axis = Vector3.right;
        var n = Mathf.Abs(Vector3.Dot(norm, axis));
        if (Mathf.Abs(Vector3.Dot(norm, Vector3.forward)) < n)
        {
            axis = Vector3.forward;
            n = Mathf.Abs(Vector3.Dot(norm, axis));
        }

        if (Mathf.Abs(Vector3.Dot(norm, Vector3.up)) < n)
        {
            axis = Vector3.up;
            n = Mathf.Abs(Vector3.Dot(norm, axis));
        }

        Y = Vector3.Cross(norm, axis);
        X = Vector3.Cross(Y, norm);
    }
}