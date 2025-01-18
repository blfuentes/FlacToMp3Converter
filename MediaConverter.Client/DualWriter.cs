using System.IO;
using System.Text;

namespace MediaConverter.Client;

public class DualWriter : TextWriter
{
    private readonly TextWriter consoleWriter;
    private readonly StringBuilder stringBuilder;

    public event Action<string> OutputUpdated;

    public string Output => stringBuilder.ToString();

    public DualWriter(TextWriter consoleWriter)
    {
        this.consoleWriter = consoleWriter;
        this.stringBuilder = new StringBuilder();
    }

    public override Encoding Encoding => consoleWriter.Encoding;

    public override void Write(char value)
    {
        consoleWriter.Write(value);
        stringBuilder.Append(value);
        OutputUpdated?.Invoke(Output);
    }

    public override void Write(string value)
    {
        consoleWriter.Write(value);
        stringBuilder.Append(value);
        OutputUpdated?.Invoke(Output);
    }

    public override void WriteLine(string value)
    {
        consoleWriter.WriteLine(value);
        stringBuilder.AppendLine(value);
        OutputUpdated?.Invoke(Output);
    }
}
