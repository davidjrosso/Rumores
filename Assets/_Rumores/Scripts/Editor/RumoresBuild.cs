using System;
using System.IO;
using Rumores.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rumores.Editor
{
    public static class RumoresBuild
    {
        const string Scene="Assets/_Rumores/Scenes/Main.unity";
        public const string IconPath="Assets/_Rumores/Art/AppIcon.png";
        [MenuItem("Rumores/Validar contenido")]
        public static void ValidateContent()
        {
            var asset=AssetDatabase.LoadAssetAtPath<PartyCatalogAsset>("Assets/_Rumores/Resources/PartyCatalog.asset");
            if(asset==null)throw new InvalidOperationException("Falta el catálogo.");
            var catalog=asset.Load();Debug.Log("Rumores: "+catalog.news.Count+" noticias, "+catalog.actions.Count+" maneras de contar el chisme validadas.");
        }
        [MenuItem("Rumores/Configurar Android")]
        public static void ConfigureAndroid()
        {
            var config=AndroidBuildConfig.Read(Environment.GetEnvironmentVariable,
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
                PlayerSettings.bundleVersion,PlayerSettings.Android.bundleVersionCode,
                (int)PlayerSettings.Android.targetSdkVersion);
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if(icon==null)throw new InvalidOperationException("Falta importar el icono: "+IconPath);
            PlayerSettings.companyName="David J. Rosso";
            PlayerSettings.productName="Rumores";
            PlayerSettings.bundleVersion=config.Version;
            PlayerSettings.Android.bundleVersionCode=config.VersionCode;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,config.ApplicationId);
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)config.TargetApi;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android,ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.Android.forceInternetPermission=false;
            // Default icon also provides Android's legacy fallback. Adaptive layers require a device review.
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Rumores/Compilar APK de desarrollo")]
        public static void Android()
        {
            Build(false);
        }
        [MenuItem("Rumores/Compilar AAB firmado")]
        public static void AndroidBundle()
        {
            Build(true);
        }
        static void Build(bool bundle)
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Instalá Android Build Support, SDK/NDK y OpenJDK de Unity Hub.");
            // Switching here can reload scripts mid-method. Batch callers use -buildTarget Android.
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
                throw new InvalidOperationException("Seleccioná Android y Switch Platform antes de compilar; en batch usá -buildTarget Android.");
            ValidateContent();ConfigureAndroid();
            if(bundle)
            {
                if(PlayerSettings.Android.targetSdkVersion==AndroidSdkVersions.AndroidApiLevelAuto)
                    throw new InvalidOperationException("Para release fijá Target API explícitamente después de consultar las políticas actuales de Play.");
                string key=PlayerSettings.Android.keystoreName;
                if(!PlayerSettings.Android.useCustomKeystore||string.IsNullOrWhiteSpace(key)||
                    !Path.IsPathRooted(key)||!File.Exists(key)||
                    string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName)||
                    string.IsNullOrEmpty(PlayerSettings.Android.keystorePass)||
                    string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
                    throw new InvalidOperationException("Configurá la upload key y sus contraseñas en Player Settings > Publishing Settings. Usá una ruta absoluta fuera del proyecto.");
                string project=Path.GetFullPath(".")+Path.DirectorySeparatorChar;
                if(Path.GetFullPath(key).StartsWith(project,StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Guardá la clave de firma fuera del proyecto.");
            }
            Directory.CreateDirectory("Builds");
            string output=bundle?"Builds/Rumores-release.aab":"Builds/Rumores-dev.apk";
            bool previousBundle=EditorUserBuildSettings.buildAppBundle;
            bool previousSigning=PlayerSettings.Android.useCustomKeystore;
            try
            {
                if(File.Exists(output))File.Delete(output);
                EditorUserBuildSettings.buildAppBundle=bundle;
                if(!bundle)PlayerSettings.Android.useCustomKeystore=false;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes=new[]{Scene},locationPathName=output,target=BuildTarget.Android,
                    options=bundle?BuildOptions.None:BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded||!File.Exists(output)||new FileInfo(output).Length==0)
                    throw new InvalidOperationException("Falló la compilación Android: "+report.summary.result);
                Debug.Log("Compilación creada: "+output+" · "+report.summary.totalSize+" bytes. Pendiente de instalar y probar.");
            }
            catch
            {
                if(File.Exists(output))File.Delete(output);
                throw;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle=previousBundle;
                PlayerSettings.Android.useCustomKeystore=previousSigning;
            }
        }
    }
}
