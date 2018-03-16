namespace ConnectionFactory
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// This base class provides datatype conversion services for the Cf provider.
    /// </summary>
    public abstract class CfConvert
    {
        /// <summary>
        /// Initializes the conversion class
        /// </summary>       
        internal CfConvert()
        {
        }

        #region Type Conversions

        /// <summary>
        /// For a given intrinsic type, return a DbType
        /// </summary>
        /// <param name="typ">The native type to convert</param>
        /// <returns>The corresponding (closest match) DbType</returns>
        internal static DbType TypeToDbType(Type typ)
        {
            var tc = NetTypes.GetNetType(typ);
            if (tc == NetType.Object)
            {
                if (typ == typeof(byte[])) return DbType.Binary;
                if (typ == typeof(Guid)) return DbType.Guid;
                return DbType.String;
            }
            return _typetodbtype[(int)tc];
        }

        private static DbType[] _typetodbtype = {
      DbType.Object,
      DbType.Binary,
      DbType.Object,
      DbType.Boolean,
      DbType.SByte,
      DbType.SByte,
      DbType.Byte,
      DbType.Int16, // 7
      DbType.UInt16,
      DbType.Int32,
      DbType.UInt32,
      DbType.Int64, // 11
      DbType.UInt64,
      DbType.Single,
      DbType.Double,
      DbType.Decimal,
      DbType.DateTime,
      DbType.Object,
      DbType.String,
    };

        #endregion
    }

    /// <summary>
    /// Intermediary types for converting between Cf data types and CLR data types
    /// </summary>
    public enum NetType
    {
        /// <summary>
        /// Empty data type
        /// </summary>
        Empty = 0,
        /// <summary>
        /// Object data type
        /// </summary>
        Object = 1,
        /// <summary>
        /// SQL NULL value
        /// </summary>
        DBNull = 2,
        /// <summary>
        /// Boolean data type
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// Character data type
        /// </summary>
        Char = 4,
        /// <summary>
        /// Signed byte data type
        /// </summary>
        SByte = 5,
        /// <summary>
        /// Byte data type
        /// </summary>
        Byte = 6,
        /// <summary>
        /// Short/Int16 data type
        /// </summary>
        Int16 = 7,
        /// <summary>
        /// Unsigned short data type
        /// </summary>
        UInt16 = 8,
        /// <summary>
        /// Integer data type
        /// </summary>
        Int32 = 9,
        /// <summary>
        /// Unsigned integer data type
        /// </summary>
        UInt32 = 10,
        /// <summary>
        /// Long/Int64 data type
        /// </summary>
        Int64 = 11,
        /// <summary>
        /// Unsigned long data type
        /// </summary>
        UInt64 = 12,
        /// <summary>
        /// Single precision float data type
        /// </summary>
        Single = 13,
        /// <summary>
        /// Double precision float data type
        /// </summary>
        Double = 14,
        /// <summary>
        /// Decimal data type
        /// </summary>
        Decimal = 15,
        /// <summary>
        /// DateTime data type
        /// </summary>
        DateTime = 16,
        /// <summary>
        /// String data type
        /// </summary>
        String = 18,
    }

    internal static class NetTypes
    {
        private static Dictionary<Type, NetType> types = new Dictionary<Type, NetType> {
      {typeof(bool), NetType.Boolean},
      {typeof(char), NetType.Char},
      {typeof(sbyte), NetType.SByte},
      {typeof(byte), NetType.Byte},
      {typeof(short), NetType.Int16},
      {typeof(ushort), NetType.UInt16},
      {typeof(int), NetType.Int32},
      {typeof(uint), NetType.UInt32},
      {typeof(long), NetType.Int64},
      {typeof(ulong), NetType.UInt64},
      {typeof(float), NetType.Single},
      {typeof(double), NetType.Double},
      {typeof(decimal), NetType.Decimal},
      {typeof(DateTime), NetType.DateTime},
      {typeof(string), NetType.String}
    };
        public static NetType GetNetType(Type type)
        {
            if (type == (Type)null)
                return NetType.Empty;
            else if (type != type.UnderlyingSystemType && type.UnderlyingSystemType != (Type)null)
                return GetNetType(type.UnderlyingSystemType);
            else
                return GetNetTypeImpl(type);
        }
        private static NetType GetNetTypeImpl(Type type)
        {
            if (types.ContainsKey(type))
            {
                return types[type];
            }

            if (type.IsEnum)
            {
                return types[Enum.GetUnderlyingType(type)];
            }

            return NetType.Object;
        }
    }
}
