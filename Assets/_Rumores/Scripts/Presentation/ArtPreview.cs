using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Rumores.Presentation.UiStrings;

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
        ReactionBurstGraphic burst;
        static readonly string[] CharacterIds={"miedoso","exagerado","mentiroso","ignorante"};
        PartyAudio sound;
        void Start()
        {
            if(art==null){Debug.LogError("ArtPreview necesita RumoresArt.");return;}
            sound=gameObject.AddComponent<PartyAudio>();
            var cameraObject=new GameObject("PreviewCamera",typeof(Camera));cameraObject.transform.SetParent(transform,false);
            var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ui.Cream;camera.cullingMask=0;
            var go=new GameObject("ArtCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=1;
            Ui.Panel(go.transform,"Paper",Vector2.zero,Vector2.one,Ui.Cream);
            safe=Ui.Rect(go.transform,"SafeArea",Vector2.zero,Vector2.one);Update();
            Ui.Text(safe,T("art.title"),new Vector2(.04f,.93f),new Vector2(.96f,.99f),30,Ui.Teal);
            var town=Ui.Rect(safe,"Town",new Vector2(.04f,.56f),new Vector2(.96f,.92f));art.Town(town);
            SelectCharacter(0);
            for(int i=0;i<CharacterIds.Length;i++)
            {
                int index=i;float x=.44f+i*.13f;
                var button=Ui.Button(safe,"PreviewCharacter_"+i,"",new Vector2(x,.45f),new Vector2(x+.125f,.55f),()=>SelectCharacter(index),Ui.Pale);
                CharacterView.Create(button.transform,art,CharacterIds[i],Vector2.zero,Vector2.one,true,true);
            }
            Ui.Text(safe,T("art.choose"),new Vector2(.44f,.41f),new Vector2(.97f,.45f),18);
            string[] actions={"panico","exagerar","mentir","confundir","teoria","meme"};
            for(int i=0;i<actions.Length;i++)
            {float x=.44f+i*.087f;ActionIconGraphic.Create(safe,actions[i],new Vector2(x,.37f),new Vector2(x+.07f,.41f));}
            burst=ReactionBurstGraphic.Create(safe,new Vector2(.03f,.27f),new Vector2(.42f,.55f));
            Signal=Ui.Rect(safe,"Transmission",new Vector2(.44f,.33f),new Vector2(.95f,.37f)).gameObject.AddComponent<RumorSignal>();
            status=Ui.Text(safe,T("art.reaction.0"),new Vector2(.44f,.29f),new Vector2(.95f,.34f),22,Ui.Teal);
            Ui.Button(safe,"PreviewFast",T("art.fast"),new Vector2(.44f,.24f),new Vector2(.96f,.29f),()=>{
                fast=!fast;Character.Motion.Fast=fast;status.text=T(fast?"art.reduced":"art.normal");
            },size:19);
            var labels=new string[9];for(int i=0;i<labels.Length;i++)labels[i]=T("art.reaction."+i);
            for(int i=0;i<labels.Length;i++)
            {
                int index=i;float x=.03f+(i%3)*.315f,y=.16f-(i/3)*.065f;
                Ui.Button(safe,"Preview_"+i,labels[i],new Vector2(x,y),new Vector2(x+.31f,y+.06f),()=>{
                    status.text=labels[index];
                    sound.Play(index==5?PartySound.Celebrate:index==6?PartySound.Defeat:index==3?PartySound.Panic:index==2?PartySound.Surprise:index>=7||index==1?PartySound.Transmit:PartySound.Select);
                    if(index<7)Character.Motion.Play((CharacterReaction)index);
                    else {Character.Motion.Play(CharacterReaction.Talk);Signal.Play(index==8,fast);}
                    if(index==5||index==6)burst.Play(index==5,fast);
                },size:22);
            }
            Ui.Text(safe,T("art.footer"),new Vector2(.04f,0),new Vector2(.96f,.03f),17);
            if(FindObjectOfType<EventSystem>()==null)
            {var events=new GameObject("PreviewEventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform,false);}
        }
        void SelectCharacter(int index)
        {
            if(Character!=null){Character.gameObject.SetActive(false);Destroy(Character.gameObject);}
            Character=CharacterView.Create(safe,art,CharacterIds[index],new Vector2(.04f,.27f),new Vector2(.40f,.55f),false,fast);
            if(status!=null)status.text=T("art.character."+index);
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
