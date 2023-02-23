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
import java.util.Queue;
import java.util.concurrent.*;
import java.util.function.Predicate;
import java.util.function.Supplier;

public class LocalCompletionHandler implements CompletionHandler<Integer,ByteBuffer>, Context {


    private AsynchronousSocketChannel client;
    private Connection conn;
    private int maxTimeout;
    private  Queue<Object> storage=new LinkedBlockingQueue<>();

    public LocalCompletionHandler(AsynchronousSocketChannel client,
                                  Supplier<Connection> conn,int maxTimeout) {

        this.client = client;
        this.conn = conn.get();
        this.maxTimeout = maxTimeout;
    }

    @Override
    public void completed(Integer result, ByteBuffer attachment) {
        attachment.flip();
        if (result != -1) {
            onMessageReceived(client, attachment,maxTimeout);
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



    private void onMessageReceived(AsynchronousSocketChannel client, ByteBuffer buffer,int maxTimeout) {
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
                if(timeout< (new Date().getTime()-maxTimeout)){
                    shouldCloseConnection=true;
                }
            }
            if(shouldCloseConnection){
                try {

                    client.shutdownOutput();
                } catch (Exception ex) {

                }
            }else if(matchingCount>0){
                return;
            }else{
                System.out.println("[SERVER] ERROR 01: " + buffer);
            }
        }catch (Exception ex){

            System.out.println("[SERVER] ERROR 02: " + ex.getMessage());
            try {
                client.close();
            } catch (IOException e) {
                //throw new RuntimeException(e);
            }
        }



    }

    private List<ByteBuffer> buffers = new ArrayList<>();

    @Override
    public Future<Integer> write(PGServerMessage message) {
        System.out.println("[SERVER] Sent: "+ message.getClass().getSimpleName());
        var buffer = message.encode();
        try {

            return client.write(buffer);
        }catch(Exception ex){
            try {
                Thread.sleep(100);
            }catch (Exception ex2){

            }
            return client.write(buffer);

        }
    }

    @Override
    public void add(Object pgClientMessage) {
        storage.add(pgClientMessage);
    }

    @Override
    public Object get(Predicate<Object> test) {
        Object item = null;
        item = storage.peek();
        if(test.test(item)){
            storage.remove(item);
            return item;
        }
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
