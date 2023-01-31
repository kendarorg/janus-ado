using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.wire.client;
using PgWireAdo.wire.server;

namespace PgWireAdo.ado;

public class PgwCommand : DbCommand
{
    private List<RowDescriptor> _fields;

    public PgwCommand(DbConnection dbConnection)
    {
        DbConnection = dbConnection;
    }

    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }


    protected override DbParameter CreateDbParameter()
    {
        return new PgwParameter();
    }
    public override void Cancel()
    {
        throw new NotImplementedException();
    }

    public override int ExecuteNonQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        CallQuery();
        var commandComplete = new CommandComplete();
        if (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            return commandComplete.Count;
        }

        return 0;
    }

    public override object? ExecuteScalar()
    {
        CallQuery();
        throw new NotImplementedException();
    }

    public override void Prepare()
    {
        
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        CallQuery();
        throw new NotImplementedException();
    }

    private void CallQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        if (DbParameterCollection == null || DbParameterCollection.Count == 0)
        {
            var queryMessage = new QueryMessage(CommandText);
            queryMessage.Write(stream);
        }
        else
        {
            throw new NotImplementedException();
        }

        var rowDescription = new RowDescription();
        if (rowDescription.IsMatching(stream))
        {
            rowDescription.Read(stream);
            _fields = rowDescription.Fields;
        }

    }
}
