package org.kendar.pgwire;

import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.sql.*;

public class PgSqlTest {
    private static Thread mainThread;

    public static final String POSTGRES_FAKE_CONNECTION_STRING = "jdbc:postgresql://localhost:5959/test?" +
            "user=fred&" +
            "password=secret&" +
            "ssl=false";

    @AfterAll
    static void afterAll(){
        mainThread.stop();
    }

    @BeforeAll
    static void beforeAll(){
        var postgresPort = 5959;
        var maxTimeout = 120000;
        mainThread = new Thread(()->PgwJdbcBridge.start(()->{
            try {
                var conn = DriverManager.
                        getConnection("jdbc:postgresql://localhost:5432/","postgres","postgres");
                //Statement stat = conn.createStatement();
                //stat.execute("CREATE ALIAS IF NOT EXISTS pg_sleep FOR 'java.lang.Thread.sleep(long)'");
                return conn;
            } catch (Exception e) {
                throw new RuntimeException(e);
            }
        },maxTimeout,postgresPort));
        mainThread.start();
    }



   /* @Test
    void selectQuestionOnWire() throws SQLException {
        String url = POSTGRES_FAKE_CONNECTION_STRING;
        Connection conn = DriverManager.getConnection(url);
        PreparedStatement ps = conn.prepareStatement("SELECT ?");
        ps.setInt(1,2);
        ps.execute();
        var rs = ps.getResultSet();
        var md = rs.getMetaData();
        System.out.println(md.getColumnClassName(1));
    }


    @Test
    void selectQuestionOnReal() throws SQLException {
        var conn = DriverManager.
                getConnection("jdbc:postgresql://localhost:5432/","postgres","postgres");

        PreparedStatement ps = conn.prepareStatement("SELECT ?");
        ps.setInt(1,2);
        ps.execute();
        var rs = ps.getResultSet();
        var md = rs.getMetaData();
        System.out.println(md.getColumnClassName(1));
    }*/
}
