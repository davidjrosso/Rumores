using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Separate review scene, not part of the gameplay or Android build flow.
    public sealed class ArtPreview : MonoBehaviour
    {
        [SerializeField] RumoresArt art;
        public CharacterView Character {get;private set;}
        public RumorSignal Signal {get;private set;}
        RectTransform safe;
        bool fast;
        Text status;
        void Start()
        {
            if(art==null){Debug.LogError("ArtPreview necesita RumoresArt.");return;}
            var cameraObject=new GameObject("PreviewCamera",typeof(Camera));cameraObject.transform.SetParent(transform,false);
            var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ui.Cream;camera.cullingMask=0;
            var go=new GameObject("ArtCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=1;
            Ui.Panel(go.transform,"Paper",Vector2.zero,Vector2.one,Ui.Cream);
            safe=Ui.Rect(go.transform,"SafeArea",Vector2.zero,Vector2.one);Update();
            Ui.Text(safe,"RUMORES · muestra artística",new Vector2(.04f,.93f),new Vector2(.96f,.99f),30,Ui.Teal);
            var town=Ui.Rect(safe,"Town",new Vector2(.04f,.56f),new Vector2(.96f,.92f));art.Town(town);
            Character=CharacterView.Create(safe,art,"miedoso",new Vector2(.04f,.27f),new Vector2(.40f,.55f),false,false);
            CharacterView.Create(safe,art,"miedoso",new Vector2(.49f,.44f),new Vector2(.66f,.54f),true,true);
            CharacterView.Create(safe,art,"exagerado",new Vector2(.73f,.44f),new Vector2(.90f,.54f),true,true);
            Ui.Text(safe,"Retrato / avatar provisional",new Vector2(.46f,.40f),new Vector2(.97f,.44f),18);
            Signal=Ui.Rect(safe,"Transmission",new Vector2(.44f,.34f),new Vector2(.95f,.40f)).gameObject.AddComponent<RumorSignal>();
            status=Ui.Text(safe,"Espera",new Vector2(.44f,.29f),new Vector2(.95f,.34f),22,Ui.Teal);
            Ui.Button(safe,"PreviewFast","Reducir movimiento",new Vector2(.44f,.24f),new Vector2(.96f,.29f),()=>{
                fast=!fast;Character.Motion.Fast=fast;status.text=fast?"Movimiento reducido":"Movimiento normal";
            },size:19);
            string[] labels={"Espera","Hablar","Sorpresa","Pánico","Risa","Festejo","Desánimo","Transmitir","Dividir"};
            for(int i=0;i<labels.Length;i++)
            {
                int index=i;float x=.03f+(i%3)*.315f,y=.16f-(i/3)*.065f;
                Ui.Button(safe,"Preview_"+i,labels[i],new Vector2(x,y),new Vector2(x+.31f,y+.06f),()=>{
                    status.text=labels[index];
                    if(index<7)Character.Motion.Play((CharacterReaction)index);
                    else {Character.Motion.Play(CharacterReaction.Talk);Signal.Play(index==8,fast);}
                },size:22);
            }
            Ui.Text(safe,"Prueba visual · sin cambios en las reglas",new Vector2(.04f,0),new Vector2(.96f,.03f),17);
            if(FindObjectOfType<EventSystem>()==null)
            {var events=new GameObject("PreviewEventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform,false);}
        }
        void Update()
        {
            if(safe==null||Screen.width==0||Screen.height==0)return;
            var area=Screen.safeArea;
            safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);
            safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
        }
    }
}
