using System;
using System.Reflection;

namespace Daybreak.Config
{
    public class Validation
    {
        [AttributeUsage(AttributeTargets.Field)]
        public abstract class BaseValidator : Attribute
        {
            public abstract bool Validate(object val, Type t);
        }

        public class LengthBetween : BaseValidator
        {
            public LengthBetween(int min, int max)
            {
                _min = min;
                _max = max;
            }

            public override bool Validate(object val, Type t)
            {
                if (t != typeof(string)) return false;
                var v = (string)val;
                return v.Length > _min && v.Length < _max;
            }

            private int _min;
            private int _max;
        }

        public class Between : BaseValidator
        {
            public Between(int min, int max)
            {
                _min = min;
                _max = max;
            }

            public override bool Validate(object val, Type t)
            {
                if (!typeof(IComparable).IsAssignableFrom(t)) return false;
                var v = (IComparable) val;
                return v.CompareTo(_min) > 0 && v.CompareTo(_max) < 0;
            }

            private int _min;
            private int _max;
        }
    }
}