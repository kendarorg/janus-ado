package org.kendar.pgwire.utils;

public class TypesOids {
    public static int Int8         = 20;
    public static int Float8       = 701;
    public static int Int4         = 23;
    public static int Numeric      = 1700;
    public static int Float4       = 700;
    public static int Int2         = 21;
    public static int Money        = 790;

    // Boolean
    public static int Bool        = 16;

    // Geometric
    public static int Box         = 603;
    public static int Circle      = 718;
    public static int Line        = 628;
    public static int LSeg        = 601;
    public static int Path        = 602;
    public static int Point       = 600;
    public static int Polygon     = 604;

    // Character
    public static int BPChar      = 1042;
    public static int Text        = 25;
    public static int Varchar     = 1043;
    public static int Name        = 19;
    public static int Char        = 18;

    // Binary data
    public static int Bytea       = 17;

    // Date/Time
    public static int Date        = 1082;
    public static int Time        = 1083;
    public static int Timestamp   = 1114;
    public static int TimestampTz = 1184;
    public static int Interval    = 1186;
    public static int TimeTz      = 1266;
    public static int Abstime     = 702;

    // Network address
    public static int Inet        = 869;
    public static int Cidr        = 650;
    public static int Macaddr     = 829;
    public static int Macaddr8    = 774;

    // Bit string
    public static int Bit         = 1560;
    public static int Varbit      = 1562;

    // Text search
    public static int TsVector    = 3614;
    public static int TsQuery     = 3615;
    public static int Regconfig   = 3734;

    // UUID
    public static int Uuid        = 2950;

    // XML
    public static int Xml         = 142;

    // JSON
    public static int Json        = 114;
    public static int Jsonb       = 3802;
    public static int JsonPath    = 4072;

    // public
    public static int Refcursor   = 1790;
    public static int Oidvector   = 30;
    public static int Int2vector  = 22;
    public static int Oid         = 26;
    public static int Xid         = 28;
    public static int Xid8        = 5069;
    public static int Cid         = 29;
    public static int Regtype     = 2206;
    public static int Tid         = 27;
    public static int PgLsn       = 3220;

    // Special
    public static int Record      = 2249;
    public static int Void        = 2278;
    public static int Unknown     = 705;

    // Range types
    public static int Int4Range   = 3904;
    public static int Int8Range   = 3926;
    public static int NumRange    = 3906;
    public static int TsRange     = 3908;
    public static int TsTzRange   = 3910;
    public static int DateRange   = 3912;

    // Multirange types
    public static int Int4Multirange   = 4451;
    public static int Int8Multirange   = 4536;
    public static int NumMultirange    = 4532;
    public static int TsMultirange     = 4533;
    public static int TsTzMultirange   = 4534;
    public static int DateMultirange   = 4535;
}
