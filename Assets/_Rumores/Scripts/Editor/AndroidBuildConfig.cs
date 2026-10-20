using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Rumores.Editor
{
    // No Unity dependencies: validate every override before modifying PlayerSettings.
    public sealed class AndroidBuildConfig
    {
        public string ApplicationId {get;private set;}
        public string Version {get;private set;}
        public int VersionCode {get;private set;}
        public int TargetApi {get;private set;}

        public static AndroidBuildConfig Read(Func<string,string> environment,
            string currentId,string currentVersion,int currentCode,int currentTarget)
        {
            var config=new AndroidBuildConfig {
                ApplicationId=environment("RUMORES_APPLICATION_ID")??currentId,
                Version=environment("RUMORES_VERSION")??currentVersion,
                VersionCode=Number(environment("RUMORES_VERSION_CODE"),currentCode,1,2100000000,"RUMORES_VERSION_CODE"),
                TargetApi=Number(environment("RUMORES_TARGET_API"),currentTarget,0,99,"RUMORES_TARGET_API")
            };
            if(string.IsNullOrEmpty(config.ApplicationId)||
                !Regex.IsMatch(config.ApplicationId,@"\A[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+\z"))
                throw new ArgumentException("Identificador Android inválido. Usá segmentos en minúsculas separados por puntos.");
            if(string.IsNullOrWhiteSpace(config.Version)||config.Version.Length>64||
                !Regex.IsMatch(config.Version,@"\A[0-9A-Za-z][0-9A-Za-z.+_-]*\z"))
                throw new ArgumentException("RUMORES_VERSION inválida.");
            if(config.TargetApi!=0&&config.TargetApi<23)
                throw new ArgumentException("RUMORES_TARGET_API debe ser 0 (Auto) o al menos 23.");
            return config;
        }
        static int Number(string value,int fallback,int min,int max,string name)
        {
            int parsed=fallback;
            if(value!=null&&!int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out parsed))
                throw new ArgumentException(name+" debe ser un entero sin espacios.");
            if(parsed<min||parsed>max)throw new ArgumentException(name+" fuera de rango.");
            return parsed;
        }
    }
}
