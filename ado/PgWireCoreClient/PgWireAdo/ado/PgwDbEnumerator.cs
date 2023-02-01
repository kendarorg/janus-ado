using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.ado
{
    public class PgwDbEnumerator:IEnumerator
    {
        private readonly PgwDataReader _pgwDataReader;

        public PgwDbEnumerator(PgwDataReader pgwDataReader)
        {
            _pgwDataReader = pgwDataReader;
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public object Current { get; }
    }
}
