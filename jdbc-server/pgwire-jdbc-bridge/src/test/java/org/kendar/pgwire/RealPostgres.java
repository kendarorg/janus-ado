package org.kendar.pgwire;

import org.junit.jupiter.api.Test;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;

public class RealPostgres {
    public static final String POSTGRES_REAL_CONNECTION_STRING = "jdbc:postgresql://localhost:5432/postgres?user=postgres&password=postgres&ssl=false";

    //@Test
    public void realPostgresTest() throws SQLException {
        Connection conn = DriverManager.getConnection(POSTGRES_REAL_CONNECTION_STRING);
        var st = conn.createStatement();
        var rs = st.executeQuery("select oid, typbasetype from pg_type where typname = 'lo'");
        var md = rs.getMetaData();
        for(var i=1;i<=md.getColumnCount();i++){
            System.out.println("===================");
            System.out.println((int)md.getColumnType(i));
            System.out.println(md.getColumnClassName(i));
            System.out.println(md.getColumnName(i));
            System.out.println(md.getPrecision(i));
        }
    }
}
