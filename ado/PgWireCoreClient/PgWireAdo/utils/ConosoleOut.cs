namespace PgWireAdo.utils;

public class ConsoleOut
{
    private static Action<string> _action = (String a) =>
    {
        Console.WriteLine(a);
    };
    public static void WriteLine(String data)
    {
        _action.Invoke(DateTime.Now+ " "+data);
    }

    public static void setup(Action<string> action)
    {
        _action=action;
    }
}