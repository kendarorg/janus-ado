package org.kendar.pgwire;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.flow.*;
import org.kendar.pgwire.initialize.SSLNegotation;
import org.kendar.pgwire.initialize.StartupMessage;
import org.kendar.pgwire.server.ErrorResponse;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.utils.ConsoleOut;

import java.io.BufferedReader;
import java.io.EOFException;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Supplier;

public class PgwSocketHandler implements Runnable, Context {
    private static AtomicInteger pidCounter  = new AtomicInteger(1);
    private AtomicBoolean running  = new AtomicBoolean(true);
    private final Socket clientSocket;
    private Supplier<Connection> connectionSupplier;
    private final ConcurrentLinkedQueue<DataMessage> inputQueue = new ConcurrentLinkedQueue<>();

    private Thread responder;
    private Connection connection;

    public PgwByteBuffer getBuffer() {
        return buffer;
    }

    private boolean transaction = false;
    @Override
    public boolean inTransaction() {
        return transaction;
    }

    @Override
    public void setTransaction(boolean val) {
        this.transaction = val;
    }

    private final Map<String,Object> cache = new ConcurrentHashMap<>();
    @Override
    public void put(String key, Object object) {
        if(object==null){
            cache.remove(key.toLowerCase(Locale.ROOT));
        }else {
            cache.put(key.toLowerCase(Locale.ROOT), object);
        }
    }

    @Override
    public Object get(String key) {
        return cache.get(key.toLowerCase(Locale.ROOT));
    }

    @Override
    public Connection getConnection() {
        if(connection!=null)return connection;
        connection = connectionSupplier.get();
        return connection;
    }


    private PgwByteBuffer buffer;

    // Constructor
    public PgwSocketHandler(Socket socket, Supplier<Connection> connectionSupplier)
    {
        this.clientSocket = socket;
        this.connectionSupplier = connectionSupplier;
    }



    public void run()
    {
        responder = new Thread(()->respondToClient());
        responder.start();
        PrintWriter out = null;
        BufferedReader in = null;
        try {
            buffer = new PgwByteBuffer(clientSocket);
            buffer.read(new SSLNegotation());
            var startup = new StartupMessage(pidCounter.incrementAndGet());
            buffer.read(startup);
            while(running.get()){

                var messageType = buffer.readByte();
                //ConsoleOut.println("R "+(char)messageType);
                var messageLength = buffer.readInt32();
                var data = buffer.read(messageLength-4);
                var dm = new DataMessage((char)messageType,messageLength,data);
                inputQueue.add(dm);
            }
        }
        catch (EOFException e) {
            this.close();
        }
        catch (IOException e) {
            this.close();
        }
        finally {
            running.set(false);
            try {
                if (out != null) {
                    out.close();
                }
                if (in != null) {
                    in.close();
                    clientSocket.close();
                }
            }
            catch (IOException e) {

            }
        }
    }

    private static void sleep(long millis){
        try {
            Thread.sleep(millis);
        } catch (InterruptedException e) {
        }
    }

    public void respondToClient() {
        char lastFoundedType = '0';
        while (running.get()) {
            try {
                var item = inputQueue.poll();
                if (item == null) {
                    sleep(10);
                    continue;
                }
                PgwFlowMessage message = null;
                lastFoundedType = item.getType();

                switch (item.getType()) {
                    case 'P':
                        message = new ParseMessage();
                        break;
                    case 'B':
                        message = new BindMessage();
                        break;
                    case 'E':
                        message = new ExecuteMessage();
                        break;
                    case 'S':
                        message = new SyncMessage();
                        break;
                    case 'D':
                        message = new DescribeMessage();
                        break;
                    case 'X':
                        message = new TerminateMessage();
                        break;
                    case 'Q':
                        message = new QueryMessage();
                        break;
                }
                ConsoleOut.println("[SERVER] Recv: "+message.getClass().getSimpleName());
                message.read(item);
                message.handle(this);
            }catch (Exception ex){
                try {
                    setTransaction(false);
                    ex.printStackTrace();
                    ConsoleOut.println("[ERROR] On message "+lastFoundedType+" "+ex.getMessage());
                    this.getBuffer().write(new ErrorResponse(ex.getMessage()));
                    this.getBuffer().write(new ReadyForQuery(inTransaction()));
                } catch (IOException e) {

                }
                //ex.printStackTrace();
            }
        }
    }


    @Override
    public DataMessage waitFor(char s) throws SQLException {
        DataMessage dm =null;
        var now =System.currentTimeMillis()+1000;
        while(dm==null && running.get()) {
            for (var item : inputQueue) {
                if (item.getType() == s) {
                    dm = item;
                }
            }
            var after = now =System.currentTimeMillis();
            if(after>now){
                throw new SQLException("Unable to find "+s+" message");
            }
            if(dm==null)sleep(10);
        }
        if(dm!=null){
            ConsoleOut.println("[SERVER] Recv:* "+dm.getType());
            inputQueue.remove(dm);
        }
        return dm;
    }

    @Override
    public void close() {
        this.running.set(false);
        try {
            this.clientSocket.close();
            this.responder.stop();
            this.connection.close();

        } catch (Exception e) {

        }
    }
}

