using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.utils
{
    public class EncodingResult
    {
        public int Length { get; set; }
        public byte[] Data { get; set; }
    }
    public class EncodingUtils
    {
        public static EncodingResult GetUTF8(String input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            return new EncodingResult()
            {
                Length = data.Length,
                Data = data
            };
        }
    }
}
