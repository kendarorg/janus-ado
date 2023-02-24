package org.example.server;

import org.example.messages.PGClientMessage;
import org.example.messages.PGServerMessage;

import java.sql.Connection;
import java.util.concurrent.Future;
import java.util.function.Function;
import java.util.function.Predicate;

public interface Context {
    Future<Integer> write(PGServerMessage readyForQuery,Future<Integer> prev);

    void add(Object pgClientMessage);
    Object get(Predicate<Object> test);
    Connection getConnection();
}
