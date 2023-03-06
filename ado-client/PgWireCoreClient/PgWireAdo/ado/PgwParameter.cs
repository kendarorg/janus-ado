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
            this.Direction = ParameterDirection.Input;
        }

    }
    public class PgwParameter:DbParameter
    {
        private object? _value;

        public PgwParameter(string parameterName, DbType dbType)
        {
            this.ParameterName = parameterName;
            this.DbType = dbType;
            this.Direction = ParameterDirection.Input;
        }

        public PgwParameter(object value)
        {
            this.Value= value;
            this.Direction = ParameterDirection.Input;
        }

        public PgwParameter()
        {
            this.Direction = ParameterDirection.Input;

        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; [param: AllowNull] set; }
        public override string SourceColumn { get; [param: AllowNull] set; }

        public override object? Value
        {
            get => _value;
            set
            {
                if (value != null)
                {
                    DbType = PgwConverter.ConvertToDbType(value); ;
                }
                _value = value;
            }
        }

        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }
        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }
    }
}
