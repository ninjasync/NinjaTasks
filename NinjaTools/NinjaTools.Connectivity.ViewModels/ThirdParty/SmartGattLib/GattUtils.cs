using System;

namespace com.movisens.smartgattlib
{

	public class GattUtils
	{
		public static readonly byte[] leastSigBits = new byte[] {0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb};

		public const int FIRST_BITMASK = 0x01;
		public static readonly int SECOND_BITMASK = FIRST_BITMASK << 1;
		public static readonly int THIRD_BITMASK = FIRST_BITMASK << 2;
		public static readonly int FOURTH_BITMASK = FIRST_BITMASK << 3;
		public static readonly int FIFTH_BITMASK = FIRST_BITMASK << 4;
		public static readonly int SIXTH_BITMASK = FIRST_BITMASK << 5;
		public static readonly int SEVENTH_BITMASK = FIRST_BITMASK << 6;
		public static readonly int EIGTH_BITMASK = FIRST_BITMASK << 7;

		public const int FORMAT_UINT8 = 17;
		public const int FORMAT_UINT16 = 18;
		public const int FORMAT_UINT32 = 20;
		public const int FORMAT_SINT8 = 33;
		public const int FORMAT_SINT16 = 34;
		public const int FORMAT_SINT32 = 36;
		public const int FORMAT_SFLOAT = 50;
		public const int FORMAT_FLOAT = 52;

		public static Guid ToGuid(string uuidString)
		{
			return Guid.Parse(uuidString);
		}

		public static Guid ToGuid(int assignedNumber)
		{
            return new Guid((int)assignedNumber, 0, 0x1000, leastSigBits);
			//return new Guid((((ulong)assignedNumber) << 32) | 0x1000, leastSigBits);
		}

        public static string ToUuid128(int assignedNumber)
		{
			return ToGuid(assignedNumber).ToString();
		}

		public static string ToUuid16(int assignedNumber)
		{
			return assignedNumber.ToString("x");
		}

		public static int? GetIntValue(sbyte[] value, int format, int position)
		{
			if (value == null)
			{
				return null;
			}
			if (position + (format & 0xF) > value.Length)
			{
				return null;
			}
			switch (format)
			{
			case FORMAT_UINT8:
				return Convert.ToInt32(value[position] & 0xFF);
			case FORMAT_UINT16:
				return Convert.ToInt32(add(value[position], value[(position + 1)]));
			case FORMAT_UINT32:
				return Convert.ToInt32(add(value[position], value[(position + 1)], value[(position + 2)], value[(position + 3)]));
			case FORMAT_SINT8:
				return Convert.ToInt32(signed(value[position] & 0xFF, 8));
			case FORMAT_SINT16:
				return Convert.ToInt32(signed(add(value[position], value[(position + 1)]), 16));
			case FORMAT_SINT32:
				return Convert.ToInt32(signed(add(value[position], value[(position + 1)], value[(position + 2)], value[(position + 3)]), 32));
			}
			return null;
		}

		public static float? GetFloatValue(sbyte[] value, int format, int position)
		{
			if (value == null)
			{
				return null;
			}
			if (position + (format & 0xF) > value.Length)
			{
				return null;
			}
			int i;
			int mantissa;
			int exponent;
			switch (format)
			{
			case FORMAT_SFLOAT:
				i = value[(position + 1)];
				position = value[position];
				mantissa = signed((position & 0xFF) + ((i & 0xFF & 0xF) << 8), 12);
				exponent = signed((i & 0xFF) >> 4, 4);
				return Convert.ToSingle((float)(mantissa * Math.Pow(10.0D, exponent)));
			case FORMAT_FLOAT:
				exponent = value[(position + 3)];
				mantissa = value[(position + 2)];
				i = value[(position + 1)];
				position = value[position];
				return Convert.ToSingle((float)((format = signed((position & 0xFF) + ((i & 0xFF) << 8) + ((mantissa & 0xFF) << 16), 24)) * Math.Pow(10.0D, exponent)));
			}
			return null;
		}

		public static string GetStringValue(sbyte[] value, int position)
		{
			if (value == null)
			{
				return null;
			}
			if (position > value.Length)
			{
				return null;
			}
			sbyte[] arrayOfByte = new sbyte[value.Length - position];
			for (int i = 0; i != value.Length - position; i++)
			{
				arrayOfByte[i] = value[(position + i)];
			}
			return StringHelperClass.NewString(arrayOfByte);
		}

		private static int add(sbyte byte1, sbyte byte2)
		{
			return (byte1 & 0xFF) + ((byte2 & 0xFF) << 8);
		}

		private static int add(sbyte byte1, sbyte byte2, sbyte byte3, sbyte byte4)
		{
			return (byte1 & 0xFF) + ((byte2 & 0xFF) << 8) + ((byte3 & 0xFF) << 16) + ((byte4 & 0xFF) << 24);
		}

		private static int signed(int value, int length)
		{
			if ((value & 1 << length - 1) != 0)
			{
				value = -1 * ((1 << length - 1) - (value & (1 << length - 1) - 1));
			}
			return value;
		}

        ///// <summary>
        ///// Convert hex byte array from motorola API to byte array.
        ///// </summary>
        ///// <param name="hexByteArray">
        ///// @return </param>
        //public static sbyte[] hexByteArrayToByteArray(sbyte[] hexByteArray)
        //{
        //    return hexStringToByteArray(StringHelperClass.NewString(hexByteArray));
        //}

        ///// <summary>
        ///// Convert string from motorola API to a byte array.
        ///// </summary>
        ///// <param name="hexString">
        ///// @return </param>
        //public static sbyte[] hexStringToByteArray(string hexString)
        //{
        //    int len = hexString.Length;
        //    sbyte[] data = new sbyte[len / 2];
        //    for (int i = 0; i < len; i += 2)
        //    {
        //        data[i / 2] = (sbyte)((char.IsDigit(hexStrin[i], 16) << 4) + char.digit(hexString[i + 1], 16));
        //    }
        //    return data;
        //}
	}

}