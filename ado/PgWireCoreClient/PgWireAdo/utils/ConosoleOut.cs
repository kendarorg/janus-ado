using System.Diagnostics;
using System.Globalization;

namespace PgWireAdo.utils;

public class ConsoleOut
{
    private static Action<string> _action = (String a) =>
    {
        Console.WriteLine(a);
    };
    public static void WriteLine(String data)
    {
        if(data.StartsWith("["))return;
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
            CultureInfo.InvariantCulture);
        _action.Invoke(timestamp + " "+data);
        Trace.Flush();
    }

    public static void setup(Action<string> action)
    {
        _action=action;
    }
}