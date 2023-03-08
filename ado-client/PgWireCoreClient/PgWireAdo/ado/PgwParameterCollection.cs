using System.Collections;
using System.Data.Common;

namespace PgWireAdo.ado;

public class PgwParameterCollection: DbParameterCollection
{
    private readonly Object _syncRoot = new Object();
    private readonly List<DbParameter?> _data = new();

    public List<DbParameter> Data=> _data;
    public override int Add(object value)
    {
        if (value==null)
        {
            throw new ArgumentNullException();
        }
        if (typeof(PgwParameter)!=value.GetType() && !value.GetType().IsSubclassOf(typeof(PgwParameter)))
        {
            throw new InvalidCastException();
        }
        _data.Add((DbParameter)value);
        return _data.Count;
    }

    public override void Insert(int index, object value)
    {
        
        while (_data.Count <= index)
        {
            _data.Add(null);
        }
        if (typeof(PgwParameter) == value.GetType() || value.GetType().IsSubclassOf(typeof(PgwParameter)))
        {
            _data.Insert(index, (PgwParameter)value);
        }
        else
        {
            _data.Insert(index, new PgwParameter(value));
        }
        
    }

    public override void Clear()
    {
        _data.Clear();
    }

    /*new DbParameter this[String i]
    {
        get { return GetParameter(i); }
        set
        {
            throw new InvalidOperationException();
            if (!EqualStr(value.ParameterName,i))
            {
                throw new ArgumentException();
            }
            SetParameter(i,value);
        }
    }*/


    public override bool Contains(object value)
    {
        var result = _data.Find(a => value.Equals(a.Value));
        return result != null;
    }

    public override int IndexOf(object value)
    {
        if (value is PgwParameter)
        {
            var idx = _data.FindIndex(a => value.Equals(a.Value));
            return idx;
        }
        else
        {
            return IndexOf((String)value);
        }
    }

    private String CleanString(String parameterName)
    {
        if (parameterName.StartsWith(":") || parameterName.StartsWith("@") || parameterName.StartsWith("$"))
        {
            parameterName = parameterName.Substring(1);
        }
        return parameterName;
    }

    private bool EqualStr(String first,String second)
    {
        first = CleanString(first).ToLowerInvariant();
        second = CleanString(second).ToLowerInvariant();
        return first == second;
    }

    public override int IndexOf(string parameterName)
    {
        var idx = _data.FindIndex(a =>
                EqualStr(parameterName,a.ParameterName));
            return idx;
    }

    

    public override void Remove(object value)
    {
        var idx = _data.FindIndex(a => value.Equals(a));
        _data.RemoveAt(idx);
    }

    public override void RemoveAt(int index)
    {
        _data.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        var idx = _data.FindIndex(a => EqualStr(a.ParameterName,parameterName));
        if (idx != -1)
        {
            _data.RemoveAt(idx);
        }
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        while (_data.Count <= index)
        {
            _data.Add(null);
        }
        _data.Insert(index, value);
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        if (!EqualStr(value.ParameterName, parameterName))
        {
            throw new ArgumentException();
        }
        var idx = _data.FindIndex(a =>EqualStr(a.ParameterName,parameterName));
        if (idx != -1)
        {
            _data[idx]=value;
        }
    }

    public override int Count => _data.Count;
    public override object SyncRoot => _syncRoot;

    

    public override bool Contains(string value)
    {
        return _data.FindIndex(a =>EqualStr( a.ParameterName, value))>=0;
    }

    public override void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    protected override DbParameter GetParameter(int index)
    {
        return _data[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return _data.Find(a => EqualStr(parameterName,a.ParameterName));
    }

    public override void AddRange(Array values)
    {
        throw new NotImplementedException();
    }

    public PgwParameterCollection Clone()
    {
        var result = new PgwParameterCollection();
        foreach (var src in _data)
        {
            result.Data.Add(new PgwParameter()
            {
                Value = src.Value,
                DbType = src.DbType,
                Direction = src.Direction,
                IsNullable = src.IsNullable,
                Precision = src.Precision,
                Scale = src.Scale,
                Size = src.Size,
                ParameterName = src.ParameterName,
                SourceColumn = src.SourceColumn,
                SourceColumnNullMapping = src.SourceColumnNullMapping,
                SourceVersion = src.SourceVersion
            });
        }
        return result;
    }
}