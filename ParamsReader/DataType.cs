using System;

namespace ParamsReader
{
    public record struct DataType(TokenType Type, ReadOnlyMemory<char> Data);
}
