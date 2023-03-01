package org.kendar;

import org.example.PgWireFakeServer;

import java.io.IOException;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.concurrent.ExecutionException;

public class Main {
    public static void main(String[] args) throws IOException, ExecutionException, InterruptedException {
        var postgresPort = 5432;
        var maxTimeout = 120000;
        //Class.forName("org.h2.Driver");
        PgwJdbcBridge.start(()->{
            try {
                var conn = DriverManager.
                        getConnection("jdbc:h2:mem:test;MODE=PostgreSQL;DATABASE_TO_LOWER=TRUE;DEFAULT_NULL_ORDERING=HIGH", "sa","sa");
                Statement stat = conn.createStatement();
                stat.execute("CREATE ALIAS IF NOT EXISTS pg_sleep FOR 'java.lang.Thread.sleep(long)'");
                return conn;
            } catch (Exception e) {
                throw new RuntimeException(e);
            }
        },maxTimeout,postgresPort);
    }
}