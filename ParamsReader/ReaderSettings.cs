using System.CodeDom.Compiler;

namespace ParamsReader;

public struct ReaderSettings
{
    private OutputType _outputType;

    public OutputType Type
    {
        get => _outputType;

        set
        {
            if (value == null)
            {
                _outputType = OutputType.String;
            }
            else
            {
                _outputType = value;
            }
        }
    }

    public IndentedTextWriter? Writer { get; set; }
}
