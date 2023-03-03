package org.kendar;

import org.kendar.pgwire.PgwJdbcBridge;

import java.io.IOException;
import java.sql.DriverManager;
import java.sql.Statement;
import java.util.concurrent.ExecutionException;

public class Main {
    public static final String POSTGRES_REAL_CONNECTION_STRING = "jdbc:postgresql://localhost:5432/postgres?user=postgres&password=postgres&ssl=false";

    public static final String H2_MEM="jdbc:h2:mem:test;MODE=PostgreSQL;DATABASE_TO_LOWER=TRUE;DEFAULT_NULL_ORDERING=HIGH";
    public static void main(String[] args) throws IOException, ExecutionException, InterruptedException {
        var postgresPort = 5432;
        var maxTimeout = 120000;
        //Class.forName("org.h2.Driver");
        PgwJdbcBridge.start(()->{
            try {
                var conn = DriverManager.
                        getConnection(H2_MEM,
                                "sa","sa");
                Statement stat = conn.createStatement();
                stat.execute("CREATE ALIAS IF NOT EXISTS pg_sleep FOR 'java.lang.Thread.sleep(long)'");
                return conn;
            } catch (Exception e) {
                throw new RuntimeException(e);
            }
        },maxTimeout,postgresPort);
    }
}