using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;

namespace ConnectionFactory
{
   /// <summary>
   /// The 'Singleton' class
   /// </summary>
   internal sealed class CfEntityPropertiesPool
   {
      // Static members are 'eagerly initialized', that is, 
      // immediately when class is loaded for the first time.
      // .NET guarantees thread safety for static initialization
      private static readonly CfEntityPropertiesPool Instance = new CfEntityPropertiesPool();
      private readonly Dictionary<string, Dictionary<string, PropertyInfo>> _propertiesPool = new Dictionary<string, Dictionary<string, PropertyInfo>>();

      // Constructor (protected)
      private CfEntityPropertiesPool() { }

      [System.Diagnostics.DebuggerStepThrough]
      public static Dictionary<string, PropertyInfo> GetInstance(Type sourceType)
      {
         return Instance.GetProperties(sourceType);
      }

      #region Private methods
      
      [System.Diagnostics.DebuggerStepThrough]
      private Dictionary<string, PropertyInfo> GetProperties(Type sourceType)
      {
         LoadProperties(sourceType);
         return _propertiesPool[sourceType.FullName];
      }

      [System.Diagnostics.DebuggerStepThrough]
      private void LoadProperties(Type targetType)
      {
         if (_propertiesPool.ContainsKey(targetType.FullName) && _propertiesPool[targetType.FullName] != null) return;
         var objectProperties = targetType.GetProperties();

         var hashtable = new Dictionary<string, PropertyInfo>();
         foreach (var info in objectProperties.Where(info => info.CanWrite))
         {
            hashtable[GetDbColumnName(info) ?? info.Name.ToUpper()] = info;
         }
         _propertiesPool[targetType.FullName] = hashtable;
      }

      [System.Diagnostics.DebuggerStepThrough]
      private static string GetDbColumnName(PropertyInfo targetType)
      {
         var columnAttribute =
         (ColumnAttribute)Attribute.GetCustomAttribute(targetType, typeof(ColumnAttribute));

         if (columnAttribute != null && !string.IsNullOrWhiteSpace(columnAttribute.Name))
         {
            return columnAttribute.Name.ToUpper();
         }
         return null;
      }
      
      #endregion


   }
}
