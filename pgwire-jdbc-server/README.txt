


psql -h localhost -p 5432 -U postgres
create table if not exists test(id int, name varchar);
insert into test values(1,'test1');
insert into test values(2,'test2');
insert into test values(3,'test3');insert into test values(4,'test4');
select * from test;
select * from testa;


insert into test values(3,'test3');insert into test values('what;','test4');

insert into test values(5,'test5');insert into test values('what;','test6');

postgres=> create table if not exists test(id int, name varchar);
UPDATE 0
postgres=> insert into test values(1,'test1');
INSERT 0 1
postgres=> insert into test values(2,'test2');
INSERT 0 1
postgres=> select * from test;
 id | name
----+-------
  1 | test1
  2 | test2
(2 rows)

postgres=> select * from testa;
FATAL:  org.h2.jdbc.JdbcSQLSyntaxErrorException: Table "testa" not found; SQL statement:
select * from testa
; [42102-214]
message contents do not agree with length in message type "E"
postgres=>