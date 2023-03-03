using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.ado
{
    public class PgwException:Exception
    {
        public PgwException(String message) : base(message)
        {

        }
        public PgwException(Exception exception):base("ERROR",exception)
        {
            
        }
    }
}
