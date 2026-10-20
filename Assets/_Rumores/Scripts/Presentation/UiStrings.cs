using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Rumores.Presentation
{
    public static class UiStrings
    {
        [Serializable] public sealed class Entry {public string key,value;}
        [Serializable] public sealed class Table {public Entry[] entries;}
        static Dictionary<string,string> entries;
        public static string T(string key,params object[] args)
        {
            if(entries==null)
            {
                var source=Resources.Load<TextAsset>("UiStrings.es-AR");
                if(source==null)throw new InvalidOperationException("Missing UI language table.");
                var loaded=new Dictionary<string,string>(StringComparer.Ordinal);
                foreach(var entry in JsonUtility.FromJson<Table>(source.text).entries)loaded.Add(entry.key,entry.value);
                entries=loaded;
            }
            if(!entries.TryGetValue(key,out var value))throw new InvalidOperationException("Missing UI string: "+key);
            return args.Length==0?value:string.Format(CultureInfo.InvariantCulture,value,args);
        }
    }
}
