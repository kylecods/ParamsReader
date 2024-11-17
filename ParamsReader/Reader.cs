using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace ParamsReader;

public sealed class Reader : IDisposable
{
    private ReaderSettings _settings;

    private bool disposedValue;

    private readonly IndentedTextWriter _writer;

    private readonly StringWriter _stringWriter;

    private DataType datatype = new ();

    private ref DataType _datatype => ref datatype;

    private ParamSyntax paramSyntax = new();

    private ref ParamSyntax _paramSyntax => ref paramSyntax;

    private static List<ParamSyntax> _syntaxes = [];

    private static List<DataType> _tokens = [];

    private int _parsePosition;

    public Reader(ReaderSettings settings = default)
    {
        _settings = settings;

        _stringWriter = new StringWriter();

        _writer = new IndentedTextWriter(_stringWriter);
    }

    private DataType ParseCurrent => _tokens[_parsePosition];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DataType Advance()
    {
        var current = ParseCurrent;

        _parsePosition++;

        return current;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DataType Match(TokenType type)
    {
        if (ParseCurrent.Type == type)
        {
            return Advance();
        }
        throw new InvalidDataException("Unexpected value");
    }

    private void Tokenize(ReadOnlyMemory<char> memText)
    {
        var position = 0;

        var text = memText.Span;

        while (position < text.Length)
        {
            var current = text[position];

            switch (current)
            {
                case '&'://skip
                    position++;
                    break;
                case '=':
                    position++;
                    current = text[position];
                    if (char.IsLetterOrDigit(current))
                    {
                        var start = position;

                        bool isDecimal = false;

                        while (position < text.Length && 
                            (char.IsLetterOrDigit(current) || (!isDecimal && current == '.')))
                        {
                            if (current == '.') isDecimal = true;
                            position++;
                            if (position < text.Length) current = text[position];
                        }

                        _datatype.Type = TokenType.Value;
                        _datatype.Data = memText.Slice(start, position - start);

                        _tokens.Add(_datatype);
                    }
                    break;
                default:
                    if (char.IsLetter(current))
                    {
                        var start = position;

                        while (position < text.Length && char.IsLetter(current))
                        {
                            position++;
                            if (position < text.Length) current = text[position];
                        }

                        _datatype.Type = TokenType.Key;
                        _datatype.Data = memText.Slice(start, position - start);

                        _tokens.Add(_datatype);
                    }
                    break;
            }
        }


        _datatype.Type = TokenType.Eos;
        _datatype.Data = null;

        _tokens.Add(_datatype);
    }

    
    public string ParseToString(string data)
    {
        var text = data.AsMemory();

        var syntax = Parse(text);

        _writer.Write("[");
        _writer.WriteLine();
        _writer.Indent++;
        for (int i = 0; i < syntax.Syntaxes.Count; i++)
        {
            var token = syntax.Syntaxes[i];
            _writer.Write(token.Key.Data);
            _writer.Write(":");
            _writer.Write(token.Value.Data);
            _writer.WriteLine();
        }
        _writer.Indent--;
        _writer.Write(']');


        return _stringWriter.ToString();
    }

    private MainSyntax Parse(ReadOnlyMemory<char> text)
    {
        Tokenize(text);

        return InternalParse();
    }

    private MainSyntax InternalParse()
    {
        while (ParseCurrent.Type != TokenType.Eos)
        {
            //parse 'key=value'
            //match key
            if (ParseCurrent.Type == TokenType.Key)
            {
                var key = Match(TokenType.Key);

                _paramSyntax.Key = key;

            }

            //match value
            if (ParseCurrent.Type == TokenType.Value)
            {
                var value = Match(TokenType.Value);

                _paramSyntax.Value = value;
            }
            _syntaxes.Add(_paramSyntax);
            
        }

        return new MainSyntax(_syntaxes);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _writer.Dispose();

                _stringWriter.Dispose();

                _syntaxes.Clear();

                _tokens.Clear();
            }
            disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
