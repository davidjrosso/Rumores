using System;
using System.IO;
using System.Text.RegularExpressions;
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
            PlayerSettings.companyName="David J. Rosso";
            PlayerSettings.productName="Rumores";
            PlayerSettings.bundleVersion="0.1.0";
            string id=Environment.GetEnvironmentVariable("RUMORES_APPLICATION_ID")??"com.davidjrosso.rumores";
            if(!Regex.IsMatch(id,@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$"))throw new ArgumentException("RUMORES_APPLICATION_ID inválido.");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,id);
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android,ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.Android.forceInternetPermission=false;
            PlayerSettings.Android.useCustomKeystore=false;
            EditorUserBuildSettings.buildAppBundle=false;
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Rumores/Compilar APK de desarrollo")]
        public static void Android()
        {
            ValidateContent();ConfigureAndroid();
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("Instalá Android Build Support, SDK/NDK y OpenJDK de Unity Hub.");
            if(!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("No se pudo seleccionar Android.");
            Directory.CreateDirectory("Builds");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{Scene},locationPathName="Builds/Rumores-dev.apk",target=BuildTarget.Android,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Falló la compilación Android: "+report.summary.result);
            Debug.Log("APK generado: Builds/Rumores-dev.apk · "+report.summary.totalSize+" bytes.");
        }
    }
}
