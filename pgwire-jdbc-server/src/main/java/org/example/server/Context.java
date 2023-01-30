package org.example.server;

import org.example.messages.PGClientMessage;
import org.example.messages.PGServerMessage;

import java.sql.Connection;
import java.util.concurrent.Future;
import java.util.function.Function;
import java.util.function.Predicate;

public interface Context {
    Future<Integer> write(PGServerMessage readyForQuery);

    void add(PGClientMessage pgClientMessage);
    PGClientMessage get(Predicate<PGClientMessage> test);
    Connection getConnection();
}
