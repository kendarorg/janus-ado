package org.example;

import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.IOException;
import java.sql.*;
import java.util.Calendar;
import java.util.concurrent.ExecutionException;

public class TestServer {
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

    ///@Test
    void simpleFakeQuery() throws InterruptedException, SQLException, IOException, ClassNotFoundException {



        PgWireFakeServer.setUseFakeResponse(true);
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test  (id int)");
        Statement st = conn.createStatement();
        ResultSet rs = st.executeQuery("select 1;");
        while (rs.next()) {
            System.out.println("Data "+rs.getInt(1)+", "+rs.getString(2));
        }
        conn.createStatement().execute("drop table test");
        rs.close();
        st.close();
    }



    @Test
    void simpleParamsQuery() throws InterruptedException, SQLException, IOException, ClassNotFoundException {

        PgWireFakeServer.setUseFakeResponse(true);

        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        PreparedStatement st = conn.prepareStatement("select * from people where firstname=?");
        st.setString(1,"TEST");
        ResultSet rs = st.executeQuery();
        while (rs.next()) {
            System.out.println("Data "+rs.getInt(1)+", "+rs.getString(2));
        }
        rs.close();
        st.close();
    }

    @Test
    void testRealTable() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        PgWireFakeServer.setUseFakeResponse(false);
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test(id int, name varchar)");
        conn.createStatement().execute("insert into test values(1,'fuffa')");
        conn.createStatement().execute("insert into test values(2,'faffa')");
        var result = conn.createStatement().executeQuery("select * FROM test");
        while(result.next()){
            System.out.println(result.getInt(1)+"-"+result.getString(2));
        }

        conn.createStatement().execute("drop table test");
    }

    @Test
    void testPerparedStatementTable() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        PgWireFakeServer.setUseFakeResponse(false);
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
        PgWireFakeServer.setUseFakeResponse(false);
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
        while(result.next()){
            System.out.println(result.getString(1));
        }

        conn.createStatement().execute("drop ALIAS IP_ADDRESS");
    }


    void getDbMetadata() throws InterruptedException, SQLException, IOException, ClassNotFoundException {

        PgWireFakeServer.setUseFakeResponse(false);

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
    void testRealTableDouble() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        PgWireFakeServer.setUseFakeResponse(false);
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test(id int, val REAL)");
        conn.createStatement().execute("insert into test values(1,1.72)");
        conn.createStatement().execute("insert into test values(2,3.75)");
        var ps = conn.prepareStatement("select * FROM test where val=?");
        ps.setFloat(1,1.72F);
        var result = ps.executeQuery();
        while(result.next()){
            System.out.println(result.getInt(1)+"-"+result.getFloat(2));
        }

        conn.createStatement().execute("drop table test");
    }

    @Test
    void testRealTableLocalDate() throws InterruptedException, SQLException, IOException, ClassNotFoundException {


        //conn.prepareStatement("create table test(id int)").execute();
        PgWireFakeServer.setUseFakeResponse(false);
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);

        conn.createStatement().execute("create table if not exists test(id int, val DATE)");
        conn.createStatement().execute("insert into test values(1,'2020-12-25')");
        conn.createStatement().execute("insert into test values(2,'2021-12-25')");
        var ps = conn.prepareStatement("select * FROM test where val=?");
        ps.setDate(1,Date.valueOf("2020-12-25"));
        var result = ps.executeQuery();
        while(result.next()){
            System.out.println(result.getInt(1)+"-"+result.getDate(2));
        }

        conn.createStatement().execute("drop table test");
    }
}
