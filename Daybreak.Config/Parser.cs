using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;

namespace Daybreak.Config
{
    internal class Parser
    {
        private static bool IsValidIdentifier(char c)
        {
            return c == '_' || char.IsLetterOrDigit(c);
        }

        internal struct ConfigField
        {
            public int Position;
            public int Length;
            public FieldInfo FieldInfo;
            public CfgField FieldAttr;
            public List<ConfigField> SubFields;
            public bool IsSection;
            public List<Validation.BaseValidator> Validators;
            public int Id => FieldInfo.GetHashCode();
        }

        internal static List<ConfigField> ParseFields(Type t, bool nested = false)
        {
            var fields = new List<ConfigField>();
            if (t == typeof(BaseConfig))
            {
                throw new InvalidCastException("What are you even doing");
            }

            foreach (var field in t.GetFields())
            {
                CfgField attr;
                try
                {
                    attr = (CfgField) field.GetCustomAttributes(true).First(x =>
                        x.GetType() == typeof(CfgField) || x.GetType() == typeof(CfgSection));
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                if (attr.FieldName is null)
                {
                    attr.FieldName = field.Name.Underscore();
                }

                var cField = new ConfigField
                {
                    Position = -1,
                    Length = -1,
                    FieldInfo = field,
                    FieldAttr = attr,
                    SubFields = null,
                    IsSection = attr.GetType() == typeof(CfgSection),
                    Validators = field.GetCustomAttributes(true)
                        .Where(x => x.GetType().IsSubclassOf(typeof(Validation.BaseValidator)))
                        .Cast<Validation.BaseValidator>().ToList()
                };

                if (cField.IsSection)
                {
                    if (nested)
                    {
                        throw new Exceptions.TooManyNestedException();
                    }

                    cField.SubFields = ParseFields(field.FieldType, true);
                }

                fields.Add(cField);
            }

            return fields;
        }

        internal static bool ParseConfig<T>(T inst, string config, string filename = "[string]") where T : BaseConfig
        {
            var t = (BaseConfig) inst;
            t.Config = config;

            var nullField = new ConfigField();

            var rootSection = true;
            var stringBuf = new List<char>();
            var currentField = nullField;
            var section = nullField;
            var hasField = false;
            var updatedFields = new List<ConfigField>();
            var i = 0;
            while (i < t.Config.Length)
            {
                while (char.IsWhiteSpace(t.Config[i]) || t.Config[i] == '\n')
                {
                    i++;
                }

                if (i == t.Config.Length) break;

                if (t.Config[i] == '/' && t.Config[i + 1] == '/')
                {
                    i += 2;
                    while (t.Config[i] != '\n' && i < t.Config.Length)
                    {
                        i++;
                    }

                    continue;
                }

                if (t.Config[i] == '[')
                {
                    i++;
                    stringBuf.Clear();
                    while (true)
                    {
                        if (t.Config[i] == ']') break;
                        if (!IsValidIdentifier(t.Config[i]))
                            throw new Exceptions.InvalidTokenException(filename, t.Config, i, t.Config[i]);
                        stringBuf.Add(t.Config[i]);
                        i++;
                    }

                    i++;

                    var sectionName = new string(stringBuf.ToArray());
                    try
                    {
                        section = t._fields.First(x =>
                            x.IsSection && x.FieldAttr.FieldName == sectionName);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new Exceptions.InvalidSectionException(filename, t.Config, i - sectionName.Length - 1,
                            sectionName);
                    }


                    rootSection = false;
                }

                while (char.IsWhiteSpace(t.Config[i]))
                {
                    i++;
                }

                if (!hasField)
                {
                    if (!rootSection && section.Position == -1)
                    {
                        section.Position = i;
                        updatedFields.Add(section);
                    }

                    stringBuf.Clear();
                    while (!char.IsWhiteSpace(t.Config[i]) && t.Config[i] != '=')
                    {
                        if (!IsValidIdentifier(t.Config[i]))
                        {
                            throw new Exceptions.InvalidTokenException(filename, t.Config, i, t.Config[i]);
                        }

                        stringBuf.Add(t.Config[i]);
                        i++;
                    }

                    var fieldName = new string(stringBuf.ToArray());
                    try
                    {
                        if (rootSection)
                        {
                            currentField = t._fields.First(x => x.FieldAttr.FieldName == fieldName);
                        }
                        else
                        {
                            currentField = section.SubFields.First(x => x.FieldAttr.FieldName == fieldName);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        throw new Exceptions.InvalidFieldException(fieldName);
                    }

                    while (char.IsWhiteSpace(t.Config[i]))
                    {
                        i++;
                    }

                    if (t.Config[i] != '=')
                    {
                        throw new Exceptions.InvalidTokenException(filename, t.Config, i, t.Config[i]);
                    }

                    i++;
                    while (char.IsWhiteSpace(t.Config[i]))
                    {
                        i++;
                    }

                    hasField = true;
                }
                else
                {
                    stringBuf.Clear();
                    currentField.Position = i;
                    if (t.Config[i] == '"')
                    {
                        i++;
                        while (t.Config[i] != '"')
                        {
                            if (currentField.FieldInfo.FieldType != typeof(string))
                            {
                                throw new InvalidCastException("Don't quote non string fields");
                            }

                            if (t.Config[i] == '\n')
                            {
                                throw new Exceptions.InvalidTokenException(filename, t.Config, i, '\n');
                            }

                            if (i >= t.Config.Length)
                            {
                                throw new Exception(); // TODO: Write exception type for unexpected EOF
                            }

                            stringBuf.Add(t.Config[i]);
                            i++;
                        }
                    }
                    else
                    {
                        while (i < t.Config.Length && !char.IsWhiteSpace(t.Config[i]) && t.Config[i] != '\n')
                        {
                            stringBuf.Add(t.Config[i]);
                            i++;
                        }
                    }


                    currentField.Length = stringBuf.Count;

                    var fieldVal = new string(stringBuf.ToArray());
                    object val = null;
                    try
                    {
                        val = Conversion.ConvertTo(currentField.FieldInfo.FieldType, fieldVal);
                        if (currentField.Validators.Any(validator => !validator.Validate(val, currentField.FieldInfo.FieldType)))
                        {
                            throw new Exceptions.InternalInvalidValueException(val.GetType(), $"{val}");
                        }
                    }
                    catch (Exceptions.InternalInvalidValueException e)
                    {
                        throw new Exceptions.InvalidValueException(filename, t.Config, i - fieldVal.Length, e.Message);
                    }

                    currentField.FieldInfo.SetValue(rootSection ? t : section.FieldInfo.GetValue(t), val);
                    updatedFields.Add(currentField);

                    i++;
                    hasField = false;
                }
            }

            foreach (var field in updatedFields)
            {
                for (var x = 0; x < t._fields.Count; x++)
                {
                    if (field.Id == t._fields[x].Id)
                    {
                        t._fields[x] = field;
                        goto NEXTFIELD;
                    }

                    if (t._fields[x].IsSection)
                    {
                        for (var y = 0; y < t._fields[x].SubFields.Count; y++)
                        {
                            if (field.Id == t._fields[x].SubFields[y].Id)
                            {
                                t._fields[x].SubFields[y] = field;
                                goto NEXTFIELD;
                            }
                        }
                    }
                }

                NEXTFIELD: ;
            }

            return true;
        }
    }
}