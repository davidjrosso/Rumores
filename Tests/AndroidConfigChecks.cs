using System;
using System.Collections.Generic;
using Rumores.Editor;

namespace Rumores.Tests
{
    static class AndroidConfigChecks
    {
        public static Dictionary<string,Action> Cases()
        {
            var cases=new Dictionary<string,Action>();
            cases["Android: conserva identificador, versión, código y API configurados"]=()=>{
                var config=Read();
                CoreChecks.Assert(config.ApplicationId=="com.ejemplo.rumores"&&config.Version=="1.2.3"&&config.VersionCode==42&&config.TargetApi==35);
            };
            cases["Android: overrides explícitos y Auto solo si se pide"]=()=>{
                var config=Read(new Dictionary<string,string>{
                    {"RUMORES_APPLICATION_ID","ar.estudio.rumores"},{"RUMORES_VERSION","2.0.0-qa"},
                    {"RUMORES_VERSION_CODE","43"},{"RUMORES_TARGET_API","0"}});
                CoreChecks.Assert(config.ApplicationId=="ar.estudio.rumores"&&config.Version=="2.0.0-qa"&&config.VersionCode==43&&config.TargetApi==0);
            };
            cases["Android: rechaza identificadores mal formados"]=()=>Reject("RUMORES_APPLICATION_ID","", "Rumores","com..rumores","com.4rumores","com.rumores\n","com.ru-mores");
            cases["Android: rechaza versiones vacías y saltos de línea"]=()=>Reject("RUMORES_VERSION",""," ","1.0\n",new string('a',65));
            cases["Android: rechaza códigos inválidos antes de configurar"]=()=>Reject("RUMORES_VERSION_CODE","0","-1","2147483648","2100000001"," 2","2x","");
            cases["Android: rechaza API por debajo del mínimo y valores inválidos"]=()=>Reject("RUMORES_TARGET_API","22","-1","100","Auto","35\n","");
            cases["Android: admite límites de código y API mínima"]=()=>{
                var config=Read(new Dictionary<string,string>{{"RUMORES_VERSION_CODE","2100000000"},{"RUMORES_TARGET_API","23"}});
                CoreChecks.Assert(config.VersionCode==2100000000&&config.TargetApi==23);
            };
            return cases;
        }
        static AndroidBuildConfig Read(Dictionary<string,string> values=null)
        {
            return AndroidBuildConfig.Read(key=>values!=null&&values.TryGetValue(key,out var value)?value:null,
                "com.ejemplo.rumores","1.2.3",42,35);
        }
        static void Reject(string key,params string[] invalid)
        {
            foreach(string value in invalid)
            {
                bool rejected=false;
                try {Read(new Dictionary<string,string>{{key,value}});}
                catch(ArgumentException){rejected=true;}
                CoreChecks.Assert(rejected,key+" aceptó un valor inválido.");
            }
        }
    }
}
