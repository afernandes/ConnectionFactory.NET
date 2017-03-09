using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace ConnectionFactory
{
   /// <summary>
   /// Cf implementation of DbParameter.
   /// </summary>
   public sealed class CfParameter : ICloneable
   {
      /// <summary>
      /// The data type of the parameter
      /// </summary>
      private int _dbType;
      /// <summary>
      /// The value of the data in the parameter
      /// </summary>
      private Object _objValue;
      /// <summary>
      /// The column name
      /// </summary>
      private string _parameterName;
      /// <summary>
      /// The data size, unused by Cf
      /// </summary>
      private int _dataSize;

      private bool _nullable;

      /// <summary>
      /// Default constructor
      /// </summary>
      public CfParameter()
         : this(null, (DbType)(-1), 0)
      {
      }

      /// <summary>
      /// Constructs a named parameter given the specified parameter name
      /// </summary>
      /// <param name="parameterName">The parameter name</param>
      public CfParameter(string parameterName)
         : this(parameterName, (DbType)(-1), 0)
      {
      }

      /// <summary>
      /// Constructs a named parameter given the specified parameter name and initial value
      /// </summary>
      /// <param name="parameterName">The parameter name</param>
      /// <param name="value">The initial value of the parameter</param>
      public CfParameter(string parameterName, object value)
         : this(parameterName, (DbType)(-1), 0)
      {
         ParamValue = value;
      }

      /// <summary>
      /// Constructs a named parameter of the specified type
      /// </summary>
      /// <param name="parameterName">The parameter name</param>
      /// <param name="dbType">The datatype of the parameter</param>
      public CfParameter(string parameterName, DbType dbType)
         : this(parameterName, dbType, 0)
      {
      }

      /// <summary>
      /// Constructs a named parameter of the specified type and source column reference
      /// </summary>
      /// <param name="parameterName">The parameter name</param>
      /// <param name="dbType">The data type</param>
      /// <param name="value">The initial value of the parameter</param>
      public CfParameter(string parameterName, object value, DbType dbType)
         : this(parameterName, dbType, 0)
      {
         ParamValue = value;
      }

      /// <summary>
      /// Constructs a named parameter of the specified type, size, source column and row version
      /// </summary>
      /// <param name="parameterName">The name of the parameter</param>
      /// <param name="parameterType">The data type</param>
      /// <param name="parameterSize">The size of the parameter</param>
      public CfParameter(string parameterName, DbType parameterType, int parameterSize)
      {
         _parameterName = parameterName;
         _dbType = (int)parameterType;
         _objValue = DBNull.Value;
         _dataSize = parameterSize;
         _nullable = true;
      }

      /// <summary>
      /// Constructs a named parameter of the specified type, size, source column and row version
      /// </summary>
      /// <param name="parameterName">The name of the parameter</param>
      /// <param name="parameterType">The data type</param>
      /// <param name="parameterSize">The size of the parameter</param>
      /// <param name="direction">Only input parameters are supported in Cf</param>
      /// <param name="isNullable">Ignored</param>
      /// <param name="precision">Ignored</param>
      /// <param name="scale">Ignored</param>
      /// <param name="value">The initial value to assign the parameter</param>   
      [EditorBrowsable(EditorBrowsableState.Advanced)]
      public CfParameter(string parameterName, DbType parameterType, int parameterSize, ParameterDirection direction, bool isNullable, byte precision, byte scale, object value)
         : this(parameterName, parameterType, parameterSize)
      {
         ParamDirection = direction;
         IsNullable = isNullable;
         ParamValue = value;
      }

      public CfParameter(CfParameter parameter)
         : this(parameter.ParamName, parameter.ParamValue, parameter.ParamDbType)
      {
         IsNullable = parameter.IsNullable;
      }


      /// <summary>
      /// Constructs a named parameter, yet another flavor
      /// </summary>
      /// <param name="parameterName">The name of the parameter</param>
      /// <param name="parameterType">The data type</param>
      /// <param name="parameterSize">The size of the parameter</param>
      /// <param name="direction">Only input parameters are supported in Cf</param>
      /// <param name="precision">Ignored</param>
      /// <param name="scale">Ignored</param>
      /// <param name="value">The intial value to assign the parameter</param>
      [EditorBrowsable(EditorBrowsableState.Advanced)]
      public CfParameter(string parameterName, DbType parameterType, int parameterSize, ParameterDirection direction, byte precision, byte scale, object value)
         : this(parameterName, parameterType, parameterSize)
      {
         ParamDirection = direction;
         ParamValue = value;
      }

      /// <summary>
      /// Whether or not the parameter can contain a null value
      /// </summary>
      public bool IsNullable
      {
         get
         {
            return _nullable;
         }
         set
         {
            _nullable = value;
         }
      }

      /// <summary>
      /// Returns the datatype of the parameter
      /// </summary>
      [DbProviderSpecificTypeProperty(true)]
      public DbType ParamDbType
      {
         get
         {
            if (_dbType == -1)
            {
               if (_objValue != null && _objValue != DBNull.Value)
               {
                  return CfConvert.TypeToDbType(_objValue.GetType());
               }
               return DbType.String; // Unassigned default value is String
            }
            return (DbType)_dbType;
         }
         set
         {
            _dbType = (int)value;
         }
      }

      /// <summary>
      /// Supports only input parameters
      /// </summary>
      public ParameterDirection ParamDirection
      {
         get
         {
            return ParameterDirection.Input;
         }
         set
         {
            if (value != ParameterDirection.Input)
               throw new NotSupportedException();
         }
      }

      /// <summary>
      /// Returns the parameter name
      /// </summary>
      public string ParamName
      {
         get
         {
            return _parameterName;
         }
         set
         {
            _parameterName = value;
         }
      }

      /// <summary>
      /// Resets the DbType of the parameter so it can be inferred from the value
      /// </summary>
      public void ResetDbType()
      {
         _dbType = -1;
      }

      /// <summary>
      /// Returns the size of the parameter
      /// </summary>
      [DefaultValue((int)0)]
      public  int Size
      {
         get
         {
            return _dataSize;
         }
         set
         {
            _dataSize = value;
         }
      }

      /// <summary>
      /// Gets and sets the parameter value.  If no datatype was specified, the datatype will assume the type from the value given.
      /// </summary>
      public object ParamValue
      {
         get
         {
            return _objValue;
         }
         set
         {
            _objValue = value ?? DBNull.Value;
            if (_dbType == -1 && _objValue != null && _objValue != DBNull.Value) // If the DbType has never been assigned, try to glean one from the value's datatype 
               _dbType = (int)CfConvert.TypeToDbType(_objValue.GetType());
         }
      }

      /// <summary>
      /// Clones a parameter
      /// </summary>
      /// <returns>A new, unassociated CfParameter</returns>
      public object Clone()
      {
         var newparam = new CfParameter(this);

         return newparam;
      }

      /// <summary>
      ///     <see cref="System.Object.ToString()"/>
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return string.Format("{0} = {1} ({2})", 
            ParamName,
            (ParamValue == null || ParamValue == DBNull.Value) ? "NULL" : String.Format("'{0}'", ParamValue), 
            ParamDbType);
      }
   

   }
}
