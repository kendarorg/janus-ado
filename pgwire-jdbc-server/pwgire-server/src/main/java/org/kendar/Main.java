package org.kendar;

import org.example.PgWireFakeServer;

import java.io.IOException;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.concurrent.ExecutionException;

public class Main {
    public static void main(String[] args) throws IOException, ExecutionException, InterruptedException {
        PgWireFakeServer.start(()->{
            try {
                return DriverManager.
                        getConnection("jdbc:h2:mem:test;", "sa","sa");
            } catch (SQLException e) {
                throw new RuntimeException(e);
            }
        });
    }
}