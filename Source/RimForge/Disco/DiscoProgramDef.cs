using RimForge.Disco;
using RimForge.Disco.Programs;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class DiscoProgramDef : Def
    {
        public Type programClass;
        private DiscoDict inputs = new DiscoDict();

        [XmlIgnore]
        private Dictionary<string, object> parsed = new Dictionary<string, object>();

        public T Get<T>(string inputName, T defaultValue = default)
        {
            if (inputName == null)
                return defaultValue;
            inputName = inputName.Trim();

            if (parsed.TryGetValue(inputName, out var found))
            {
                if (found is T t)
                    return t;

                Core.Warn($"Tried to get input '{inputName}' as type {typeof(T).Name}, but it was previously loaded as a {found.GetType().Name}");
                return defaultValue;
            }

            if (!inputs.TryGetValue(inputName, out var rawText))
            {
                //Core.Error($"Did not find input called '{inputName}'");
                return defaultValue;
            }

            if (typeof(T).IsArray)
            {
                var arr = TryParseArray(rawText, typeof(T));
                parsed.Add(inputName, arr);
                return (T)(object)arr;
            }

            if (TryParse(rawText, typeof(T), out object created))
            {
                parsed.Add(inputName, created);
                return (T)created;
            }
            return defaultValue;
        }

        public T Get<T>(string inputName, int index, T defaultValue = default)
        {
            if (index < 0)
                return defaultValue;

            if (inputName == null)
                return defaultValue;
            inputName = inputName.Trim();

            if (parsed.TryGetValue(inputName, out var found))
            {
                if (found is Array arr)
                {
                    if (index >= arr.Length)
                    {
                        Core.Error($"Index {index} is out of bounds for input array '{inputName}' (length {arr.Length})");
                        return defaultValue;
                    }
                    return (T)arr.GetValue(index);
                }

                Core.Warn($"Tried to get item at index {index} of input '{inputName}', but that input is not an array!");
                return defaultValue;
            }

            if (!inputs.TryGetValue(inputName, out var rawText))
            {
                Core.Error($"Did not find input array called '{inputName}'");
                return defaultValue;
            }

            var arr2 = TryParseArray(rawText, typeof(T).MakeArrayType());
            parsed.Add(inputName, arr2);
            if (index >= arr2.Length)
            {
                Core.Error($"Index {index} is out of bounds for input array '{inputName}' (length {arr2.Length})");
                return defaultValue;
            }
            return (T)arr2.GetValue(index);
        }

        protected Array TryParseArray(string rawText, Type arrayType, object defaultValue = null)
        {
            string[] parts = rawText.Split(':');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            Type type = arrayType.GetElementType();
            Array arr = Array.CreateInstance(type, parts.Length);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].NullOrEmpty())
                    Core.Warn($"Empty item in input array: '{rawText}'");

                bool worked = TryParse(parts[i], type, out object created);
                arr.SetValue(worked ? created : defaultValue, i);
            }

            return arr;
        }

        protected virtual bool TryParse(string rawText, Type type, out object value)
        {
            bool worked;

            if (type == typeof(int))
            {
                worked = int.TryParse(rawText, out int l);
                value = l;
                return worked;
            }
            if (type == typeof(float))
            {
                worked = float.TryParse(rawText, out float f);
                value = f;
                return worked;
            }
            if (type == typeof(bool))
            {
                worked = bool.TryParse(rawText, out bool b);
                value = b;
                return worked;
            }
            if (type == typeof(string))
            {
                if (rawText.StartsWith("\"") && rawText.EndsWith("\""))
                    rawText = rawText.Substring(1, rawText.Length - 2);

                value = rawText;
                return true;
            }
            if (type == typeof(Color))
            {
                try
                {
                    value = ParseHelper.ParseColor(rawText);
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
                return true;
            }
            if (type == typeof(IntVec2))
            {
                try
                {
                    value = ParseHelper.ParseIntVec2(rawText);
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
                return true;
            }
            if (type == typeof(IntVec3))
            {
                try
                {
                    value = ParseHelper.ParseIntVec3(rawText);
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
                return true;
            }
            if (type == typeof(Vector2))
            {
                try
                {
                    value = ParseHelper.FromStringVector2(rawText);
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
                return true;
            }
            if (type == typeof(Vector3))
            {
                try
                {
                    value = ParseHelper.FromStringVector3(rawText);
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
                return true;
            }

            Core.Error($"Tried to parse invalid parameter type: {type.Name}");
            value = null;
            return false;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(var item in base.ConfigErrors())
                yield return item;

            if (programClass == null)
                yield return "Null programClass. Check spelling and recompile?";
            else if (!typeof(DiscoProgram).IsAssignableFrom(programClass))
            {
                yield return $"programClass '{programClass.FullName}' is not a subclass of DiscoProgram. Expect errors.";
                programClass = null;
            }
        }

        public DiscoProgram MakeProgram(Building_DJStand stand)
        {
            if (programClass == null)
                return null;

            var instance = Activator.CreateInstance(programClass, this) as DiscoProgram;
            if (instance == null)
                return null;

            instance.DJStand = stand;
            instance.Init();
            return instance;
        }
    }
}
