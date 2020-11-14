using paracobNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Sm5sh.PrcParser
{
    public class PrcParser
    {
        private readonly Dictionary<ulong, string> _paramHashes;

        public PrcParser(Dictionary<ulong, string> paramHashes)
        {
            _paramHashes = paramHashes;
        }

        public T ReadPrcFile<T>(string inputFile, Dictionary<ulong, string> paramHashes = null) where T : IPrcParsable, new()
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

        public bool SavePrcFile<T>(string outputFile, T inputObj) where T : IPrcParsable
        {
            //Open file
            var t = new ParamFile();

            //Write recursively
            WritePrc(t.Root, inputObj, inputObj.GetType().GetProperties());

            //Save
            t.Save(outputFile);

            //Return success
            return true;
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

                    foreach(var filterRegex in filtersRegex) 
                    {
                        if (Regex.IsMatch(keyString, filterRegex.Value.Regex))
                        {
                            var newObjToMap = WriteNewFilterStruct(objToMap, objType, filterRegex.Key, node.Key);
                            propertyInfo = newObjToMap.GetType().GetProperty("Values");
                            ReadPrc(node.Value, newObjToMap, propertyInfo);
                            break;
                        }
                    }

                    if(propertyInfo == null)
                        throw new Exception($"Hex 0x{node.Key:X} was not mapped to any property.");
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
            filterStructObjType.GetProperty("Id").SetValue(newfilterStructObjInstance, new PrcHash40(hexKey, _paramHashes));

            return newfilterStructObjInstance;
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
                var newListObj = GetListObjectType(newList.GetType());
                var newListAdd = newList.GetType().GetMethod("Add");

                //Set in property
                propertyInfo.SetValue(objToMap, newList);
                foreach (var nodeChild in nodeList.Nodes)
                {
                    var newObjInstance = Activator.CreateInstance(newListObj);
                    newListAdd.Invoke(newList, new[] { newObjInstance });
                    ReadPrc(nodeChild, newObjInstance, null);
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
                if(nodeValue.TypeKey == ParamType.hash40)
                {
                    propertyInfo.SetValue(objToMap, new PrcHash40((ulong)nodeValue.Value, _paramHashes));
                }
                else
                {
                    propertyInfo.SetValue(objToMap, nodeValue.Value);
                }
                
            }
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
            else
            {
                var paramType = GetParamTypeFromProperty(inputObjPropertyInfo);
                ParamValue newValue;
                if (paramType == ParamType.hash40)
                {
                    newValue = new ParamValue(paramType, ((PrcHash40)inputObjPropertyInfo.GetValue(inputObj)).HexValue);
                }
                else
                {
                    newValue = new ParamValue(paramType, inputObjPropertyInfo.GetValue(inputObj));
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
                    var hexKey = MapHexKeyFromPropertyInfo(inputObjPropertyInfo);
                    WritePrc(paramsStruct.Nodes, inputObj, hexKey, inputObjPropertyInfo);
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
                            var hexKey = (PrcHash40)prcFilterInstanceType.GetProperty("Id").GetValue(prcFilterInstance);
                            WritePrc(paramsStruct.Nodes, prcFilterInstance, hexKey.HexValue, prcFilterValuesPropertyType);
                        }
                    }
                }
            }
        }
        #endregion

        #region Utils
        private Type GetListObjectType(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    return type.GetGenericArguments()[0];
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

        private ulong MapHexKeyFromPropertyInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<PrcHexMapping>().Value;
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
            if (typeProperty == typeof(PrcHash40))
            {
                return ParamType.hash40;
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
            if (typeProperty == typeof(string))
            {
                return ParamType.@string;
            }
            if (typeProperty == typeof(uint))
            {
                return ParamType.@uint;
            }
            if (typeProperty == typeof(ushort))
            {
                return ParamType.@ushort;
            }

            throw new Exception("Case non handled");
        }
        #endregion
    }
}
