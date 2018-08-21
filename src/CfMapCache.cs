using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ConnectionFactory
{
    /// <summary>
    /// The 'Singleton' class
    /// </summary>
    internal sealed class CfMapCache
    {
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly CfMapCache Instance = new CfMapCache();
        private readonly Dictionary<string, Dictionary<string, PropertyInfo>> _propertiesPool = new Dictionary<string, Dictionary<string, PropertyInfo>>();

        // Constructor (protected)
        private CfMapCache() { }

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
            Dictionary<string, PropertyInfo> value;
            if (_propertiesPool.TryGetValue(targetType.FullName, out value) && value != null) return;
            var objectProperties = targetType.GetProperties();

            var hashtable = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo info in objectProperties.Where(info => info.CanWrite))
            {
                hashtable[GetDbColumnName(info) ?? info.Name.ToUpper()] = info;
            }
            _propertiesPool[targetType.FullName] = hashtable;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private static string GetDbColumnName(MemberInfo targetType)
        {
            var columnAttribute =
            (ColumnAttribute)Attribute.GetCustomAttribute(targetType, typeof(ColumnAttribute));

            return !string.IsNullOrWhiteSpace(columnAttribute?.Name) ? columnAttribute.Name.ToUpper() : null;
        }

        #endregion


    }
}
