using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using PgWireAdo.utils;
using NpgsqlCommand = PgWireAdo.ado.PgwCommand;
using PostgresException = PgWireAdo.ado.PgwException;
using NpgsqlException = PgWireAdo.ado.PgwException;
using MultiplexingTestBase = PgWireAdo.Test.Utils.TestBase;
using NpgsqlDbType = System.Data.DbType;

namespace Npgsql.Tests.Types;

public class NumericTests : MultiplexingTestBase
{
    static readonly object[] ReadWriteCases = new[]
    {
        new object[] { "CONVERT(0.0000000000000000000000000001,DOUBLE PRECISION)", 0.0000000000000000000000000001M },
        new object[] { "CONVERT(0.000000000000000000000001,DOUBLE PRECISION)", 0.000000000000000000000001M },
        new object[] { "CONVERT(0.00000000000000000001,DOUBLE PRECISION)", 0.00000000000000000001M },
        new object[] { "CONVERT(0.0000000000000001,DOUBLE PRECISION)", 0.0000000000000001M },
        new object[] { "CONVERT(0.000000000001,DOUBLE PRECISION)", 0.000000000001M },
        new object[] { "CONVERT(0.00000001,DOUBLE PRECISION)", 0.00000001M },
        new object[] { "CONVERT(0.0001,DOUBLE PRECISION)", 0.0001M },
        new object[] { "CONVERT(1,DECIMAL)", 1M },
        new object[] { "CONVERT(10000,DECIMAL)", 10000M },
        new object[] { "CONVERT(100000000,DECIMAL)", 100000000M },
        new object[] { "CONVERT(1000000000000,DOUBLE PRECISION)", 1000000000000M },
        new object[] { "CONVERT(10000000000000000,DOUBLE PRECISION)", 10000000000000000M },
        new object[] { "CONVERT(100000000000000000000,DOUBLE PRECISION)", 100000000000000000000M },
        new object[] { "CONVERT(1000000000000000000000000,DOUBLE PRECISION)", 1000000000000000000000000M },
        new object[] { "CONVERT(10000000000000000000000000000,DOUBLE PRECISION)", 10000000000000000000000000000M },

        new object[] { "CONVERT(1E-28,DOUBLE PRECISION)", 0.0000000000000000000000000001M },
        new object[] { "CONVERT(1E-24,DOUBLE PRECISION)", 0.000000000000000000000001M },
        new object[] { "CONVERT(1E-20,DOUBLE PRECISION)", 0.00000000000000000001M },
        new object[] { "CONVERT(1E-16,DOUBLE PRECISION)", 0.0000000000000001M },
        new object[] { "CONVERT(1E-12,DOUBLE PRECISION)", 0.000000000001M },
        new object[] { "CONVERT(1E-8,DOUBLE PRECISION)", 0.00000001M },
        new object[] { "CONVERT(1E-4,DOUBLE PRECISION)", 0.0001M },
        new object[] { "CONVERT(1E+0,DECIMAL)", 1M },
        new object[] { "CONVERT(1E+4,DECIMAL)", 10000M },
        new object[] { "CONVERT(1E+8,DECIMAL)", 100000000M },
        new object[] { "CONVERT(1E+12,DOUBLE PRECISION)", 1000000000000M },
        new object[] { "CONVERT(1E+16,DOUBLE PRECISION)", 10000000000000000M },
        new object[] { "CONVERT(1E+20,DOUBLE PRECISION)", 100000000000000000000M },
        new object[] { "CONVERT(1E+24,DOUBLE PRECISION)", 1000000000000000000000000M },
        new object[] { "CONVERT(1E+28,DOUBLE PRECISION)", 10000000000000000000000000000M },

        new object[] { "CONVERT(11.222233334444555566667777888,DECIMAL(30,28))", 11.222233334444555566667777888M },
        new object[] { "CONVERT(111.22223333444455556666777788,DECIMAL(30,27))", 111.22223333444455556666777788M },
        new object[] { "CONVERT(1111.2222333344445555666677778,DECIMAL(30,26))", 1111.2222333344445555666677778M },

        //new object[] { "+79228162514264337593543950335,DECIMAL)", +79228162514264337593543950335M },
        new object[] { "CONVERT(+79228162514264337593543950335,DECIMAL)", +79228162514264337593543950335M },
        new object[] { "CONVERT(-79228162514264337593543950335,DECIMAL)", -79228162514264337593543950335M },

        // It is important to test rounding on both even and odd
        // numbers to make sure midpoint rounding is away from zero.
        new object[] { "CONVERT(1,NUMERIC(10,2))", 1.00M },
        new object[] { "CONVERT(2,NUMERIC(10,2))", 2.00M },

        new object[] { "CONVERT(1.2,NUMERIC(10,1))", 1.2M },
        new object[] { "CONVERT(1.2,NUMERIC(10,2))", 1.20M },
        new object[] { "CONVERT(1.2,NUMERIC(10,3))", 1.200M },
        new object[] { "CONVERT(1.2,NUMERIC(10,4))", 1.2000M },
        new object[] { "CONVERT(1.2,NUMERIC(10,5))", 1.20000M },

        new object[] { "CONVERT(1.4,NUMERIC(10,0))", 1M },
        new object[] { "CONVERT(1.5,NUMERIC(10,0))", 2M },
        new object[] { "CONVERT(2.4,NUMERIC(10,0))", 2M },
        new object[] { "CONVERT(2.5,NUMERIC(10,0))", 3M },

        new object[] { "CONVERT(-1.4,NUMERIC(10,0))", -1M },
        new object[] { "CONVERT(-1.5,NUMERIC(10,0))", -2M },
        new object[] { "CONVERT(-2.4,NUMERIC(10,0))", -2M },
        new object[] { "CONVERT(-2.5,NUMERIC(10,0))", -3M },

        // Bug 2033
        //new object[] { "CONVERT(0.0036882500000000000000000000,DECIMAL)", 0.0036882500000000000000000000M },

        new object[] { "CONVERT(936490726837837729197,DECIMAL)", 936490726837837729197M },
        new object[] { "CONVERT(9364907268378377291970000,DECIMAL)", 9364907268378377291970000M },
        new object[] { "CONVERT(3649072683783772919700000000,DECIMAL)", 3649072683783772919700000000M },
        new object[] { "CONVERT(1234567844445555.000000000,DECIMAL)", 1234567844445555M },
        new object[] { "CONVERT(11112222000000000000,DECIMAL)", 11112222000000000000M },
        new object[] { "CONVERT(0,DECIMAL)", 0M },
    };

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Read(string query, decimal expected)
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT " + query, conn);
        Assert.That(
            ((await cmd.ExecuteScalarAsync())!.ToString().TrimEnd('0')),
            Is.EqualTo(expected.ToString().TrimEnd('0')));
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Write(string query, decimal expected)
    {
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT @p, @p = " + query, conn);
        cmd.Parameters.AddWithValue("p", expected);
        using var rdr = await cmd.ExecuteReaderAsync();
        rdr.Read();
        Assert.That(decimal.GetBits(rdr.GetFieldValue<decimal>(0)), Is.EqualTo(decimal.GetBits(expected)));
        Assert.That(rdr.GetFieldValue<bool>(1));
    }


    //[Test] TODO NOT VALID FOR H2, Description("Tests that when Numeric value does not fit in a System.Decimal and reader is in ReaderState.InResult, the value was read wholly and it is safe to continue reading")]

    public async Task Read_overflow_is_safe()
    {
        using var conn = await OpenConnectionAsync();
        //This 29-digit number causes OverflowException. Here it is important to have unread column after failing one to leave it ReaderState.InResult
        using var cmd = new NpgsqlCommand(@"SELECT CONVERT(0.222233334444555566667777888,DECIMAL(2,20)), CONVERT(1,DECIMAL);", conn);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
        var i = 1;

        reader.Read();
        {
            Assert.That(() => reader.GetDecimal(0),
                Throws.Exception
                    .With.TypeOf<OverflowException>()
                    .With.Message.EqualTo("Numeric value does not fit in a System.Decimal"));
            var intValue = reader.GetInt32(1);

            Assert.That(intValue, Is.EqualTo(i++));
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Open | ConnectionState.Fetching));
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
        }
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Read_BigInteger(string query, decimal expected)
    {
        if (decimal.Floor(expected) == expected)
        {
            var bigInt = new BigInteger(expected);
            using var conn = await OpenConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT " + query, conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            await rdr.ReadAsync();
            Assert.That(rdr.GetFieldValue<BigInteger>(0), Is.EqualTo(bigInt));
        }
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Write_BigInteger(string query, decimal expected)
    {
        if (decimal.Floor(expected) == expected)
        {
            var bigInt = new BigInteger(expected);
            using var conn = await OpenConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT @p, @p = " + query, conn);
            cmd.Parameters.AddWithValue("p", bigInt);
            using var rdr = await cmd.ExecuteReaderAsync();
            await rdr.ReadAsync();
            Assert.That(rdr.GetFieldValue<BigInteger>(0), Is.EqualTo(bigInt));
            Assert.That(rdr.GetFieldValue<bool>(1));
        }
    }

    [Test]
    public async Task BigInteger_large()
    {
        var num = BigInteger.Parse(string.Join("", Enumerable.Range(0, 17000).Select(i => ((i + 1) % 10).ToString())));
        using var conn = await OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT CONVERT('0.1',DECIMAL), @p", conn);
        cmd.Parameters.AddWithValue("p", num);
        using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
        await rdr.ReadAsync();
        Assert.That(rdr.GetFieldValue<BigInteger>(0), Is.EqualTo(new BigInteger(0)));
        Assert.That(rdr.GetFieldValue<BigInteger>(1), Is.EqualTo(num));
    }
    
}
