## Scope

This repo contains:

* Postgres Wire compatible server able to use arbitrary JDBC driver. E.G. if you installed the H2 driver you can send H2 dialect queries
* ADO Data provider for the aforementioned server
* Some compatibility tests to use the standard ODBC Postgres driver with the server

In the future

* EF drivers
* NHibernate dialects

## Based on

* https://segmentfault.com/a/1190000017136059
* https://gavinray97.github.io/blog/postgres-wire-protocol-jdk-21
* https://www.postgresql.org/docs/current/protocol-message-formats.html
* https://www.postgresql.org/docs/current/protocol-error-fields.html
* https://www.postgresql.org/message-id/AANLkTikkkxN+-UUiGVTzj8jdfS4PdpB8_tDONMFHNqHk@mail.gmail.com

## How it works

For testing it, first install (if you are interested the Postgres SQL ODBC driver)

### Jdbc

* Just open the jdbc-server project and run the relevant tests

### Ado 

* Start the main for the JDBC server
* Run the tests

### ODBC

* Start the main for the JDBC server
* Run the tests
