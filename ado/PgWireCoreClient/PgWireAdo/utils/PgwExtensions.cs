using System.Data.Common;

namespace PgWireAdo.utils;

public static class PgwExtesnions{
    public static void AddWithValue(this DbParameterCollection coll,String name,Object?value){
        throw new NotImplementedException();
    }

    public static void AddWithValue(this DbParameterCollection coll,Object?value){
        throw new NotImplementedException();
    }
}