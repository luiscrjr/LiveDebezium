using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Infra.Shared.Extensions
{
    public static class DataReaderExtensions
    {
        /// <Summary>
        /// Map data from DataReader to list of objects
        /// </Summary>
        /// <typeparam name="T">Object</typeparam>
        /// <param name="dr">Data Reader</param>
        /// <returns>List of objects having data from data reader</returns>
        public static List<T> MapToList<T>(this DbDataReader dr)
            where T : new()
        {
            List<T> RetVal = null;
            var Entity = typeof(T);
            if (dr != null && dr.HasRows)
            {
                RetVal = new List<T>();
                var Props = Entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var PropDict = Props.ToDictionary(p => p.Name.ToUpper(), p => p);
                while (dr.Read())
                {
                    var newObject = new T();
                    for (var Index = 0; Index < dr.FieldCount; Index++)
                    {
                        if (PropDict.ContainsKey(dr.GetName(Index).ToUpper()))
                        {
                            var Info = PropDict[dr.GetName(Index).ToUpper()];
                            if ((Info != null) && Info.CanWrite)
                            {
                                var Val = dr.GetValue(Index);
                                Info.SetValue(newObject, (Val == DBNull.Value) ? null : Val, null);
                            }
                        }
                    }
                    RetVal.Add(newObject);
                }
            }
            return RetVal;
        }

        /// <Summary>
        /// Map data from DataReader to an object
        /// </Summary>
        /// <typeparam name="T">Object</typeparam>
        /// <param name="dr">Data Reader</param>
        /// <returns>Object having data from Data Reader</returns>
        public static T MapToSingle<T>(this DbDataReader dr)
            where T : new()
        {
            var RetVal = new T();
            var Entity = typeof(T);
            if (dr != null && dr.HasRows)
            {
                var Props = Entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var PropDict = Props.ToDictionary(p => p.Name.ToUpper(), p => p);
                dr.Read();
                for (var Index = 0; Index < dr.FieldCount; Index++)
                {
                    if (PropDict.ContainsKey(dr.GetName(Index).ToUpper()))
                    {
                        var Info = PropDict[dr.GetName(Index).ToUpper()];
                        if ((Info != null) && Info.CanWrite)
                        {
                            var Val = dr.GetValue(Index);
                            Info.SetValue(RetVal, (Val == DBNull.Value) ? null : Val, null);
                        }
                    }
                }
            }
            return RetVal;
        }
    }
}