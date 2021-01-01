using System;
using System.Collections.Generic;

namespace Daybreak.Config
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CfgField : Attribute
    {
        public string FieldName { get; internal set; }

        public CfgField()
        {
        }

        public CfgField(string name)
        {
            FieldName = name;
        }
    }

    public class CfgSection : CfgField
    {
        public CfgSection() : base()
        {
        }

        public CfgSection(string name) : base(name)
        {
        }
    }


    public class BaseConfig
    {
        protected BaseConfig()
        {
            _fields = Parser.ParseFields(GetType());
        }

        public bool LoadFromFile(string filename, bool optional = false)
        {
            string content;
            try
            {
                content = System.IO.File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                if (optional) return false;
                throw;
            }

            return Parser.ParseConfig(this, content);
        }

        public bool LoadFromString(string content)
        {
            return Parser.ParseConfig(this, content);
        }

        internal string Config;
        internal List<Parser.ConfigField> _fields;
    }
}