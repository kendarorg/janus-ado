Based on

* https://segmentfault.com/a/1190000017136059
* https://gavinray97.github.io/blog/postgres-wire-protocol-jdk-21
* https://www.postgresql.org/docs/current/protocol-message-formats.html
* https://www.postgresql.org/docs/current/protocol-error-fields.html
* https://www.postgresql.org/message-id/AANLkTikkkxN+-UUiGVTzj8jdfS4PdpB8_tDONMFHNqHk@mail.gmail.com

## Simple test

<pre>
+ psql -h localhost -p 5432 -U postgres

psql (15.1, server 0.0.0)
WARNING: psql major version 15, server major version 0.0.
Some psql features might not work.
WARNING: Console code page (437) differs from Windows code page (1252)
8-bit characters might not work correctly. See psql reference
page "Notes for Windows users" for details.
Type "help" for help.

postgres=>

+ select 1;

id | name
----+------
1 | one
2 | two
(2 rows)


postgres=>

+ \q
</pre>

## Prepared statement

<pre>
psql -h localhost -p 5432 -U postgres
PREPARE foo(text) AS  SELECT  *   FROM    foobar WHERE   foo = $1 ;

EXECUTE foo('foo');
</pre>

## Protocol Flow

As per https://www.postgresql.org/docs/current/protocol-flow.html

### Simple Query

Unhandled:

* CopyInResponse
* CopyOutResponse

<pre>
receive_QueryMessage{
    try{
        foreach(queryStrings){
            if(queryString){
                return_RowDescritpion
                return_DataRow
                return_CommandComplete
            }else{
                return_EmptyQueryResponse
                return_CommandComplete
            }
        }
        return_ReadyForQuery
    }catch{
        return_ErrorResponse
    }
}
</pre>

## Function Call

<pre>
receive_FunctionCallMessage{
    try{
        foreach(queryStrings){
            if(queryString){
                return_RowDescritpion
                return_DataRow
                return_CommandComplete
            }else{
                return_EmptyQueryResponse
                return_CommandComplete
            }
        }
        return_ReadyForQuery
    }catch{
        return_ErrorResponse
    }
}
</pre>
