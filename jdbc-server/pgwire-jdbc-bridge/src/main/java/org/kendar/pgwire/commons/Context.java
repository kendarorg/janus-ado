package org.kendar.pgwire.commons;

import java.sql.Connection;
import java.sql.SQLException;

public interface Context {
    PgwByteBuffer getBuffer();

    boolean inTransaction();
    void setTransaction(boolean val);

    void put(String key, Object object);

    Object get(String key);

    Connection getConnection();

    DataMessage waitFor(char s) throws SQLException;

    void close();

    void setJanus(boolean janus);
    boolean isJanus();
}
