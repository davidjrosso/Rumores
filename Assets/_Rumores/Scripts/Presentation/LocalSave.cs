using System;
using System.IO;
using Rumores.Core;
using UnityEngine;

namespace Rumores.Presentation
{
    public sealed class LocalSave
    {
        readonly string path;
        public bool ReadOnly {get;private set;}
        public string Notice {get;private set;}
        public LocalSave(string directory) {Directory.CreateDirectory(directory);path=Path.Combine(directory,"rumores-save.json");}
        public SaveData Load(Catalog catalog)
        {
            foreach(var candidate in new[]{path,path+".bak"})
            {
                if(!File.Exists(candidate)) continue;
                try
                {
                    var text=File.ReadAllText(candidate);
                    if(!text.TrimStart().StartsWith("{")) throw new InvalidDataException("JSON inválido.");
                    var data=JsonUtility.FromJson<SaveData>(text);
                    if(data!=null && (data.version>1||data.rulesVersion>catalog.rulesVersion))
                    {ReadOnly=true;Notice="Hay una partida de una versión más nueva. Se conserva el archivo; esta sesión no se guardará.";return new SaveData();}
                    var errors=SaveRules.Validate(data,catalog);
                    if(errors.Count!=0) throw new InvalidDataException(string.Join("; ",errors));
                    if(candidate!=path) Notice="Se recuperó la partida desde la copia de respaldo.";
                    return data;
                }
                catch(Exception ex) when(ex is IOException || ex is ArgumentException || ex is InvalidOperationException || ex is NullReferenceException)
                {Notice="No se pudo leer el guardado. Se intentó recuperar la copia de respaldo.";}
            }
            return new SaveData();
        }
        public bool Write(SaveData data)
        {
            if(ReadOnly) return false;
            try
            {
                string temporary=path+".tmp";
                File.WriteAllText(temporary,JsonUtility.ToJson(data,true));
                if(File.Exists(path)) File.Replace(temporary,path,path+".bak");
                else File.Move(temporary,path);
                return true;
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            {Notice="No se pudo guardar: "+ex.Message;return false;}
        }
    }
}
