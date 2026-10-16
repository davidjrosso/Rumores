using Rumores.Core;
using UnityEngine;

namespace Rumores.Presentation
{
    public sealed class UnitySaveJson : ISaveJson
    {
        public string Serialize<T>(T value) {return JsonUtility.ToJson(value,true);}
        public T Deserialize<T>(string json) {return JsonUtility.FromJson<T>(json);}
    }

    public sealed class LocalSave
    {
        readonly SaveStorage storage;
        public bool ReadOnly {get {return storage.ReadOnly;}}
        public string Notice {get {return storage.Notice;}}
        public LocalSave(string directory) {storage=new SaveStorage(directory,new UnitySaveJson());}
        public SaveData Load(Catalog catalog) {return storage.Load(catalog);}
        public bool Write(SaveData data) {return storage.Write(data);}
    }
}
