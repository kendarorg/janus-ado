package org.kendar.pgwire;

import org.kendar.pgwire.utils.ConsoleOut;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.sql.Connection;
import java.util.function.Supplier;

public class PgwJdbcBridge {

    public static void start(Supplier<Connection> connectionSupplier, int maxTimeout, int port){
        ServerSocket server = null;

        try {

            // server is listening on port 1234
            server = new ServerSocket(port);
            server.setReuseAddress(true);

            // running infinite loop for getting
            // client request
            while (true) {

                // socket object to receive incoming client
                // requests
                Socket client = server.accept();

                // Displaying that new client is connected
                // to server
                ConsoleOut.println("[SERVER] Connect: "
                        + client.getInetAddress()
                        .getHostAddress());

                // create a new thread object
                var clientSock
                        = new PgwSocketHandler(client,connectionSupplier);

                // This thread will handle the client
                // separately
                new Thread(clientSock).start();
            }
        }
        catch (IOException e) {
            e.printStackTrace();
        }
        finally {
            if (server != null) {
                try {
                    server.close();
                }
                catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }
}
