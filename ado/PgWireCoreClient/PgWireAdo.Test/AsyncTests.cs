using PgWireAdo.Test.Utils;
using static PgWireAdo.Test.Utils.TestUtils;

namespace PgWireAdo.Test
{
    public class AsyncTests:TestBase
    {
        [Test]
        public async Task NonQuery()
        {
            await using var conn = await OpenConnectionAsync();
            var tableName = await CreateTempTable(conn, "intf int");
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {tableName} (intf) VALUES (4)";
            await cmd.ExecuteNonQueryAsync();
            Assert.That(await conn.ExecuteScalarAsync($"SELECT intf FROM {tableName}"), Is.EqualTo(4));
        }
    }
}
