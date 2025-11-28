using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace eP.Text
{
    public static class StringBuilderExt
    {
        /// <summary>
        ///  Construct property string with xml format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringBuilder"></param>
        /// <param name="indentLevel"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public static void ConstructPropertyString<T>(this StringBuilder stringBuilder, int indentLevel, string propertyName, T propertyValue, string additionalPropertyStr = "")
        {
            string indendStr = "".PadLeft(indentLevel << 2, ' ');
            string formatedAttrStr = additionalPropertyStr.Length > 0 ? $" {additionalPropertyStr}" : "";
            switch (propertyValue)
            {
                case Vector2 vec:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> ({vec.X,8:0.000}, {vec.Y,8:0.000}) </{propertyName}>");
                    break;
                case Vector3 vec:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> ({vec.X,8:0.000}, {vec.Y,8:0.000}, {vec.Z,8:0.000}) </{propertyName}>");
                    break;
                case Vector4 vec:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> ({vec.X,8:0.000}, {vec.Y,8:0.000}, {vec.Z,8:0.000}, {vec.W,8:0.000}) </{propertyName}>");
                    break;
                case Matrix4x4 mat:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}>");
                    for (int i = 0; i < 4; i++)
                    {
                        stringBuilder.AppendLine($"{indendStr}    {mat[i, 0],8:0.000}, {mat[i, 1],8:0.000}, {mat[i, 2],8:0.000}, {mat[i, 3],8:0.000}");
                    }
                    stringBuilder.AppendLine($"{indendStr}</{propertyName}>");
                    break;
                case sbyte integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x2}) </{propertyName}>");
                    break;
                case short integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x4}) </{propertyName}>");
                    break;
                case int integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x8}) </{propertyName}>");
                    break;
                case long integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x16}) </{propertyName}>");
                    break;
                case byte integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x2}) </{propertyName}>");
                    break;
                case ushort integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x4}) </{propertyName}>");
                    break;
                case uint integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x8}) </{propertyName}>");
                    break;
                case ulong integer:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {integer}({integer:x16}) </{propertyName}>");
                    break;
                case bool boolean:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {(boolean ? "True" : "False")} </{propertyName}>");
                    break;
                case Array array:
                    int index = 0;
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}>");
                    foreach (object? obj in array)
                    {
                        ConstructPropertyString(stringBuilder, indentLevel + 2, $"ArrayElement", obj, $"index=\"{index:000}\"");
                        index++;
                    }
                    stringBuilder.AppendLine($"{indendStr}</{propertyName}>");
                    stringBuilder.AppendLine("");
                    break;
                case Enum _enum:
                    stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> {_enum.GetType().Name}.{Enum.GetName(_enum.GetType(), _enum) ?? "Unknown"} </{propertyName}>");
                    break;
                default:
                    if (propertyValue is null)
                    {
                        stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}> (NULL) </{propertyName}>");
                    }
                    else
                    {
                        string strObj = (propertyValue.ToString() ?? "").TrimEnd('\r', '\n');
                        string[] lines = strObj.Split(Environment.NewLine);
                        stringBuilder.AppendLine($"{indendStr}<{propertyName}{formatedAttrStr}>");
                        foreach (string line in lines)
                            stringBuilder.AppendLine($"{indendStr}    {line}");
                        stringBuilder.AppendLine($"{indendStr}</{propertyName}>");
                    }
                    break;
            }
        }
    }
}
