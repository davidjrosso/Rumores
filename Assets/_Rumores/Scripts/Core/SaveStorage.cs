using System;
using System.Collections.Generic;
using System.IO;

namespace Rumores.Core
{
    public interface ISaveJson
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }

    [Serializable] public sealed class SaveFileHeader
    {
        public int fileVersion,version,rulesVersion;
    }

    [Serializable] public sealed class SaveDocument
    {
        public int fileVersion=2,version=1,rulesVersion=1;
        public Settings settings=new Settings();
        public bool mayorTitle,hasRun;
        // Unity's inline serializer cannot reliably preserve a null RunState.
        // A presence flag plus a JSON string keeps the no-run state explicit.
        public string runJson="";
    }

    public sealed class FutureSaveException : InvalidOperationException { }

    public sealed class SaveCodec
    {
        readonly ISaveJson json;
        public SaveCodec(ISaveJson json) {this.json=json;}
        public string Encode(SaveData data)
        {
            return json.Serialize(new SaveDocument {
                version=data.version,rulesVersion=data.rulesVersion,settings=data.settings??new Settings(),
                mayorTitle=data.mayorTitle,hasRun=data.run!=null,
                runJson=data.run==null?"":json.Serialize(data.run)
            });
        }
        public SaveData Decode(string text,int rulesVersion)
        {
            if(string.IsNullOrWhiteSpace(text)||!text.TrimStart().StartsWith("{",StringComparison.Ordinal))
                throw new InvalidDataException("JSON inválido.");
            var header=json.Deserialize<SaveFileHeader>(text);
            if(header==null)throw new InvalidDataException("Falta la cabecera del guardado.");
            if(header.fileVersion>2||header.version>1||header.rulesVersion>rulesVersion)throw new FutureSaveException();
            if(header.version!=1||header.rulesVersion!=rulesVersion)throw new InvalidDataException("Versión de guardado o reglas no compatible.");
            if(header.fileVersion==0)
            {
                var legacy=json.Deserialize<SaveData>(text);
                if(legacy==null)throw new InvalidDataException("Guardado vacío.");
                // Only the entirely unstarted legacy placeholder represents null.
                // A populated/incompatible map must still fail normal validation.
                if(IsUnstartedPlaceholder(legacy.run))legacy.run=null;
                return legacy;
            }
            if(header.fileVersion!=2)throw new InvalidDataException("Formato de archivo desconocido.");
            var document=json.Deserialize<SaveDocument>(text);
            if(document==null)throw new InvalidDataException("Documento vacío.");
            if(document.hasRun&&string.IsNullOrWhiteSpace(document.runJson))throw new InvalidDataException("Falta la partida activa.");
            var run=document.hasRun?json.Deserialize<RunState>(document.runJson):null;
            if(document.hasRun&&run==null)throw new InvalidDataException("Partida activa vacía.");
            return new SaveData {version=document.version,rulesVersion=document.rulesVersion,
                settings=document.settings??new Settings(),mayorTitle=document.mayorTitle,run=run};
        }
        static bool Empty<T>(List<T> list) {return list==null||list.Count==0;}
        static bool IsUnstartedPlaceholder(RunState r)
        {
            return r!=null&&r.seed==0&&r.stage==0&&r.phase==Phase.Map&&!r.won&&
                r.wins==0&&r.totalStars==0&&r.failures==0&&(r.patience==0||r.patience==3)&&
                string.IsNullOrEmpty(r.currentNode)&&Empty(r.map)&&Empty(r.edges)&&Empty(r.decisions)&&
                Empty(r.visited)&&Empty(r.chosenRewards)&&Empty(r.rewardIds)&&
                (r.bonus==null||(r.bonus.whispers==0&&r.bonus.exaggeratorImportance==0&&
                    r.bonus.liarCredibility==0&&r.bonus.exaggeratorOutputs==0));
        }
    }

    public sealed class SaveStorage
    {
        readonly string path;
        readonly SaveCodec codec;
        bool primaryValidated;
        public bool ReadOnly {get;private set;}
        public string Notice {get;private set;}
        public SaveStorage(string directory,ISaveJson json)
        {Directory.CreateDirectory(directory);path=Path.Combine(directory,"rumores-save.json");codec=new SaveCodec(json);}
        public SaveData Load(Catalog catalog)
        {
            ReadOnly=false;Notice=null;primaryValidated=false;
            foreach(var candidate in new[]{path,path+".bak"})
            {
                if(!File.Exists(candidate))continue;
                try
                {
                    var data=codec.Decode(File.ReadAllText(candidate),catalog.rulesVersion);
                    var errors=SaveRules.Validate(data,catalog);
                    if(errors.Count>0)throw new InvalidDataException(string.Join("; ",errors));
                    primaryValidated=candidate==path;
                    if(candidate!=path)Notice=ReadOnly?"Se recuperó el respaldo, pero esta sesión no se guardará porque no se pudo preservar el archivo anterior.":"Se recuperó la partida desde la copia de respaldo.";
                    return data;
                }
                catch(FutureSaveException)
                {ReadOnly=true;Notice="Hay una partida de una versión más nueva. Se conserva el archivo; esta sesión no se guardará.";return new SaveData();}
                catch(Exception ex) when(ex is InvalidDataException || ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException || ex is NullReferenceException)
                {
                    try {File.Copy(candidate,candidate+".invalid-"+Guid.NewGuid().ToString("N"));}
                    catch(Exception copyError) when(copyError is IOException||copyError is UnauthorizedAccessException) {ReadOnly=true;}
                    Notice="El guardado no es compatible o está dañado. Se conservó el original y se iniciará sin una run activa.";
                    if(ReadOnly)Notice="No se pudo preservar el guardado anterior. Se puede jugar, pero esta sesión no se guardará.";
                }
            }
            return new SaveData();
        }
        public bool Write(SaveData data)
        {
            if(ReadOnly)return false;
            try
            {
                string temporary=path+".tmp";
                File.WriteAllText(temporary,codec.Encode(data));
                // Never replace a valid backup with the rejected primary file.
                if(File.Exists(path))File.Replace(temporary,path,primaryValidated?path+".bak":null);
                else File.Move(temporary,path);
                primaryValidated=true;return true;
            }
            catch(Exception ex) when(ex is IOException||ex is UnauthorizedAccessException)
            {Notice="No se pudo guardar: "+ex.Message;return false;}
        }
    }
}
