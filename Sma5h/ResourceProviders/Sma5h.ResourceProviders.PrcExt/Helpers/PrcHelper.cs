using paracobNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Sma5h.ResourceProviders.Prc.Helpers
{
    public class PrcHelper
    {
        private readonly Dictionary<ulong, string> _paramHashes;

        public PrcHelper(Dictionary<ulong, string> paramHashes)
        {
            _paramHashes = paramHashes;
        }

        public T ReadPrcFile<T>(string inputFile) where T : new()
        {
            //Validate file
            if (!File.Exists(inputFile))
                throw new Exception($"PRC File does not exist: {inputFile}");

            //Open file
            var t = new ParamFile();
            t.Open(inputFile);

            //Init object
            var outputObj = new T();

            //Parse recursively
            ReadPrc(t.Root, outputObj, null);

            return outputObj;
        }

        public void WritePrcFile<T>(string outputFile, T inputObj)
        {
            //Open file
            var t = new ParamFile();

            //Write recursively
            WritePrc(t.Root, inputObj, inputObj.GetType().GetProperties());

            //Save
            t.Save(outputFile);
        }

        #region Load
        private void ReadPrc(Hash40Pairs<IParam> nodes, object objToMap)
        {
            var objType = objToMap.GetType();
            var hexMapping = MapOffsetToProperties(objType);
            foreach (var node in nodes)
            {
                PropertyInfo propertyInfo = null;

                //Regular case
                if (hexMapping.ContainsKey(node.Key))
                {
                    propertyInfo = objType.GetProperty(hexMapping[node.Key]);
                    ReadPrc(node.Value, objToMap, propertyInfo);
                }
                //Prc filter
                else
                {
                    var filtersRegex = MapOffsetToRegexFilters(objType);

                    if (filtersRegex.Count == 0 || _paramHashes == null || !_paramHashes.ContainsKey(node.Key))
                        throw new Exception($"Hex 0x{node.Key:X} was not mapped to any property.");

                    var keyString = _paramHashes[node.Key];

                    foreach (var filterRegex in filtersRegex)
                    {
                        if (Regex.IsMatch(keyString, filterRegex.Value.Regex))
                        {
                            var newObjToMap = WriteNewFilterStruct(objToMap, objType, filterRegex.Key, node.Key);
                            propertyInfo = newObjToMap.GetType().GetProperty("Values");
                            ReadPrc(node.Value, newObjToMap, propertyInfo);
                            break;
                        }
                    }

                    if (propertyInfo == null)
                        throw new Exception($"Hex 0x{node.Key:X} was not mapped to any property.");
                }
            }
        }

        private void ReadPrc(IParam node, object objToMap, PropertyInfo propertyInfo)
        {
            //List
            if (node is ParamList)
            {
                if (node.TypeKey != ParamType.list)
                    throw new Exception("Data error");

                var nodeList = node as ParamList;

                //Instanciate list
                var newList = Activator.CreateInstance(propertyInfo.PropertyType);
                var dictKey = MapDictionaryKeyFromPropertyInfo(propertyInfo);
                propertyInfo.SetValue(objToMap, newList);

                //Regular case - List
                if (dictKey == null)
                {
                    var newListObj = GetListObjectType(newList.GetType());
                    var newListAdd = newList.GetType().GetMethod("Add");

                    //Set in property
                    foreach (var nodeChild in nodeList.Nodes)
                    {
                        var newObjInstance = Activator.CreateInstance(newListObj);
                        newListAdd.Invoke(newList, new[] { newObjInstance });
                        ReadPrc(nodeChild, newObjInstance, null);
                    }
                }
                //Dictionary case - PrcDictionary
                else
                {
                    var newDictObj = GetDictionaryObjectType(newList.GetType());
                    var newListAdd = newList.GetType().GetMethod("Add", new[] { newDictObj.Item1, newDictObj.Item2 });

                    //Set in property
                    foreach (var nodeChild in nodeList.Nodes)
                    {
                        var id = (ParamValue)((ParamStruct)nodeChild).Nodes[dictKey];
                        var idStr = Hash40Util.FormatToString((ulong)(id.Value), _paramHashes);
                        var newObjInstance = Activator.CreateInstance(newDictObj.Item2);
                        newListAdd.Invoke(newList, new[] { idStr, newObjInstance });
                        ReadPrc(nodeChild, newObjInstance, null);
                    }
                }
            }
            else if (node is ParamStruct)
            {
                if (node.TypeKey != ParamType.@struct)
                    throw new Exception("Data error");

                var nodeStruct = node as ParamStruct;
                ReadPrc(nodeStruct.Nodes, objToMap);
            }
            else if (node is ParamValue)
            {
                if (node.TypeKey == ParamType.@struct || node.TypeKey == ParamType.list)
                    throw new Exception("Data error");

                var nodeValue = node as ParamValue;
                if (nodeValue.TypeKey == ParamType.hash40)
                {
                    propertyInfo.SetValue(objToMap, GetStringFromHex((ulong)nodeValue.Value));
                }
                else
                {
                    propertyInfo.SetValue(objToMap, nodeValue.Value);
                }
            }
        }

        private object WriteNewFilterStruct(object objToMap, Type objType, string propertyName, ulong hexKey)
        {
            var propertyInfo = objType.GetProperty(propertyName);
            var filterStruct = propertyInfo.GetValue(objToMap);

            //Retrieve PcrFilterStruct flagged property. If null, create it
            if (filterStruct == null)
            {
                filterStruct = Activator.CreateInstance(propertyInfo.PropertyType);
                propertyInfo.SetValue(objToMap, filterStruct);
            }

            //Add a new entry
            var filterStructType = filterStruct.GetType();
            var filterStructAddMethod = filterStructType.GetMethod("Add");
            var filterStructObjType = GetListObjectType(filterStructType);
            var newfilterStructObjInstance = Activator.CreateInstance(filterStructObjType);
            filterStructAddMethod.Invoke(filterStruct, new[] { newfilterStructObjInstance });

            //Set ID
            filterStructObjType.GetProperty("Id").SetValue(newfilterStructObjInstance, GetStringFromHex(hexKey));

            return newfilterStructObjInstance;
        }
        #endregion

        #region Save
        private void WritePrc(Hash40Pairs<IParam> nodes, object inputObj, ulong hexKey, PropertyInfo inputObjPropertyInfo)
        {
            if (IsTypeAList(inputObjPropertyInfo.PropertyType))
            {
                var newList = new ParamList();
                nodes.Add(new HashValuePair<IParam>(hexKey, newList));

                var inputObjList = (IEnumerable)inputObjPropertyInfo.GetValue(inputObj, null);
                var inputObjListProperties = GetListObjectType(inputObjList.GetType()).GetProperties();

                foreach (var inputObjEntry in inputObjList)
                {
                    var newStructList = new ParamStruct();
                    newList.Nodes.Add(newStructList);
                    WritePrc(newStructList, inputObjEntry, inputObjListProperties);
                }
            }
            else if (IsTypeADictionary(inputObjPropertyInfo.PropertyType))
            {
                var newList = new ParamList();
                nodes.Add(new HashValuePair<IParam>(hexKey, newList));

                var inputObjList = (IDictionary)inputObjPropertyInfo.GetValue(inputObj, null);
                var inputObjListProperties = GetDictionaryObjectType(inputObjList.GetType()).Item2.GetProperties();

                foreach (var inputObjEntry in inputObjList.Values)
                {
                    var newStructList = new ParamStruct();
                    newList.Nodes.Add(newStructList);
                    WritePrc(newStructList, inputObjEntry, inputObjListProperties);
                }
            }
            else
            {
                var paramType = GetParamTypeFromProperty(inputObjPropertyInfo);
                var propertyValue = inputObjPropertyInfo.GetValue(inputObj);
                ParamValue newValue;
                if (paramType == ParamType.hash40)
                {
                    newValue = new ParamValue(paramType, GetHexFromString((string)propertyValue));
                }
                else if (propertyValue == null && paramType == ParamType.@string)
                {
                    newValue = new ParamValue(paramType, string.Empty);
                }
                else
                {
                    newValue = new ParamValue(paramType, propertyValue);
                }
                nodes.Add(new HashValuePair<IParam>(hexKey, newValue));
            }
        }

        private void WritePrc(ParamStruct paramsStruct, object inputObj, PropertyInfo[] inputObjProperties)
        {
            foreach (var inputObjPropertyInfo in inputObjProperties)
            {
                var prcFilter = MapPrcFilterFromPropertyInfo(inputObjPropertyInfo);

                //Regular case
                if (prcFilter == null)
                {
                    if (!ShouldIgnore(inputObjPropertyInfo))
                    {
                        var hexKey = MapHexKeyFromPropertyInfo(inputObjPropertyInfo);
                        WritePrc(paramsStruct.Nodes, inputObj, hexKey, inputObjPropertyInfo);
                    }
                }
                //Prc filter
                else
                {
                    var prcFilterList = (IEnumerable)inputObjPropertyInfo.GetValue(inputObj, null);
                    if (prcFilterList != null)
                    {
                        var prcFilterInstanceType = GetListObjectType(prcFilterList.GetType());
                        var prcFilterValuesPropertyType = prcFilterInstanceType.GetProperty("Values");
                        foreach (var prcFilterInstance in prcFilterList)
                        {
                            var hexKeyString = (string)prcFilterInstanceType.GetProperty("Id").GetValue(prcFilterInstance);
                            WritePrc(paramsStruct.Nodes, prcFilterInstance, GetHexFromString(hexKeyString), prcFilterValuesPropertyType);
                        }
                    }
                }
            }
        }
        #endregion

        #region Utils
        private string GetStringFromHex(ulong hexValue)
        {
            return _paramHashes.ContainsKey(hexValue) ? _paramHashes[hexValue] : $"0x{hexValue:x}";
        }

        private ulong GetHexFromString(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
                return 0;
            if (stringValue.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
                return Convert.ToUInt64(stringValue, 16);
            return Hash40Util.StringToHash40(stringValue);
        }

        private Type GetListObjectType(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    return type.GetGenericArguments()[0];
            }
            return null;
        }

        private Tuple<Type, Type> GetDictionaryObjectType(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return new Tuple<Type, Type>(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
            }
            return null;
        }

        private Dictionary<ulong, string> MapOffsetToProperties(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<PrcHexMapping>() != null).ToDictionary(p => p.GetCustomAttribute<PrcHexMapping>().Value, p => p.Name);
        }

        private Dictionary<string, PrcFilterMatch> MapOffsetToRegexFilters(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttribute<PrcFilterMatch>() != null).ToDictionary(p => p.Name, p => p.GetCustomAttribute<PrcFilterMatch>());
        }

        private string MapDictionaryKeyFromPropertyInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcDictionary>()?.Key;
        }

        private ulong MapHexKeyFromPropertyInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcHexMapping>().Value;
        }
        private bool IsHash40FromPropertyInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcHexMapping>().IsHash40;
        }

        private bool ShouldIgnore(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcIgnore>() != null;
        }

        private PrcFilterMatch MapPrcFilterFromPropertyInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcFilterMatch>();
        }

        private bool IsTypeAList(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    return true;
            }
            return false;
        }

        private bool IsTypeADictionary(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return true;
            }
            return false;
        }

        private ParamType GetParamTypeFromProperty(PropertyInfo propertyInfo)
        {
            var typeProperty = propertyInfo.PropertyType;

            if (typeProperty == typeof(bool))
            {
                return ParamType.@bool;
            }
            if (typeProperty == typeof(byte))
            {
                return ParamType.@byte;
            }
            if (typeProperty == typeof(float))
            {
                return ParamType.@float;
            }
            if (typeProperty == typeof(int))
            {
                return ParamType.@int;
            }
            if (typeProperty == typeof(sbyte))
            {
                return ParamType.@sbyte;
            }
            if (typeProperty == typeof(short))
            {
                return ParamType.@short;
            }
            if (typeProperty == typeof(uint))
            {
                return ParamType.@uint;
            }
            if (typeProperty == typeof(ushort))
            {
                return ParamType.@ushort;
            }
            if (IsHash40FromPropertyInfo(propertyInfo)) //Has to be done before string!!
            {
                return ParamType.hash40;
            }
            if (typeProperty == typeof(string))
            {
                return ParamType.@string;
            }

            throw new Exception("Case non handled");
        }
        #endregion
    }
}
