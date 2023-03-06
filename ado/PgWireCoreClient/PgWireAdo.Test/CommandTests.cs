
using NUnit.Framework;
using System;
using System.Buffers.Binary;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PgWireAdo.Test.Utils;
using System.Data;
using PgWireAdo.ado;
using static PgWireAdo.Test.Utils.TestUtils;

using NpgsqlCommand = PgWireAdo.ado.PgwCommand;
using PostgresException = PgWireAdo.ado.PgwException;
using NpgsqlException = PgWireAdo.ado.PgwException;
using NpgsqlDbType = System.Data.DbType;
using PgWireAdo.utils;

namespace Npgsql.Tests;

public class CommandTests : TestBase
{
    #region Legacy batching

    [Test]
    [TestCase(new[] { true }, TestName = "SingleQuery")]
    [TestCase(new[] { false }, TestName = "SingleNonQuery")]
    [TestCase(new[] { true, true }, TestName = "TwoQueries")]
    [TestCase(new[] { false, false }, TestName = "TwoNonQueries")]
    [TestCase(new[] { false, true }, TestName = "NonQueryQuery")]
    [TestCase(new[] { true, false }, TestName = "QueryNonQuery")]
    public async Task Multiple_statements(bool[] queries)
    {
        await using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "name TEXT");
        var executedQueries = new List<string>();
        var sb = new StringBuilder();
        foreach (var query in queries)
        {
            sb.Append(query ? "SELECT 1;" : $"UPDATE {table} SET name='yo' ;");
            executedQueries.Add(query ? "SELECT 1;" : $"UPDATE {table} SET name='yo' ;");
        }

        var ins = conn.CreateCommand();
        ins.CommandText = $"INSERT INTO {table} VALUES('ma')";
        ins.ExecuteNonQuery();

        var sql = sb.ToString();
        var prepInt = 0;
        //foreach (var prepare in new[] { false, true })
        var prepare = false;
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (prepare && !IsMultiplexing)
                await cmd.PrepareAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var numResultSets = queries.Count(q => q);
            for (var i = 0; i < numResultSets; i++)
            {
                Assert.That(await reader.ReadAsync(), Is.True);
                Assert.That(reader[0], Is.EqualTo(1));
                Assert.That(await reader.NextResultAsync(), Is.EqualTo(i != numResultSets - 1));
            }

            prepInt++;
        }
    }
    

    [Test]
    public async Task Multiple_statements_with_parameters([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        await using var conn = await OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CONVERT(@p1,INTEGER); SELECT @p2";
        var p1 = new NpgsqlParameter("p1", NpgsqlDbType.Int64);
        var p2 = new NpgsqlParameter("p2", NpgsqlDbType.String);
        cmd.Parameters.Add(p1);
        cmd.Parameters.Add(p2);
        if (prepare == PrepareOrNot.Prepared)
            cmd.Prepare();
        p1.Value = 8;
        p2.Value = "foo";
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.That(await reader.ReadAsync(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(8));
        Assert.That(await reader.NextResultAsync(), Is.True);
        Assert.That(await reader.ReadAsync(), Is.True);
        Assert.That(reader.GetString(0), Is.EqualTo("foo"));
        Assert.That(await reader.NextResultAsync(), Is.False);
    }

    [Test]
    public async Task SingleRow_legacy_batching([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT 1; SELECT 2", conn);
        if (prepare == PrepareOrNot.Prepared)
            cmd.Prepare();
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(1));
        Assert.That(reader.Read(), Is.False);
        Assert.That(reader.NextResult(), Is.False);
    }

    [Test, Description("Makes sure a later command can depend on an earlier one")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/641")]
    public async Task Multiple_statements_with_dependencies()
    {
        using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "a INT");

        await conn.ExecuteNonQueryAsync($"ALTER TABLE {table} ADD COLUMN b INT; INSERT INTO {table} (b) VALUES (8)");
        Assert.That(await conn.ExecuteScalarAsync($"SELECT b FROM {table}"), Is.EqualTo(8));
    }

    [Test, Description("Forces async write mode when the first statement in a multi-statement command is big")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/641")]
    public async Task Multiple_statements_large_first_command()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand($"SELECT repeat('X', {WriteBufferSize}); SELECT @p", conn);
        var expected1 = new string('X', WriteBufferSize);
        var expected2 = new string('Y', WriteBufferSize);
        cmd.Parameters.AddWithValue("p", expected2);
        using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        Assert.That(reader.GetString(0), Is.EqualTo(expected1));
        reader.NextResult();
        reader.Read();
        Assert.That(reader.GetString(0), Is.EqualTo(expected2));
    }

    [Test]
    [NonParallelizable] // Disables sql rewriting
    public async Task Legacy_batching_is_not_supported_when_EnableSqlParsing_is_disabled()
    {

        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT 1; SELECT 2", conn);
        Assert.That(async () => await cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<PostgresException>());
    }

    #endregion

    #region Timeout

    [Test, Description("Checks that CommandTimeout gets enforced as a socket timeout")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/327")]
    public async Task Timeout()
    {
        if (IsMultiplexing)
            return; // Multiplexing, Timeout

        await using var dataSource = CreateDataSource(csb => csb.CommandTimeout = 1);
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = CreateSleepCommand(conn, 10);
        Assert.That(() => cmd.ExecuteNonQuery(), Throws.Exception
            .TypeOf<InvalidOperationException>()
        );
        Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
    }

    [Test, Description("Times out an async operation, testing that cancellation occurs successfully")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/607")]
    public async Task Timeout_async_soft()
    {
        if (IsMultiplexing)
            return; // Multiplexing, Timeout

        await using var dataSource = CreateDataSource(csb => csb.CommandTimeout = 1);
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = CreateSleepCommand(conn, 10);
        Assert.That(async () => await cmd.ExecuteNonQueryAsync(),
            Throws.Exception
                .TypeOf<InvalidOperationException>());
        Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
    }
    

    #endregion
    

    #region Cursors

    //[Test] no cursors on h2
    public async Task Cursor_statement()
    {
        using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "name TEXT");
        using var t = conn.BeginTransaction();

        for (var x = 0; x < 5; x++)
            await conn.ExecuteNonQueryAsync($"INSERT INTO {table} (name) VALUES ('X')");

        var i = 0;
        var command = new NpgsqlCommand($"DECLARE TE CURSOR FOR SELECT * FROM {table}", conn);
        command.ExecuteNonQuery();
        command.CommandText = "FETCH FORWARD 3 IN TE";
        var dr = command.ExecuteReader();

        while (dr.Read())
            i++;
        Assert.AreEqual(3, i);
        dr.Close();

        i = 0;
        command.CommandText = "FETCH BACKWARD 1 IN TE";
        var dr2 = command.ExecuteReader();
        while (dr2.Read())
            i++;
        Assert.AreEqual(1, i);
        dr2.Close();

        command.CommandText = "close te;";
        command.ExecuteNonQuery();
    }

    [Test]
    public async Task Cursor_move_RecordsAffected()
    {
        using var connection = await OpenConnectionAsync();
        using var transaction = connection.BeginTransaction();
        var command = new NpgsqlCommand("DECLARE curs CURSOR FOR SELECT * FROM (VALUES (1), (2), (3)) as t", connection);
        command.ExecuteNonQuery();
        command.CommandText = "MOVE FORWARD ALL IN curs";
        var count = command.ExecuteNonQuery();
        Assert.AreEqual(3, count);
    }

    #endregion

    #region CommandBehavior.CloseConnection

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/693")]
    public async Task CloseConnection()
    {
        using var conn = await OpenConnectionAsync();
        using (var cmd = new NpgsqlCommand("SELECT 1", conn))
        using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            while (reader.Read()) {}
        Assert.That(conn.State, Is.EqualTo(ConnectionState.Closed));
    }

    //[Test] ????
    public async Task CloseConnection_with_exception()
    {
        using var conn = await OpenConnectionAsync();
        using (var cmd = new NpgsqlCommand("SE", conn))
            Assert.That(() => cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection), Throws.Exception.TypeOf<PostgresException>());
        Assert.That(conn.State, Is.EqualTo(ConnectionState.Closed));
    }

    #endregion

    [Test]
    public async Task SingleRow([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1, 2 UNION SELECT 3, 4", conn);
        if (prepare == PrepareOrNot.Prepared)
            cmd.Prepare();

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        Assert.That(() => reader.GetInt32(0), Throws.Exception.TypeOf<InvalidOperationException>());
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(1));
        Assert.That(reader.Read(), Is.False);
    }

    #region Parameters

    [Test]
    public async Task Positional_parameter()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT CONVERT($1,INTEGER)", conn);
        cmd.Parameters.Add(new NpgsqlParameter { DbType = NpgsqlDbType.Int32, Value = 8 });
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(8));
    }

    [Test]
    public async Task Positional_parameters_are_not_supported_with_legacy_batching()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT CONVERT($1,INTEGER); CONVERT($1,INTEGER)", conn);
        cmd.Parameters.Add(new NpgsqlParameter { DbType = NpgsqlDbType.Int32, Value = 8 });
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(8));
        Assert.That(async () => await cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<PostgresException>());
    }

    [Test]
    [NonParallelizable] // Disables sql rewriting
    public async Task Positional_parameters_are_supported_when_EnableSqlParsing_is_disabled()
    {
        
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT CONVERT($1,INTEGER)", conn);
        cmd.Parameters.Add(new NpgsqlParameter { DbType = NpgsqlDbType.Int32, Value = 8 });
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(8));
    }

    [Test]
    [NonParallelizable] // Disables sql rewriting
    public async Task Named_parameters_are_not_supported_when_EnableSqlParsing_is_disabled()
    {
         using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @p", conn);
        cmd.Parameters.Add(new NpgsqlParameter("p", 8));
        Assert.That(async () => await cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<NotSupportedException>());
    }

    [Test, Description("Makes sure writing an unset parameter isn't allowed")]
    public async Task Parameter_without_Value()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @p", conn);
        cmd.Parameters.Add(new NpgsqlParameter("@p", NpgsqlDbType.Int32));
        Assert.That(() => cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<Exception>());
    }

    [Test]
    public async Task Unreferenced_named_parameter_works()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        cmd.Parameters.AddWithValue("not_used", 8);
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Unreferenced_positional_parameter_works()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        cmd.Parameters.Add(new NpgsqlParameter { Value = 8 });
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Mixing_positional_and_named_parameters_is_not_supported()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT $1, @p", conn);
        cmd.Parameters.Add(new NpgsqlParameter { Value = 8 });
        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = "p", Value = 9 });
        Assert.That(() => cmd.ExecuteNonQueryAsync(), Throws.Exception.TypeOf<Exception>());
    }

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/4171")]
    public async Task Cached_command_clears_parameters_placeholder_type()
    {
        await using var conn = await OpenConnectionAsync();

        await using (var cmd1 = conn.CreateCommand())
        {
            cmd1.CommandText = "SELECT CONVERT(@p1,INTEGER)";
            cmd1.Parameters.AddWithValue("@p1", 8);
            await using var reader1 = await cmd1.ExecuteReaderAsync();
            reader1.Read();
            Assert.That(reader1[0], Is.EqualTo(8));
        }

        await using (var cmd2 = conn.CreateCommand())
        {
            cmd2.CommandText = "SELECT CONVERT($1,INTEGER)";
            cmd2.Parameters.AddWithValue(8);
            await using var reader2 = await cmd2.ExecuteReaderAsync();
            reader2.Read();
            Assert.That(reader2[0], Is.EqualTo(8));
        }
    }

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/4171")]
    public async Task Reuse_command_with_different_parameter_placeholder_types()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT ?";
        cmd.Parameters.AddWithValue("@p1", 8);
        _ = await cmd.ExecuteScalarAsync();

        cmd.CommandText = "SELECT $1";
        cmd.Parameters[0].ParameterName = null;
        _ = await cmd.ExecuteScalarAsync();
    }

    [Test]
    public async Task Positional_output_parameters_are_not_supported()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT $1", conn);
        cmd.Parameters.Add(new NpgsqlParameter { Value = 8, Direction = ParameterDirection.InputOutput });
        Assert.That(() => cmd.ExecuteNonQueryAsync(), Throws.Exception.TypeOf<NotSupportedException>());
    }

    [Test]
    public void Parameters_get_name()
    {
        var command = new NpgsqlCommand();

        // Add parameters.
        command.Parameters.Add(new NpgsqlParameter(":Parameter1", DbType.Boolean));
        command.Parameters.Add(new NpgsqlParameter(":Parameter2", DbType.Int32));
        command.Parameters.Add(new NpgsqlParameter(":Parameter3", DbType.DateTime));
        command.Parameters.Add(new NpgsqlParameter("Parameter4", DbType.DateTime));

        var idbPrmtr = command.Parameters["Parameter1"];
        Assert.IsNotNull(idbPrmtr);
        command.Parameters[0].Value = 1;

        // Get by indexers.

        Assert.AreEqual(":Parameter1", command.Parameters["Parameter1"].ParameterName);
        Assert.AreEqual(":Parameter2", command.Parameters["Parameter2"].ParameterName);
        Assert.AreEqual(":Parameter3", command.Parameters["Parameter3"].ParameterName);
        Assert.AreEqual("Parameter4", command.Parameters["Parameter4"].ParameterName); //Should this work?

        Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
        Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
        Assert.AreEqual(":Parameter3", command.Parameters[2].ParameterName);
        Assert.AreEqual("Parameter4", command.Parameters[3].ParameterName);
    }

    [Test]
    public async Task Same_param_multiple_times()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT CONVERT(@p1,INTEGER),CONVERT(@p1,INTEGER)", conn);
        cmd.Parameters.AddWithValue("@p1", 8);
        using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        Assert.That(reader[0], Is.EqualTo(8));
        Assert.That(reader[1], Is.EqualTo(8));
    }

    [Test]
    public async Task Generic_parameter()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @p1, @p2, @p3, @p4", conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("p1", 8));
        cmd.Parameters.Add(new NpgsqlParameter<short>("p2", 8) { DbType = NpgsqlDbType.Int32 });
        cmd.Parameters.Add(new NpgsqlParameter<string>("p3", "hello"));
        cmd.Parameters.Add(new NpgsqlParameter<char[]>("p4", new[] { 'f', 'o', 'o' }));
        using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        Assert.That(reader.GetInt32(0), Is.EqualTo(8));
        Assert.That(reader.GetInt32(1), Is.EqualTo(8));
        Assert.That(reader.GetString(2), Is.EqualTo("hello"));
        Assert.That(reader.GetString(3), Is.EqualTo("foo"));
    }

    #endregion Parameters

    [Test]
    public async Task CommandText_not_set()
    {
        await using var conn = await OpenConnectionAsync();
        await using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            Assert.That(cmd.ExecuteNonQueryAsync, Throws.Exception.TypeOf<InvalidOperationException>());
            cmd.CommandText = null;
            Assert.That(cmd.ExecuteNonQueryAsync, Throws.Exception.TypeOf<InvalidOperationException>());
            cmd.CommandText = "";
        }

        await using (var cmd = conn.CreateCommand())
            Assert.That(cmd.ExecuteNonQueryAsync, Throws.Exception.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task ExecuteScalar()
    {
        await using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "name TEXT");
        await using var command = new NpgsqlCommand($"SELECT name FROM {table}", conn);
        Assert.That(command.ExecuteScalarAsync, Is.EqualTo(0));

        await conn.ExecuteNonQueryAsync($"INSERT INTO {table} (name) VALUES (NULL)");
        Assert.That(command.ExecuteScalarAsync, Is.EqualTo(DBNull.Value));

        await conn.ExecuteNonQueryAsync($"TRUNCATE TABLE {table}");
        for (var i = 0; i < 2; i++)
            await conn.ExecuteNonQueryAsync($"INSERT INTO {table} (name) VALUES ('X')");
        Assert.That(command.ExecuteScalarAsync, Is.EqualTo("X"));
    }

    [Test]
    public async Task ExecuteNonQuery()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand { Connection = conn };
        var table = await CreateTempTable(conn, "name TEXT");

        cmd.CommandText = $"INSERT INTO {table} (name) VALUES ('John')";
        Assert.That(cmd.ExecuteNonQueryAsync, Is.EqualTo(1));

        cmd.CommandText = $"INSERT INTO {table} (name) VALUES ('John'); INSERT INTO {table} (name) VALUES ('John')";
        Assert.That(cmd.ExecuteNonQueryAsync, Is.EqualTo(2));

        cmd.CommandText = $"INSERT INTO {table} (name) VALUES ('{new string('x', WriteBufferSize)}')";
        Assert.That(cmd.ExecuteNonQueryAsync, Is.EqualTo(1));
    }

    [Test, Description("Makes sure a command is unusable after it is disposed")]
    public async Task Dispose()
    {
        await using var conn = await OpenConnectionAsync();
        var cmd = new NpgsqlCommand("SELECT 1", conn);
        cmd.Dispose();
        Assert.That(() => cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<ObjectDisposedException>());
        Assert.That(() => cmd.ExecuteNonQueryAsync(), Throws.Exception.TypeOf<ObjectDisposedException>());
        Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<ObjectDisposedException>());
        Assert.That(() => cmd.PrepareAsync(), Throws.Exception.TypeOf<ObjectDisposedException>());
    }

    [Test, Description("Disposing a command with an open reader does not close the reader. This is the SqlClient behavior.")]
    public async Task Command_Dispose_does_not_close_reader()
    {
        await using var conn = await OpenConnectionAsync();
        var cmd = new NpgsqlCommand("SELECT 1, 2", conn);
        await cmd.ExecuteReaderAsync();
        cmd.Dispose();
        Assert.That(() => cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<ObjectDisposedException>());
    }


    [Test]
    public async Task Parameter_and_operator_unclear()
    {
        await using var conn = await OpenConnectionAsync();
        //Without parenthesis the meaning of [, . and potentially other characters is
        //a syntax error. See comment in NpgsqlCommand.GetClearCommandText() on "usually-redundant parenthesis".
        await using var command = new NpgsqlCommand("select :arr[2]", conn);
        command.Parameters.AddWithValue(":arr", new int[] {5, 4, 3, 2, 1});
        await using var rdr = await command.ExecuteReaderAsync();
        rdr.Read();
        Assert.AreEqual(rdr.GetInt32(0), 4);
    }

    [Test]
    [TestCase(CommandBehavior.Default)]
    [TestCase(CommandBehavior.SequentialAccess)]
    public async Task Statement_mapped_output_parameters(CommandBehavior behavior)
    {
        await using var conn = await OpenConnectionAsync();
        var command = new NpgsqlCommand("select 3, 4 as param1, 5 as param2, 6;", conn);

        var p = new NpgsqlParameter("param2", NpgsqlDbType.Int32);
        p.Direction = ParameterDirection.Output;
        p.Value = -1;
        command.Parameters.Add(p);

        p = new NpgsqlParameter("param1", NpgsqlDbType.Int32);
        p.Direction = ParameterDirection.Output;
        p.Value = -1;
        command.Parameters.Add(p);

        p = new NpgsqlParameter("p", NpgsqlDbType.Int32);
        p.Direction = ParameterDirection.Output;
        p.Value = -1;
        command.Parameters.Add(p);

        await using var reader = await command.ExecuteReaderAsync(behavior);

        Assert.AreEqual(4, command.Parameters["param1"].Value);
        Assert.AreEqual(5, command.Parameters["param2"].Value);

        reader.Read();

        Assert.AreEqual(3, reader.GetInt32(0));
        Assert.AreEqual(4, reader.GetInt32(1));
        Assert.AreEqual(5, reader.GetInt32(2));
        Assert.AreEqual(6, reader.GetInt32(3));
    }
    

    [Test]
    public async Task TableDirect()
    {
        using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "name TEXT");

        await conn.ExecuteNonQueryAsync($"INSERT INTO {table} (name) VALUES ('foo')");
        using var cmd = new NpgsqlCommand(table, conn) { CommandType = CommandType.TableDirect };
        using var rdr = await cmd.ExecuteReaderAsync();
        Assert.That(rdr.Read(), Is.True);
        Assert.That(rdr["name"], Is.EqualTo("foo"));
    }

    [Test]
    [TestCase(CommandBehavior.Default)]
    [TestCase(CommandBehavior.SequentialAccess)]
    public async Task Input_and_output_parameters(CommandBehavior behavior)
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @c-1 AS c, @a+2 AS b", conn);
        cmd.Parameters.Add(new NpgsqlParameter("a", 3));
        var b = new NpgsqlParameter { ParameterName = "b", Direction = ParameterDirection.Output };
        cmd.Parameters.Add(b);
        var c = new NpgsqlParameter { ParameterName = "c", Direction = ParameterDirection.InputOutput, Value = 4 };
        cmd.Parameters.Add(c);
        using (await cmd.ExecuteReaderAsync(behavior))
        {
            Assert.AreEqual(5, b.Value);
            Assert.AreEqual(3, c.Value);
        }
    }

    [Test]
    public async Task Send_NpgsqlDbType_Unknown([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @p::TIMESTAMP", conn);
        cmd.CommandText = "SELECT @p::TIMESTAMP";
        cmd.Parameters.Add(new NpgsqlParameter("p", DbType.DateTime) { Value = "2008-1-1" });
        if (prepare == PrepareOrNot.Prepared)
            cmd.Prepare();
        using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        Assert.That(reader.GetValue(0), Is.EqualTo(new DateTime(2008, 1, 1)));
    }

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/503")]
    public async Task Invalid_UTF8()
    {
        const string badString = "SELECT 'abc\uD801\uD802d'";
        await using var dataSource = CreateDataSource();
        using var conn = await OpenConnectionAsync(); //dataSource.OpenConnectionAsync();
        Assert.That(() => conn.ExecuteScalarAsync(badString), Throws.Exception.TypeOf<EncoderFallbackException>());
    }

    [Test, Description("CreateCommand before connection open")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/565")]
    public async Task Create_command_before_connection_open()
    {
        using var conn = new PgwConnection(ConnectionString);
        var cmd = new NpgsqlCommand("SELECT 1", conn);
        conn.Open();
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(1));
    }

    [Test]
    public void Connection_not_set_throws()
    {
        var cmd = new NpgsqlCommand("SELECT 1");
        Assert.That(() => cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Connection_not_open_throws()
    {
        using var conn = CreateConnection();
        var cmd = new NpgsqlCommand("SELECT 1", conn);
        Assert.That(() => cmd.ExecuteScalarAsync(), Throws.Exception.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task ExecuteNonQuery_Throws_PostgresException([Values] bool async)
    {
        if (!async && IsMultiplexing)
            return;

        await using var conn = await OpenConnectionAsync();

        var table1 = await CreateTempTable(conn, "id integer PRIMARY key, t varchar(40)");
        var table2 = await CreateTempTable(conn, $"id SERIAL primary key, {table1}_id integer references {table1}(id) DEFERRABLE");

        //var sql = $"insert into {table2} ({table1}_id) values (1) returning id";
        
        var sql = $"SELECT id FROM  (insert into {table2} ({table1}_id) values (1))";

        var ex = async
            ? Assert.ThrowsAsync<Exception>(async () => await conn.ExecuteNonQueryAsync(sql))
            : Assert.Throws<Exception>(() => conn.ExecuteNonQuery(sql));
    }

    [Test]
    public async Task ExecuteScalar_Throws_PostgresException([Values] bool async)
    {
        if (!async && IsMultiplexing)
            return;

        await using var conn = await OpenConnectionAsync();

        var table1 = await CreateTempTable(conn, "id integer PRIMARY key, t varchar(40)");
        var table2 = await CreateTempTable(conn, $"id SERIAL primary key, {table1}_id integer references {table1}(id) DEFERRABLE");

        var sql = $"SELECT asda5w4sdfasas453#$%^$%";

        var ex = async
            ? Assert.ThrowsAsync<Exception>(async () => await conn.ExecuteScalarAsync(sql))
            : Assert.Throws<Exception>(() => conn.ExecuteScalar(sql));
    }

    [Test]
    public async Task ExecuteReader_Throws_PostgresException([Values] bool async)
    {
        if (!async && IsMultiplexing)
            return;

        await using var conn = await OpenConnectionAsync();

        var table1 = await CreateTempTable(conn, "id integer PRIMARY key, t varchar(40)");
        var table2 = await CreateTempTable(conn, $"id SERIAL primary key, {table1}_id integer references {table1}(id) DEFERRABLE");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT id FROM  (insert into {table2} ({table1}_id) values (1))";

        await using var reader = async
            ? await cmd.ExecuteReaderAsync()
            : cmd.ExecuteReader();

        Assert.IsTrue(async ? await reader.ReadAsync() : reader.Read());
        var value = reader.GetInt32(0);
        Assert.That(value, Is.EqualTo(1));
        Assert.IsFalse(async ? await reader.ReadAsync() : reader.Read());
        var ex = async
            ? Assert.ThrowsAsync<Exception>(async () => await reader.NextResultAsync())
            : Assert.Throws<Exception>(() => reader.NextResult());
    }

    [Test]
    public void Command_is_NOT_recycled()
    {
        using var conn = OpenConnection();
        var cmd1 = conn.CreateCommand();
        cmd1.CommandText = "SELECT CONVERT(@p1,INTEGER)";
        var tx = conn.BeginTransaction();
        cmd1.Transaction = tx;
        cmd1.Parameters.AddWithValue("p1", 8);
        _ = cmd1.ExecuteScalar();
        cmd1.Dispose();

        var cmd2 = conn.CreateCommand();
        Assert.That(cmd2, Is.Not.SameAs(cmd1));
        Assert.That(cmd2.CommandText, Is.Empty);
        Assert.That(cmd2.CommandType, Is.EqualTo(CommandType.Text));
        Assert.That(cmd2.Transaction, Is.Null);
        Assert.That(cmd2.Parameters, Is.Empty);
        // TODO: Leaving this for now, since it'll be replaced by the new batching API
        // Assert.That(cmd2.Statements, Is.Empty);
    }

    //[Test] commands are not recylced
    public void Command_recycled_resets_CommandType()
    {
        using var conn = CreateConnection();
        var cmd1 = conn.CreateCommand();
        cmd1.CommandType = CommandType.StoredProcedure;
        cmd1.Dispose();

        var cmd2 = conn.CreateCommand();
        Assert.That(cmd2.CommandType, Is.EqualTo(CommandType.Text));
    }

    //[Test] TODO
    [IssueLink("https://github.com/npgsql/npgsql/issues/831")]
    [IssueLink("https://github.com/npgsql/npgsql/issues/2795")]
    public async Task Many_parameters([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        using var conn = await OpenConnectionAsync();
        var table = await CreateTempTable(conn, "some_column INT");
        using var cmd = new NpgsqlCommand { Connection = conn };
        var sb = new StringBuilder($"INSERT INTO {table} (some_column) VALUES ");
        for (var i = 0; i < ushort.MaxValue; i++)
        {
            var paramName = "p" + i;
            cmd.Parameters.Add(new NpgsqlParameter(paramName, 8));
            if (i > 0)
                sb.Append(", ");
            sb.Append($"(@{paramName})");
        }

        cmd.CommandText = sb.ToString();

        if (prepare == PrepareOrNot.Prepared)
            cmd.Prepare();

        await cmd.ExecuteNonQueryAsync();
    }

    //[Test, Description("Bypasses PostgreSQL's uint16 limitation on the number of parameters")]
    //[IssueLink("https://github.com/npgsql/npgsql/issues/831")]
    //[IssueLink("https://github.com/npgsql/npgsql/issues/858")]
    //[IssueLink("https://github.com/npgsql/npgsql/issues/2703")]
    public async Task Too_many_parameters_throws([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
    {
        if (prepare == PrepareOrNot.Prepared && IsMultiplexing)
            return;

        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand { Connection = conn };
        var sb = new StringBuilder("SOME RANDOM SQL ");
        for (var i = 0; i < ushort.MaxValue + 1; i++)
        {
            var paramName = "p" + i;
            cmd.Parameters.Add(new NpgsqlParameter(paramName, 8));
            if (i > 0)
                sb.Append(", ");
            sb.Append('@');
            sb.Append(paramName);
        }
        cmd.CommandText = sb.ToString();

        if (prepare == PrepareOrNot.Prepared)
        {
            Assert.That(() => cmd.Prepare(), Throws.Exception
                .InstanceOf<NpgsqlException>()
                .With.Message.EqualTo("A statement cannot have more than 65535 parameters"));
        }
        else
        {
            Assert.That(() => cmd.ExecuteNonQueryAsync(), Throws.Exception
                .InstanceOf<NpgsqlException>()
                .With.Message.EqualTo("A statement cannot have more than 65535 parameters"));
        }
    }

    //[Test, Description("An individual statement cannot have more than 65535 parameters, but a command can (across multiple statements).")]
    //[IssueLink("https://github.com/npgsql/npgsql/issues/1199")]
    public async Task Many_parameters_across_statements()
    {
        // Create a command with 1000 statements which have 70 params each
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand { Connection = conn };
        var paramIndex = 0;
        var sb = new StringBuilder();
        for (var statementIndex = 0; statementIndex < 1000; statementIndex++)
        {
            if (statementIndex > 0)
                sb.Append("; ");
            sb.Append("SELECT ");
            var startIndex = paramIndex;
            var endIndex = paramIndex + 70;
            for (; paramIndex < endIndex; paramIndex++)
            {
                var paramName = "p" + paramIndex;
                cmd.Parameters.Add(new NpgsqlParameter(paramName, 8));
                if (paramIndex > startIndex)
                    sb.Append(", ");
                sb.Append('@');
                sb.Append(paramName);
            }
        }

        cmd.CommandText = sb.ToString();
        await cmd.ExecuteNonQueryAsync();
    }

    //[Test] TODO
     //Description("Makes sure that Npgsql doesn't attempt to send all data before the user can start reading. That would cause a deadlock.")]
    public async Task Batched_big_statements_do_not_deadlock()
    {
        // We're going to send a large multistatement query that would exhaust both the client's and server's
        // send and receive buffers (assume 64k per buffer).
        var data = new string('x', 1024);
        using var conn = await OpenConnectionAsync();
        var sb = new StringBuilder();
        for (var i = 0; i < 500; i++)
            sb.Append("SELECT ?;");
        using var cmd = new NpgsqlCommand(sb.ToString(), conn);
        cmd.Parameters.AddWithValue("p", DbType.String, data);
        using var reader = await cmd.ExecuteReaderAsync();
        for (var i = 0; i < 500; i++)
        {
            reader.Read();
            Assert.That(reader.GetString(0), Is.EqualTo(data));
            reader.NextResult();
        }
    }

    //[Test]
    public void Batched_small_then_big_statements_do_not_deadlock_in_sync_io()
    {
        if (IsMultiplexing)
            return; // Multiplexing, sync I/O

        // This makes sure we switch to async writing for batches, starting from the 2nd statement at the latest.
        // Otherwise, a small first first statement followed by a huge big one could cause us to deadlock, as we're stuck
        // synchronously sending the 2nd statement while PG is stuck sending the results of the 1st.
        using var conn = OpenConnection();
        var data = new string('x', 5_000_000);
        using var cmd = new NpgsqlCommand("SELECT generate_series(1, 500000); SELECT @p", conn);
        cmd.Parameters.AddWithValue("p", NpgsqlDbType.String, data);
        cmd.ExecuteNonQuery();
    }

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/1429")]
    public async Task Same_command_different_param_values()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT CONVERT(@p,INTEGER)", conn);
        cmd.Parameters.AddWithValue("p", 8);
        await cmd.ExecuteNonQueryAsync();

        cmd.Parameters[0].Value = 9;
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(9));
    }

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/1429")]
    public async Task Same_command_different_param_instances()
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT CONVERT(@p,INTEGER)", conn);
        cmd.Parameters.AddWithValue("p", 8);
        await cmd.ExecuteNonQueryAsync();

        cmd.Parameters.RemoveAt(0);
        cmd.Parameters.AddWithValue("p", 9);
        Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(9));
    }

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/4134")]
    public async Task Cached_command_double_dispose()
    {
        await using var conn = await OpenConnectionAsync();

        var cmd1 = conn.CreateCommand();
        cmd1.Dispose();
        cmd1.Dispose();

        var cmd2 = conn.CreateCommand();
        Assert.That(cmd2, Is.Not.SameAs(cmd1));

        cmd2.CommandText = "SELECT 1";
        Assert.That(await cmd2.ExecuteScalarAsync(), Is.EqualTo(1));
    }
    
}
