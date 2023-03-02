using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class CommandComplete:PgwClientMessage
    {

        public override BackendMessageCode BeType => BackendMessageCode.CommandComplete;
        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: CommandComplete");
            var data = stream.ReadAsciiString();
            try
            {
                var spl = data.Split(" ");
                Tag = spl[0];
                Count = int.Parse(spl[spl.Length-1]);
            }catch(Exception){}
        }

        public int Count { get; set; }

        public string Tag { get; set; }
    }
}
