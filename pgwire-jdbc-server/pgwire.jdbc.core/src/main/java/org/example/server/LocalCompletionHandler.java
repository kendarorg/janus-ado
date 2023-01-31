package org.example.server;

import org.example.messages.*;
import org.example.messages.extendedquery.ParseMessage;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.channels.AsynchronousServerSocketChannel;
import java.nio.channels.AsynchronousSocketChannel;
import java.nio.channels.CompletionHandler;
import java.sql.Connection;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.concurrent.Future;
import java.util.function.Predicate;
import java.util.function.Supplier;

public class LocalCompletionHandler implements CompletionHandler<Integer,ByteBuffer>, Context {


    private AsynchronousSocketChannel client;
    private AsynchronousServerSocketChannel sockServer;
    private Connection conn;
    private List<PGClientMessage> storage=new ArrayList<>();

    public LocalCompletionHandler(AsynchronousSocketChannel client, AsynchronousServerSocketChannel sockServer, Supplier<Connection> conn) {

        this.client = client;
        this.sockServer = sockServer;
        this.conn = conn.get();
    }

    @Override
    public void completed(Integer result, ByteBuffer attachment) {
        attachment.flip();
        if (result != -1) {
            onMessageReceived(client, attachment);
        }
        attachment.clear();
        client.read(attachment, attachment, this);
    }

    @Override
    public void failed(Throwable exc, ByteBuffer attachment) {
//        System.err.println("[SERVER] Failed to read from client: " + exc);
//        exc.printStackTrace();
    }

    private static List<PGClientMessage> messages;

    static{
        messages = new ArrayList<>();
        messages.add(new SSLNegotation());
        messages.add(new StartupMessage());
        messages.add(new QueryMessage());
        messages.add(new ParseMessage());
        messages.add(new TerminateMessage());
        messages.add(new ExecuteMessage());
    }



    private void onMessageReceived(AsynchronousSocketChannel client, ByteBuffer buffer) {
        //System.out.println("[SERVER] Received message from client: " + client);
        //System.out.println("[SERVER] Buffer: " + buffer);

        PGClientMessage lastMessage=null;

        try{
            var shouldCloseConnection = false;
            var matchingCount = 0;
            var timeout = new Date().getTime();
            while(buffer.hasRemaining() && !shouldCloseConnection) {
                for (var msg : messages) {
                    if (msg.isMatching(buffer)) {

                        System.out.println("[SERVER] Received: " + msg.getClass().getSimpleName());
                        if (msg instanceof TerminateMessage) {
                            shouldCloseConnection = true;
                            break;
                        } else {
                            var decoded = msg.decode(buffer);
                            lastMessage= (PGClientMessage) decoded;
                            decoded.handle(this);
                            matchingCount++;
                            break;
                        }
                    }
                }
                if(timeout< (new Date().getTime()-2000)){
                    //shouldCloseConnection=true;
                }
            }
            if(shouldCloseConnection){
                try {
                    client.close();
                } catch (Exception ex) {

                }
            }else if(matchingCount>0){
                return;
            }else{
                System.out.println("[SERVER] ERROR: " + buffer);
            }
        }catch (Exception ex){

            System.out.println("[SERVER] ERROR: " + ex.getMessage());
            try {
                client.close();
            } catch (IOException e) {
                //throw new RuntimeException(e);
            }
        }



    }

    @Override
    public Future<Integer> write(PGServerMessage message) {
        System.out.println("[SERVER] Sent: "+ message.getClass().getSimpleName());
        return client.write(message.encode());
    }

    @Override
    public void add(PGClientMessage pgClientMessage) {
        storage.add(pgClientMessage);
    }

    @Override
    public PGClientMessage get(Predicate<PGClientMessage> test) {
        PGClientMessage item = null;
        try {
            for (var st : storage) {
                if (test.test(st)) {
                    item = st;
                    return st;
                }
            }
            return null;
        }finally {
            storage.remove(item);
        }
    }

    public Connection getConnection() {
        return conn;
    }
}
