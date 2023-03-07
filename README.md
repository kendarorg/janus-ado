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

For testing it, first install (if you are interested) the Postgres SQL ODBC driver

On the properties of the ODBC driver, set "Parse statements" to false

### Jdbc

* Just open the jdbc-server project and run the relevant tests

### Ado 

* Start the main for the JDBC server
* Run the tests

### ODBC

* Start the main for the JDBC server
* Run the tests

### Command line

* In pgsql folder you can find the command line client. To make an example just run

    psql -h localhost -p 5432 -U postgres

And then in the prompt

    create table if not exists test(id int, name varchar);
    insert into test values(1,'test1');
    insert into test values(2,'test2');
    select * from test;


## Assorted Weirdness

### ODBC

* It does some queries to optimize probably the conversions. Luckily a simple "empty answer" can solve this
* During the startup there are many properties needed by the driver. If they are not shown everything crashes. Horribly.
  * Timezone
  * Codepage
* Internal queries are used through the "Simple" query protocol
* The Bind Message seems to not contain the output parameters at all
* The flow starts with the StartupMessage

### JDBC

* The Bind Message seems to not contain the output parameters at all
* Named parameters are only a convention
* Batched queries are allowed only through use multiple statements
* The flow starts with the SSLNegotiation

### ADO, my implementation

* Named parameters are only a convention
* The "old stile batched queries" are now deprecated but they "mostly" works

