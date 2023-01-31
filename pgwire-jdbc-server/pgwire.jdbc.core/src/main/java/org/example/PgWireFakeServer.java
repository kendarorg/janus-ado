package org.example;

import org.example.server.LocalCompletionHandler;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.channels.AsynchronousChannelGroup;
import java.nio.channels.AsynchronousServerSocketChannel;
import java.nio.channels.AsynchronousSocketChannel;
import java.sql.Connection;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.function.Function;
import java.util.function.Supplier;

public class PgWireFakeServer {
    private static final String HOST = "localhost";
    private static final int PORT = 5432;
    private static AsynchronousServerSocketChannel sockServer;

    public static void main(String[] args) throws Exception {
        start(()->null);
    }

    public static void stop() throws IOException {
        sockServer.close();
    }

    public static void setUseFakeResponse(boolean useFakeResponse) {
        PgWireFakeServer.useFakeResponse = useFakeResponse;
    }

    public static boolean isUseFakeResponse() {
        return useFakeResponse;
    }

    static boolean useFakeResponse=false;

    public static void start(Supplier<Connection> conn) throws IOException, InterruptedException, ExecutionException {
        start(conn,2000);
    }

    public static void start(Supplier<Connection> conn,int maxTimeout) throws IOException, InterruptedException, ExecutionException {
        ExecutorService executor = Executors.newFixedThreadPool(20);
        AsynchronousChannelGroup group = AsynchronousChannelGroup.withThreadPool(executor);

        try (AsynchronousServerSocketChannel server = AsynchronousServerSocketChannel.open(group)) {
            server.bind(new InetSocketAddress(HOST, PORT));
            sockServer = server;
            System.out.println("[SERVER] Listening on " + HOST + ":" + PORT);

            for (;;) {
                try {
                    Future<AsynchronousSocketChannel> future = server.accept();
                    AsynchronousSocketChannel client = future.get();
                    System.out.println("[SERVER] Accepted connection from " + client.getRemoteAddress());
                    ByteBuffer buffer = ByteBuffer.allocate(64000);
                    client.read(buffer, buffer, new LocalCompletionHandler(client, sockServer, conn,maxTimeout));
                }catch (Exception ex){

                }
            }
        }
    }

}