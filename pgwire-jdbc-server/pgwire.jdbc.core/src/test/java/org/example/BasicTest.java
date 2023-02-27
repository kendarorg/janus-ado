package org.example;

import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.IOException;
import java.sql.*;
import java.util.concurrent.ExecutionException;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class BasicTest {
    public static final String POSTGRES_FAKE_CONNECTION_STRING = "jdbc:postgresql://localhost/test?" +
            "user=fred&" +
            "password=secret&" +
            "ssl=false";

    private static Connection conn;

    public String getH2Connection(){
        return "jdbc:h2:mem:test;";
    }

    @BeforeAll
    static void beforeAll() throws InterruptedException, ClassNotFoundException, SQLException {
        Class.forName("org.postgresql.Driver");
        conn = DriverManager.
                getConnection("jdbc:h2:mem:test;", "sa","sa");
        var th = new Thread(()-> {
            try {
                PgWireFakeServer.start(()->{
                    try {
                        return DriverManager.
                                getConnection("jdbc:h2:mem:test;", "sa","sa");
                    } catch (SQLException e) {
                        throw new RuntimeException(e);
                    }
                });
            } catch (IOException e) {

            } catch (InterruptedException e) {

            } catch (ExecutionException e) {

            }
        });
        th.start();
        Thread.sleep(1000);
    }

    @AfterAll
    public static void afterAll() throws IOException {
        PgWireFakeServer.stop();
    }

    @Test
    void testSimple() throws InterruptedException, SQLException, IOException, ClassNotFoundException {

        String url = POSTGRES_FAKE_CONNECTION_STRING;
        try(Connection conn = DriverManager.getConnection(url)) {

            conn.createStatement().execute("create table if not exists test1(id int, name varchar)");
            conn.createStatement().execute("insert into test1 values(1,'fuffa')");
            conn.createStatement().execute("insert into test1 values(2,'faffa')");
            var result = conn.createStatement().executeQuery("select * FROM test1");
            assertTrue(result.next());
            assertEquals(1, result.getInt(1));
            assertEquals("fuffa", result.getString(2));
            assertTrue(result.next());
            assertEquals(2, result.getInt(1));
            assertEquals("faffa", result.getString(2));

            conn.createStatement().execute("drop table test1");
        }
    }

    @Test
    void testPerparedStatement() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test(id int, name varchar)");
        conn.createStatement().execute("insert into test values(1,'fuffa')");
        conn.createStatement().execute("insert into test values(2,'faffa')");
        var ps = conn.prepareStatement("select * FROM test where id=?");
        ps.setString(1,"1");
        var result = ps.executeQuery();
        while(result.next()){
            System.out.println(result.getInt(1)+"-"+result.getString(2));
        }

        conn.createStatement().execute("drop table test");
    }



    @Test
    void testCall() throws InterruptedException, SQLException, IOException, ClassNotFoundException {

        //conn.prepareStatement("create table test(id int)").execute();
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        var sp = "CREATE ALIAS IP_ADDRESS AS '\n" +
                "@CODE\n" +
                "String getString() throws Exception {\n" +
                "    return \"TEST\";\n" +
                "}\n" +
                "';";

        conn.createStatement().execute(sp);
        var ps = conn.prepareCall("CALL IP_ADDRESS()");
        var result = ps.executeQuery();
        assertTrue(result.next());
        assertEquals("TEST",result.getString(1));

        conn.createStatement().execute("drop ALIAS IP_ADDRESS");
    }


    void getDbMetadata() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        var res = conn.getMetaData();
        var rs = res.getCatalogs();
        while(rs.next()){
            var catalog = rs.getString(1);
            var rst =res.getTables(catalog, null, "%", null);
            while(rst.next()){
                var tb = rst.getString(1);
            }

        }
        System.out.println(res.getCatalogs());
    }

    @Test
    void testDouble() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        try(Connection conn = DriverManager.getConnection(url)) {

            conn.createStatement().execute("create table if not exists test4(id int, VAL REAL)");
            conn.createStatement().execute("insert into test4 values(1,1.72)");
            conn.createStatement().execute("insert into test4 values(2,3.75)");
            var ps = conn.prepareStatement("select * FROM test4 where VAL=?");
            ps.setFloat(1, 1.72F);
            var result = ps.executeQuery();
            assertTrue(result.next());
            assertEquals(1, result.getInt(1));
            assertEquals(1.72F, result.getFloat(2));

            conn.createStatement().execute("drop table test4");
        }
    }

    @Test
    void testNpg() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        try(Connection conn = DriverManager.getConnection(url)) {


            var ps = conn.prepareStatement("SELECT ?");
            ps.setInt(1, 4);
            var result = ps.executeQuery();
            assertTrue(result.next());
            assertEquals(4, result.getInt(1));

            conn.createStatement().execute("drop table test4");
        }
    }

    @Test
    void testDate() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test3(id int, val DATE)");
        conn.createStatement().execute("insert into test3 values(1,'2020-12-25')");
        conn.createStatement().execute("insert into test3 values(2,'2021-12-25')");
        var ps = conn.prepareStatement("select * FROM test3 where val=?");
        ps.setDate(1,Date.valueOf("2020-12-25"));
        var result = ps.executeQuery();
        assertTrue(result.next());
        assertEquals(1,result.getInt(1));
        assertEquals("2020-12-25",result.getDate(2).toString());

        conn.createStatement().execute("drop table test3");
    }

    @Test
    void testPerparedStatemen2t() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test(id int, name varchar)");

        conn.createStatement().execute("insert into test values(1,'fuffa')");
        conn.createStatement().execute("insert into test values(2,'faffa')");
        conn.createStatement().execute("insert into test values(3,'faffa')");
        conn.createStatement().execute("insert into test values(4,'fuffa')");
        var ps = conn.prepareStatement("select * FROM test where name='fuffa'");
        var result = ps.executeQuery();
        result.next();
        System.out.println(result.getInt(1)+"-"+result.getString(2));


        var ps2 = conn.prepareStatement("select * FROM test where name='faffa'");
        var result2 = ps2.executeQuery();
        result2.next();
        System.out.println(result2.getInt(1)+"-"+result2.getString(2));

        conn.createStatement().execute("drop table test");
    }


    @Test
    void stupidTest() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        var ps = conn.prepareStatement("select 1");
        var result = ps.executeQuery();
        result.next();
        System.out.println(result.getInt(1));



    }
}
