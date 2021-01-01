using System;

namespace Daybreak.Config
{
    public class Exceptions
    {
        public class ConfigException : Exception
        {
            public ConfigException(string filename, string doc, int pos, string message) : base(message)
            {
                _filename = filename;
                for (var i = 0; i < pos; i++)
                {
                    _col++;

                    if (doc[i] == '\n')
                    {
                        _line += 1;
                        _col = 0;
                    }
                }

                var x = pos - _col;
                while (true)
                {
                    _context += doc[x];
                    x++;
                    if (x >= doc.Length || doc[x] == '\n')
                    {
                        break;
                    }
                }
            }

            public override string StackTrace =>
                $@"  at {_filename}:{_line}:{_col}
  {_context}
  {new string(' ', _col)}^ Here";

            private int _line;
            private string _context;
            private int _col;
            private string _filename;
        }

        public class InvalidTokenException : ConfigException
        {
            public InvalidTokenException(string filename, string doc, int pos, char c) : base(filename, doc, pos,
                $"Unexpected character '{c}'")
            {
            }
        }

        public class InvalidSectionException : ConfigException
        {
            public InvalidSectionException(string filename, string doc, int pos, string name) : base(filename, doc, pos,
                $"Unexpected section \"{name}\"")
            {
            }
        }

        public class InvalidFieldException : Exception
        {
            public InvalidFieldException(string name) : base($"Unexpected field \"{name}\"")
            {
            }
        }

        public class TooManyNestedException : Exception
        {
            public TooManyNestedException() : base()
            {
            }
        }

        internal class InternalInvalidValueException : Exception
        {
            public InternalInvalidValueException(Type type, string val) : base(
                $"Invald value for {type.Name}: \"{val}\"")
            {
            }
        }

        public class InvalidValueException : ConfigException
        {
            public InvalidValueException(string filename, string doc, int pos, string msg) : base(filename, doc, pos,
                msg)
            {
            }
        }
    }
}