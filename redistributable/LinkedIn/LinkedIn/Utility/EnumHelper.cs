using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace LinkedIn.Utility
{
  /// <summary>
  /// A helper class for enums.
  /// </summary>
  public static class EnumHelper
  {
    /// <typeparam name="TValue">usually int</typeparam>
    public static List<TValue> GetValues<TEnum, TValue>()
    {
      List<TValue> values = new List<TValue>();
      Array array = Enum.GetValues(typeof(TEnum));
      foreach (TValue item in array)
      {
        values.Add(item);
      }

      return values;
    }

    /// <summary>
    /// Get the description of a <see cref="Enum" /> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A description of the <see cref="Enum" /> value.</returns>
    public static string GetDescription(Enum value)
    {
      FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
      DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fieldInfo.GetCustomAttributes(
            typeof(DescriptionAttribute), false);

      return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="enumeratedType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool HasFlag<TEnum>(this TEnum enumeratedType, TEnum value)
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
      if ((enumeratedType is Enum) == false)
      {
        throw new InvalidOperationException("Struct is not an Enum.");
      }

      if (typeof(TEnum).GetCustomAttributes(
          typeof(FlagsAttribute), false).Length == 0)
      {
        throw new InvalidOperationException("Enum must use [Flags].");
      }

      long enumValue = enumeratedType.ToInt64(CultureInfo.InvariantCulture);
      long flagValue = value.ToInt64(CultureInfo.InvariantCulture);

      if ((enumValue & flagValue) == flagValue)
      {
        return true;
      }

      return false;
    }
  }
}