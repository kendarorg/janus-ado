using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class CommandComplete:PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            try
            {
                var to = stream.ReadTimeout;
                stream.ReadTimeout = 100;
                var result = ReadData(stream, () =>
                    stream.ReadByte() == (byte)BackendMessageCode.CommandComplete);

                stream.ReadTimeout = to;
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("CommandComplete");
            stream.ReadByte();
            stream.ReadInt32();
            var data = stream.ReadAsciiString();
            try
            {
                var spl = data.Split(" ");
                Tag = spl[0];
                Count = int.Parse(spl[1]);
            }catch(Exception){}
        }

        public int Count { get; set; }

        public string Tag { get; set; }
    }
}
