// Adapted by Olaf from http://snipplr.com/view/15179

using System;
using System.Linq;

namespace Utils
{
    /// <summary>
    ///     Converts base data types to an array of bytes, and an array of bytes to base
    ///     data types.
    ///     All info taken from the meta data of System.BitConverter. This implementation
    ///     allows for Endianness consideration.
    ///</summary>
    public  class EndianessAwareBitConverter
    {
        private bool _isLittleEndian;

        /// <summary>
        ///     Indicates the byte order ("endianess") in which data is processed by 
        ///     this BitConverter
        ///</summary>
        public bool IsLittleEndian
        {
            get { return _isLittleEndian; }
            set { _isLittleEndian = value; IsReverseEndianess = value != BitConverter.IsLittleEndian; }

        } 
        public bool IsReverseEndianess { get; private set; }

        /// <summary>
        ///     Converts the specified double-precision floating point number to a 64-bit
        ///     signed integer.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     A 64-bit signed integer whose value is equivalent to value.
        ///</summary>
        public  long DoubleToInt64Bits(double value) { throw new NotImplementedException(); }
        ///
        /// <summary>
        ///     Returns the specified Boolean value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     A Boolean value.
        ///
        /// Returns:
        ///     An array of bytes with length 1.
        ///</summary>
        public  byte[] GetBytes(bool value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified Unicode character value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     A character to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 2.
        ///</summary>
        public  byte[] GetBytes(char value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified double-precision floating point value as an array of
        ///     bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 8.
        ///</summary>
        public  byte[] GetBytes(double value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified single-precision floating point value as an array of
        ///     bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 4.
        ///</summary>
        public  byte[] GetBytes(float value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 32-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 4.
        ///</summary>
        public  byte[] GetBytes(int value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 64-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 8.
        ///</summary>
        public  byte[] GetBytes(long value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 16-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 2.
        ///</summary>
        public  byte[] GetBytes(short value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 32-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 4.
        ///</summary>
        //[CLSCompliant(false)]
        public  byte[] GetBytes(uint value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 64-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 8.
        ///</summary>
        //[CLSCompliant(false)]
        public  byte[] GetBytes(ulong value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }
        ///
        /// <summary>
        ///     Returns the specified 16-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     An array of bytes with length 2.
        ///</summary>
        public  byte[] GetBytes(ushort value)
        {
            if (!IsReverseEndianess)
            {
                return System.BitConverter.GetBytes(value);
            }
            else
            {
                var ret = System.BitConverter.GetBytes(value);
                Array.Reverse(ret);
                return ret;
            }
        }

        ///
        /// <summary>
        ///     Converts the specified 64-bit signed integer to a double-precision floating
        ///     point number.
        ///
        /// Parameters:
        ///   value:
        ///     The number to convert.
        ///
        /// Returns:
        ///     A double-precision floating point number whose value is equivalent to value.
        ///</summary>
        public double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        ///
        /// <summary>
        ///     Returns a Boolean value converted from one byte at a specified position in
        ///     a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     true if the byte at startIndex in value is nonzero; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        ///
        /// <summary>
        ///     Returns a Unicode character converted from two bytes at a specified position
        ///     in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A character formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex equals the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public char ToChar(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToChar, value, startIndex, 2);
        }

        ///
        /// <summary>
        ///     Returns a double-precision floating point number converted from eight bytes
        ///     at a specified position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A double precision floating point number formed by eight bytes beginning
        ///     at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 7, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public double ToDouble(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToDouble, value, startIndex, 8);
        }

        ///
        /// <summary>
        ///     Returns a 16-bit signed integer converted from two bytes at a specified position
        ///     in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 16-bit signed integer formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex equals the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  short ToInt16(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToInt16, value, startIndex, 2);
        }
        ///
        /// <summary>
        ///     Returns a 32-bit signed integer converted from four bytes at a specified
        ///     position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 32-bit signed integer formed by four bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 3, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  int ToInt32(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToInt32, value, startIndex, 4);
        }
        ///
        /// <summary>
        ///     Returns a 64-bit signed integer converted from eight bytes at a specified
        ///     position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 64-bit signed integer formed by eight bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 7, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  long ToInt64(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToInt64, value, startIndex, 8);
        }
        ///
        /// <summary>
        ///     Returns a single-precision floating point number converted from four bytes
        ///     at a specified position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A single-precision floating point number formed by four bytes beginning at
        ///     startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 3, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  float ToSingle(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToSingle, value, startIndex, 4);
        }
        ///
        /// <summary>
        ///     Converts the numeric value of each element of a specified array of bytes
        ///     to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        /// Returns:
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair
        ///     represents the corresponding element in value; for example, "7F-2C-4A".
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     value is null.
        ///</summary>
        public  string ToString(byte[] value)
        {
            // there is no endianess in a string.
            return System.BitConverter.ToString(value);
        }
        ///
        /// <summary>
        ///     Converts the numeric value of each element of a specified subarray of bytes
        ///     to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair
        ///     represents the corresponding element in a subarray of value; for example,
        ///     "7F-2C-4A".
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  string ToString(byte[] value, int startIndex)
        {
            // there is no endianess in a string.
            return System.BitConverter.ToString(value, startIndex);
        }
        ///
        /// <summary>
        ///     Converts the numeric value of each element of a specified subarray of bytes
        ///     to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        ///   length:
        ///     The number of array elements in value to convert.
        ///
        /// Returns:
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair
        ///     represents the corresponding element in a subarray of value; for example,
        ///     "7F-2C-4A".
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex or length is less than zero.  -or- startIndex is greater than
        ///     zero and is greater than or equal to the length of value.
        ///
        ///   System.ArgumentException:
        ///     The combination of startIndex and length does not specify a position within
        ///     value; that is, the startIndex parameter is greater than the length of value
        ///     minus the length parameter.
        ///</summary>
        public  string ToString(byte[] value, int startIndex, int length)
        {
            // there is no endianess in a string.
            return System.BitConverter.ToString(value, startIndex, length);
        }
        ///
        /// <summary>
        ///     Returns a 16-bit unsigned integer converted from two bytes at a specified
        ///     position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     The array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 16-bit unsigned integer formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex equals the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  ushort ToUInt16(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToUInt16, value, startIndex, 2);
        }
        ///
        /// <summary>
        ///     Returns a 32-bit unsigned integer converted from four bytes at a specified
        ///     position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 32-bit unsigned integer formed by four bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 3, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  uint ToUInt32(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToUInt32, value, startIndex, 4);

        }
        ///
        /// <summary>
        ///     Returns a 64-bit unsigned integer converted from eight bytes at a specified
        ///     position in a byte array.
        ///
        /// Parameters:
        ///   value:
        ///     An array of bytes.
        ///
        ///   startIndex:
        ///     The starting position within value.
        ///
        /// Returns:
        ///     A 64-bit unsigned integer formed by the eight bytes beginning at startIndex.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     startIndex is greater than or equal to the length of value minus 7, and is
        ///     less than or equal to the length of value minus 1.
        ///
        ///   System.ArgumentNullException:
        ///     value is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public  ulong ToUInt64(byte[] value, int startIndex)
        {
            return Convert(BitConverter.ToUInt64, value, startIndex, 8);

        }

        private T Convert<T>(Func<byte[], int, T> c, byte[] data, int startIndex, int bytesUsed)
                                where T : struct
        {
            if (!IsReverseEndianess)
                return c(data, startIndex);

            byte[] b = new byte[bytesUsed];
            Array.Copy(data, startIndex, b, 0, bytesUsed);
            Array.Reverse(b);
            return c(b, 0);
        }
    }
}