using System;
using System.Collections.Generic;
using System.Data;
using nVentive.Umbrella.Extensions;

namespace SimpleQuery.Impl {
    public static class DTOMapper {
        public static T Map<T>(IDataReader reader) where T : new() {
            T item = default(T);

            if (reader.Read()) {
                item = new T();
                ProcessSimpleDTO(item, reader);
            }

            return item;
        }

        public static IEnumerable<T> MapSet<T>(IDataReader reader) where T : new() {
            List<T> resVal = new List<T>();

            while (reader.Read()) {
                T item = new T();
                ProcessSimpleDTO(item, reader);
                resVal.Add(item);
            }

            return resVal;
        }

        private static void ProcessSimpleDTO(object instance, IDataReader reader) {
            ProcessSimpleDTO(instance, reader, 0);
        }

        private static void ProcessSimpleDTO(object instance, IDataReader reader, int startField) {
            for (int i = startField; i < reader.FieldCount; i++) {
                ProcessDTOProperty(reader.GetName(i).Split('.'), instance, reader.GetValue(i));
            }
        }

        private static void ProcessDTOProperty(string[] path, object instance, object value) {
            if (instance.Reflection().FindDescriptor(path[0]) != null) {
                if (value == DBNull.Value) {
                    value = null;
                }

                if (path.Length == 1) {
                    instance.Reflection().Set(path[0], value);
                } else {
                    var subInstance = instance.Reflection().Get(path[0]);

                    if (subInstance == null) {
                        subInstance = Activator.CreateInstance(instance.Reflection().FindDescriptor(path[0]).Type);
                        instance.Reflection().Set(path[0], subInstance);
                    }

                    string[] subPath = new string[path.Length - 1];
                    Array.Copy(path, 1, subPath, 0, path.Length - 1);
                    ProcessDTOProperty(subPath, subInstance, value);
                }
            }
        }
    }
}
