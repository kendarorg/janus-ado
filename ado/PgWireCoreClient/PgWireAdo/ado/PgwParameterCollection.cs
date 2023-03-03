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
        _data.Add((DbParameter)value);
        return _data.Count;
    }

    public override void Clear()
    {
        _data.Clear();
    }

    new DbParameter this[String i]
    {
        get { return GetParameter(i); }
        set { SetParameter(i,value); }
    }

    public override bool Contains(object value)
    {
        var result = _data.Find(a => value.Equals(a.Value));
        return result != null;
    }

    public override int IndexOf(object value)
    {
        var idx = _data.FindIndex(a => value.Equals(a.Value));
        return idx;
    }

    public override void Insert(int index, object value)
    {
        while (_data.Count <= index)
        {
            _data.Add(null);
        }
        _data.Insert(index, new PgwParameter(value));
    }

    public override void Remove(object value)
    {
        var idx = _data.FindIndex(a => value.Equals(a.Value));
        _data.RemoveAt(idx);
    }

    public override void RemoveAt(int index)
    {
        _data.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        var idx = _data.FindIndex(a => a.ParameterName!=null && a.ParameterName == parameterName);
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
        var idx = _data.FindIndex(a => a.ParameterName != null && a.ParameterName == parameterName);
        if (idx != -1)
        {
            _data[idx]=value;
        }
    }

    public override int Count => _data.Count;
    public override object SyncRoot => _syncRoot;

    public override int IndexOf(string parameterName)
    {
        return _data.FindIndex(a => a.ParameterName != null && a.ParameterName == parameterName);
    }

    public override bool Contains(string value)
    {
        return _data.FindIndex(a => a.ParameterName != null && a.ParameterName == value)>=0;
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
        if (parameterName.StartsWith(":") || parameterName.StartsWith("@"))
        {
            parameterName = parameterName.Substring(1);
        }
        return _data.Find(a => parameterName ==a.ParameterName ||
                               ":" + parameterName == a.ParameterName ||
                               "@" + parameterName == a.ParameterName);
    }

    public override void AddRange(Array values)
    {
        throw new NotImplementedException();
    }
}