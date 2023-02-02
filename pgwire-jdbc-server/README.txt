START TRANSACTION;
    SELECT pg_advisory_xact_lock(0);
    DROP TABLE IF EXISTS {tableName} CASCADE;
    COMMIT;
    CREATE TABLE {tableName} ({columns});


