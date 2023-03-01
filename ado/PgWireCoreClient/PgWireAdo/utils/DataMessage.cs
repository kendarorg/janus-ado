﻿using System;
using System.Buffers.Binary;
using System.Text;

namespace PgWireAdo.utils;

public class DataMessage
{
    public long Timestamp { get; }
    private int _cursor = 0;
    public char Type { get; }
    public int Length { get; }
    public byte[] Data { get; }


    public DataMessage(char type, int length, byte[] data)
    {
        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Type = type;
        Length = length;
        Data = data;
    }
    

    public int ReadInt()
    {
        var result = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(Data, _cursor));
        _cursor += 4;
        return result;
    }

    public int ReadInt32()
    {
        return ReadInt();
    }

    public short GetShort()
    {
        var intArray = new byte[2];
        var result = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(Data, _cursor));
        _cursor += 2;
        return result;
    }

    public String ReadAsciiString()
    {
        var ms = new MemoryStream();

        var count = 0;
        var start = _cursor;
        for (; _cursor < Data.Length; _cursor++)
        {

            if (Data[_cursor] == 0x00)
            {
                _cursor++;
                break;
            }
            ms.WriteByte(Data[_cursor]);
            count++;
        }

        return ASCIIEncoding.Default.GetString(ms.ToArray());
    }

    public String ReadUTF8String()
    {
        var ms = new MemoryStream();

        var count = 0;
        var start = _cursor;
        for (; _cursor < Data.Length; _cursor++)
        {

            if (Data[_cursor] == 0x00)
            {
                _cursor++;
                break;
            }
            ms.WriteByte(Data[_cursor]);
            count++;
        }

        return UTF8Encoding.Default.GetString(ms.ToArray());
    }

    public byte[] ReadBytes(int parameterLength)
    {
        var result = new byte[parameterLength];
        for (var i = 0; i < parameterLength; i++, _cursor++)
        {
            result[i] = Data[_cursor];
        }
        return result;
    }

    public byte ReadByte()
    {
        var result = Data[_cursor];
        _cursor++;
        return result;
    }

    public short ReadInt16()
    {
        return GetShort();
    }
}