namespace PgWireAdo.utils.parse;

public class SqlParameter
{
    public bool Named { get; set; }
    public string? Name { get; set; }
    public int? Index { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }

    public override string ToString()
    {
        return Named + " " + Name + " " + Index + " " + Start + " " + Length;
    }
}