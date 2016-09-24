using System;
using System.Text;
using Utils;

namespace com.movisens.smartgattlib
{
	public class GattByteBuffer
	{
	    public static GattByteBuffer Allocate(int i)
        {
            return new GattByteBuffer(new byte[i]);
        }

		public static GattByteBuffer FromArray(byte[] byteArray)
		{
            return new GattByteBuffer(byteArray);
		}

        // Gatt Bytes come in Little Endian Order.
	    private readonly EndianessAwareBitConverter convert = new EndianessAwareBitConverter {IsLittleEndian = true};
        private readonly byte[] buffer;
	    private int idx;

	    private GattByteBuffer(byte[] byteArray)
	    {
	        buffer = byteArray;
	    }

        //public byte[] array()
        //{
        //    return buffer;
        //}

        //public int capacity()
        //{
        //    return buffer.Length;
        //}

        //public void getInt8(byte[] value, int i, int j)
        //{
        //    Array.Copy(buffer, i, value, 0, j-i);
        //    //convert.GetBytes(buffer, .get(value, i, j));
        //}

        //public void getUint8(short[] value, int offset, int length)
        //{
        //    for (int i = 0; i < length; i++)
        //    {
        //        value[i + offset] = Uint8;
        //    }
        //}

        private T Convert<T>(Func<byte[], int, T> c, int bytesUsed) 
                                where T:struct
	    {
			T ret = c(buffer, idx);
	        idx += bytesUsed;
		    return ret;
	    }

		public bool Boolean()
		{
			return buffer[idx++] != 0;
		}

		public float Float32()
		{
		    return Convert(convert.ToSingle, 4);
		}

		public short Int16()
		{
		    return Convert(convert.ToInt16, 2);
		}

		public int Int32()
		{
            return Convert<Int32>(convert.ToInt32, 4);
		}


	    public long Int64()
		{
			return Convert(convert.ToInt64, 8);
		}

		public sbyte Int8()
		{
		    return (sbyte) buffer[idx++];
		}

		public string String()
		{
            StringBuilder bld = new StringBuilder();

		    byte b;
			while ((b = Uint8()) != 0)
				bld.Append((char) b);
			
            return bld.ToString();
		}

		public ushort Uint16()
		{
            return Convert(convert.ToUInt16, 2);
		}

		public uint Uint32()
		{
            return Convert(convert.ToUInt32, 4);

		}

		public byte Uint8()
		{
		    return buffer[idx++];
		}

		public GattByteBuffer Seek(int i)
		{
		    idx = i;
			return this;
		}

        //public GattByteBuffer putUint8(short[] value, int offset, int length)
        //{
        //    for (int i = 0; i < length; i++)
        //    {
        //        putUint8(value[i + offset]);
        //    }
        //    return this;
        //}

        //public GattByteBuffer putInt8(sbyte[] value, int i, int j)
        //{
        //    buffer.put(value, i, j);
        //    return this;
        //}

        //public GattByteBuffer putBoolean(bool doEnable)
        //{
        //    if (doEnable)
        //    {
        //        buffer.put((sbyte) 1);
        //    }
        //    else
        //    {
        //        buffer.put((sbyte) 0);
        //    }
        //    return this;
        //}

        //public GattByteBuffer putFloat32(float? value)
        //{
        //    buffer.putFloat(value);
        //    return this;
        //}

        //public GattByteBuffer putInt16(short value)
        //{
        //    buffer.putShort(value);
        //    return this;
        //}

        //public GattByteBuffer putInt32(int value)
        //{
        //    buffer.putInt(value);
        //    return this;
        //}

        //public GattByteBuffer putInt64(long value)
        //{
        //    buffer.putLong(value);
        //    return this;
        //}

        //public GattByteBuffer putInt8(sbyte value)
        //{
        //    buffer.put(value);
        //    return this;
        //}

        //public GattByteBuffer putString(string value)
        //{
        //    buffer.put(value.Bytes);
        //    buffer.put((sbyte) 0);
        //    return this;
        //}

        //public GattByteBuffer putUint16(int value)
        //{
        //    buffer.putShort((short) value);
        //    return this;
        //}

        //public GattByteBuffer putUint32(long value)
        //{
        //    buffer.putInt((int) value);
        //    return this;
        //}

        //public GattByteBuffer putUint8(short value)
        //{
        //    buffer.put((sbyte) value);
        //    return this;
        //}

        //public GattByteBuffer rewind()
        //{
        //    buffer.rewind();
        //    return this;
        //}

        public bool HasRemaining()
        {
            return buffer.Length > idx;
        }
	}

}