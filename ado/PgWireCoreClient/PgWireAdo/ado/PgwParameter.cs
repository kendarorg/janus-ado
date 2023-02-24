using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.ado
{
    public class PgwParameter<T>:PgwParameter
    {
        public PgwParameter(string parameterName,T? value)
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }

    }
    public class PgwParameter:DbParameter
    {

        public PgwParameter(string parameterName, DbType dbType)
        {
            this.ParameterName = parameterName;
            this.DbType = dbType;
        }

        public PgwParameter(object value)
        {
            this.Value= value;
        }

        public PgwParameter()
        {
            
        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; [param: AllowNull] set; }
        public override string SourceColumn { get; [param: AllowNull] set; }
        public override object? Value { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }
        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }
    }
}
