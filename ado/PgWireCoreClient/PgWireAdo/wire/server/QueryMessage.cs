﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.server
{
    public class QueryMessage : PgwServerMessage
    {
        private readonly string _query;

        public QueryMessage(String query)
        {
            _query = query;
        }
        public override void Write(ReadSeekableStream stream)
        {
            int length = 1 + 4 + _query.Length+1;
            WriteByte((byte)'Q');
            WriteInt32(length);
            WriteASCIIString(_query);
            WriteByte(0);
            Flush(stream);
        }
    }
}