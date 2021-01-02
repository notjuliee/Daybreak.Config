using System;
using System.Collections.Generic;

namespace Daybreak.Config
{
    public class Conversion
    {
        public interface IConverter
        {
            object Convert(string value);
            string UnConvert(object value);
        }

        public static void RegisterConverter<T>(IConverter converter)
        {
            _converters.Add(typeof(T).GetHashCode(), converter);
        }

        internal static object ConvertTo(Type t, string value)
        {
            return _converters[t.GetHashCode()].Convert(value);
        }

        internal static string ConvertFrom(Type t, object value)
        {
            return _converters[t.GetHashCode()].UnConvert(value);
        }

        private class StringConverter : IConverter
        {
            public object Convert(string value)
            {
                return value;
            }

            public string UnConvert(object value)
            {
                return (string) value;
            }
        }

        private class IntConverter : IConverter
        {
            public object Convert(string value)
            {
                return int.Parse(value);
            }

            public string UnConvert(object value)
            {
                return ((int) value).ToString();
            }
        }

        private class BoolConverter : IConverter
        {
            public object Convert(string value)
            {
                switch (value)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    default:
                        throw new Exceptions.InternalInvalidValueException(typeof(bool), value);
                }
            }

            public string UnConvert(object value)
            {
                return (bool) value ? "true" : "false";
            }
        }

        private static Dictionary<int, IConverter> _converters = new Dictionary<int, IConverter>
        {
            {typeof(string).GetHashCode(), new StringConverter()},
            {typeof(int).GetHashCode(), new IntConverter()},
            {typeof(bool).GetHashCode(), new BoolConverter()}
        };
    }
}