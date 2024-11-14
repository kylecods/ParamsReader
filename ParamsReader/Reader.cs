using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ParamsReader
{
    public class Reader 
    {
        private int _position;
        private readonly string _text;
        private int _start;
        private int _parsePostion;

        private string _value;

        private TokenTypes _tokenType;

        private readonly List<DataType> _tokens = [];

        private static StringBuilder _builder = new();

        public Reader(string text)
        {
            _start = _position;
            _text = text;
        }

        public char Current => Peek(0);

        public DataType ParseCurrent => ParsePeek(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataType ParsePeek(int offset)
        {
            var pos = _parsePostion + offset;

            if ((uint)pos >= (uint)_tokens.Count)
            {
                return _tokens[_tokens.Count + 1];
            }

            return _tokens[pos];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataType Advance()
        {
            var current = ParseCurrent;
            _parsePostion++;

            return current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataType Match(TokenTypes type)
        {
            if(ParseCurrent.type == type)
            {
                return Advance();
            }
            throw new InvalidDataException("Unexpected value");
        }

        public char Peek(int offset)
        {
            var pos = _position + offset;

            if ((uint)pos < (uint)_text.Length)
            {
                return _text[pos];
            }

            return '\0';
        }

        public DataType Identify()
        {
            switch (Current)
            {
                case '\0':
                    _tokenType = TokenTypes.Eos;
                    break;
                case '&':
                    _position++;
                    _value = null;
                    _tokenType = TokenTypes.Ampersand;
                    break;
                case '=':
                    _position++;
                    if (char.IsLetterOrDigit(Current))
                    {
                        while (char.IsLetterOrDigit(Current))
                        {
                            _position++;
                        }

                        var length = _position - _start;

                        var text = _text.Substring(_start, length);

                        _value = text;
                        _tokenType = TokenTypes.Value;
                        _start = _position + 1;
                    }
               
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        while (char.IsLetter(Current))
                        {
                            _position++;
                        }

                        var length = _position - _start;

                        var text = _text.Substring(_start, length);

                        _value = text;
                        _tokenType = TokenTypes.Key;

                        //we set the new start position
                        _start = _position + 1;
                    }
                    break;
            }
            return new DataType(_tokenType, _value);
        }

        public string Parse()
        {
            DataType token;

            _builder.Clear();

            //Read
            do
            {
                token = Identify();

                if (token.type != TokenTypes.Ampersand)
                {
                    _tokens.Add(token);
                }

            } while (token.type != TokenTypes.Eos);


            while(ParseCurrent.type != TokenTypes.Eos)
            {
                //parse 'key=value'
                //match key
                if(ParseCurrent.type == TokenTypes.Key)
                {
                    var key = Match(TokenTypes.Key);
                    _builder.Append(key.data);
                }

                _builder.Append(':');


                //match value
                if (ParseCurrent.type == TokenTypes.Value)
                {
                    var value = Match(TokenTypes.Value);
                    _builder.Append(value.data);
                }
            }

            return _builder.ToString();
        }

    }


    public record struct DataType(TokenTypes type, string data);

    public enum TokenTypes
    {
        Eos = 0,
        Key = 1, 
        Value = 2,
        Ampersand = 3,
        Equals = 4
    }
}
