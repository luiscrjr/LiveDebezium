using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Domain.Repositories.Impl.Helpers.Extensions
{
    public static class DataReaderExtensions
    {
        /// <Summary>
        /// Map data from DataReader to list of objects
        /// </Summary>
        /// <typeparam name="T">Object</typeparam>
        /// <param name="dr">Data Reader</param>
        /// <returns>List of objects having data from data reader</returns>
        public static List<T> MapToList<T>(this DbDataReader dr) where T : new()
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

                                var properType = Nullable.GetUnderlyingType(Info.PropertyType) ?? Info.PropertyType;

                                if (properType == typeof(bool))
                                    Info.SetValue(newObject, (Val == DBNull.Value) ? default : Val.ToString().StringToBoolean(), null);
                                else
                                    Info.SetValue(newObject, (Val == DBNull.Value) ? default : Convert.ChangeType(Val, properType), null);
                            }
                        }
                    }
                    RetVal.Add(newObject);
                }
            }
            else
            {
                return new List<T>();
            }

            return RetVal;
        }

        public static List<T> MapToListNonObject<T>(this DbDataReader dr)

        {
            List<T> RetVal = null;

            if (dr != null && dr.HasRows)
            {
                RetVal = new List<T>();
                while (dr.Read())
                {
                    for (var Index = 0; Index < dr.FieldCount; Index++)
                    {
                        var value = dr.GetValue(Index).ToString();

                        if (!string.IsNullOrWhiteSpace(value)) RetVal.Add(dr.GetFieldValue<T>(Index));
                    }
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

                            var properType = Nullable.GetUnderlyingType(Info.PropertyType) ?? Info.PropertyType;

                            if (properType == typeof(bool))
                                Info.SetValue(RetVal, (Val == DBNull.Value) ? default : Val.ToString().StringToBoolean(), null);
                            else
                                Info.SetValue(RetVal, (Val == DBNull.Value) ? default : Convert.ChangeType(Val, properType), null);
                        }
                    }
                }
            }
            return RetVal;
        }
    }
}