using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rumores.Core.Party;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    public sealed class PartyApp : MonoBehaviour
    {
        [SerializeField] PartyCatalogAsset catalogAsset;
        [SerializeField] PartyActionCard actionCardPrefab;
        [SerializeField] RumoresArt art;
        PartyCatalog catalog;
        RectTransform safeRoot,screen;
        Rect lastSafeArea;
        int lastWidth,lastHeight;
        readonly List<PartyPlayer> roster=new List<PartyPlayer>();
        readonly List<InputField> inputs=new List<InputField>();
        readonly List<PartyActionCard> actionCards=new List<PartyActionCard>();
        readonly List<PartyAction> choices=new List<PartyAction>();
        string selectedAction;
        bool fast;
        int screenVersion,choicePage;
        Text errorLabel,actionHint;
        Button confirm;
        const string FastKey="Rumores.Party.FastAnimations";
        static readonly string[] Avatars={"miedoso","exagerado","mentiroso","ignorante"};
        static readonly string[] AvatarNames={"Asombro","Risa","Rulo","Lentes","Asombro II","Risa II","Rulo II","Lentes II"};
        public PartyGame Game {get;private set;}
        public bool FastAnimations {get {return fast;}set {fast=value;}}

        void Start()
        {
            try
            {
                Screen.orientation=ScreenOrientation.Portrait;Application.targetFrameRate=60;
                BuildCanvas();
                if(catalogAsset==null||actionCardPrefab==null)throw new InvalidOperationException("Faltan las referencias PartyCatalog o PartyActionCard en Main.");
                catalog=catalogAsset.Load();fast=PlayerPrefs.GetInt(FastKey,0)==1;
                roster.Add(new PartyPlayer {name="Jugador 1",avatar=0});roster.Add(new PartyPlayer {name="Jugador 2",avatar=1});
                Menu();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                if(safeRoot!=null){Page("No se pudo iniciar");Ui.ScrollText(screen,ex.Message,new Vector2(.05f,.2f),new Vector2(.95f,.8f));}
            }
        }
        void BuildCanvas()
        {
            var cameraObject=new GameObject("PartyCamera",typeof(Camera));cameraObject.transform.SetParent(transform,false);
            var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ui.Cream;camera.cullingMask=0;
            var canvasObject=new GameObject("PartyCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.transform.SetParent(transform,false);
            canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=1;
            Ui.Panel(canvasObject.transform,"Background",Vector2.zero,Vector2.one,Ui.Cream);
            safeRoot=Ui.Rect(canvasObject.transform,"SafeArea",Vector2.zero,Vector2.one);ApplySafeArea();
            if(FindObjectOfType<EventSystem>()==null){var events=new GameObject("PartyEventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform,false);}
        }
        void Update()
        {if(safeRoot!=null&&(Screen.safeArea!=lastSafeArea||Screen.width!=lastWidth||Screen.height!=lastHeight))ApplySafeArea();}
        void ApplySafeArea()
        {
            lastSafeArea=Screen.safeArea;lastWidth=Screen.width;lastHeight=Screen.height;
            if(lastWidth==0||lastHeight==0)return;
            safeRoot.anchorMin=new Vector2(lastSafeArea.xMin/lastWidth,lastSafeArea.yMin/lastHeight);
            safeRoot.anchorMax=new Vector2(lastSafeArea.xMax/lastWidth,lastSafeArea.yMax/lastHeight);
        }
        void Page(string title)
        {
            StopAllCoroutines();screenVersion++;
            // Disable immediately: Destroy is deferred until end of frame.
            if(screen!=null){screen.gameObject.SetActive(false);Destroy(screen.gameObject);}
            screen=Ui.Rect(safeRoot,"Page",new Vector2(.025f,.015f),new Vector2(.975f,.985f));
            Label(title,.03f,.92f,.97f,.995f,34,Ui.Teal);
            inputs.Clear();actionCards.Clear();
        }
        Text Label(string text,float x0,float y0,float x1,float y1,int size=24,Color? color=null)
        {var label=Ui.Text(screen,text,new Vector2(x0,y0),new Vector2(x1,y1),size,color);label.supportRichText=false;return label;}
        Button Button(string name,string title,float x0,float y0,float x1,float y1,Action callback,Color? color=null)
        {
            int version=screenVersion;
            return Ui.Button(screen,name,title,new Vector2(x0,y0),new Vector2(x1,y1),()=>{if(version==screenVersion)callback();},color);
        }
        void Menu()
        {
            Page("RUMORES");
            SpeechBubbleGraphic.Create(screen,"RumoresMarkBack",new Vector2(.76f,.93f),new Vector2(.87f,.99f),new Color32(131,199,222,255));
            SpeechBubbleGraphic.Create(screen,"RumoresMarkFront",new Vector2(.85f,.92f),new Vector2(.97f,.98f),new Color32(243,201,91,255));
            Label("Un chisme, tus amigos y un montón de fruta.",.04f,.83f,.96f,.91f,27);
            var town=Ui.Rect(screen,"Town",new Vector2(.09f,.55f),new Vector2(.91f,.81f));
            if(art!=null)art.Town(town);else Ui.Town(town);
            Button("Play","Jugar",.09f,.42f,.91f,.51f,()=>Setup());
            Button("HowToPlay","Cómo jugar",.09f,.31f,.91f,.40f,HowToPlay);
            Button("Settings","Ajustes",.09f,.20f,.91f,.29f,Settings);
            if(Game!=null)Button("Continue","Volver a la ronda",.09f,.09f,.91f,.18f,ShowPhase,Ui.Orange);
            Label("2–8 amigos · leé, elegí y pasá el celu",.05f,0,.95f,.07f,21);
        }
        void HowToPlay()
        {
            Page("Cómo jugar");
            var text=Ui.ScrollText(screen,"1. Anoten sus apodos. Juegan de 2 a 8 amigos con un solo celu.\n\n2. Pasen el celu a quien aparece en pantalla. Leé el chisme y elegí una de las tres respuestas. No hay una respuesta correcta. Podés tocar «Ver otras 3» para ver más opciones sin gastar el turno.\n\n3. Tocá «Contarlo así», leé tu versión en voz alta y pasá el celu. Cada persona juega dos veces.\n\n4. Al final, miren cómo empezó la historia y en qué terminó. Pueden comentar la pregunta entre todos o jugar otra.\n\nSon chismes inventados con guiños a la cultura argentina.\n\nLa ronda queda abierta mientras la app siga abierta. Si la cerrás, empiezan de nuevo.",new Vector2(.03f,.13f),new Vector2(.97f,.90f),28);text.supportRichText=false;
            Button("Back","Volver",.09f,.02f,.91f,.11f,Menu);
        }
        void Settings()
        {
            Page("Ajustes");Label("Presentación de los cambios",.05f,.74f,.95f,.85f,28);
            Button("ToggleAnimations",fast?"Animaciones: rápidas":"Animaciones: normales",.05f,.59f,.95f,.71f,()=>{fast=!fast;PlayerPrefs.SetInt(FastKey,fast?1:0);PlayerPrefs.Save();Settings();});
            Label("Elegí cómo aparecen los textos. Podés pasar al siguiente cuando quieras.",.05f,.35f,.95f,.55f,26);
            Button("Back","Volver",.09f,.02f,.91f,.11f,Menu);
        }
        void ReadNames()
        {for(int i=0;i<inputs.Count&&i<roster.Count;i++)roster[i].name=inputs[i].text;}
        void Setup(string message="")
        {
            Page("¿Quiénes juegan?");Label("Apodos únicos · tocá el avatar para cambiarlo",.03f,.86f,.97f,.92f,22);
            var viewport=Ui.Rect(screen,"RosterScroll",new Vector2(.03f,.28f),new Vector2(.97f,.85f));
            var background=viewport.gameObject.AddComponent<Image>();background.color=Ui.Pale;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            viewport.gameObject.AddComponent<RectMask2D>();scroll.viewport=viewport;
            var content=Ui.Rect(viewport,"Roster",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,roster.Count*116);scroll.content=content;
            for(int i=0;i<roster.Count;i++)
            {
                int index=i,version=screenVersion;float top=1f-(float)i/roster.Count,bottom=1f-(float)(i+1)/roster.Count;
                var row=Ui.Rect(content,"Player_"+i,new Vector2(0,bottom),new Vector2(1,top),4);
                var avatar=Ui.Button(row,"Avatar_"+i,"",new Vector2(0,0),new Vector2(.22f,1),()=>{if(version!=screenVersion)return;ReadNames();roster[index].avatar=(roster[index].avatar+1)%8;Setup();},roster[i].avatar<4?Ui.Teal:Ui.Orange);
                CharacterView.Create(avatar.transform,art,Avatars[roster[i].avatar%4],new Vector2(.05f,.27f),new Vector2(.95f,.99f),true,true);
                Ui.Text(avatar.transform,AvatarNames[roster[i].avatar],new Vector2(0,0),new Vector2(1,.27f),15,Ui.Cream,TextAnchor.MiddleCenter);
                var input=Ui.Input(row,roster[i].name,new Vector2(.24f,.12f),new Vector2(.84f,.88f));input.gameObject.name="PlayerName_"+i;input.contentType=InputField.ContentType.Standard;input.characterLimit=16;input.textComponent.supportRichText=false;input.text=roster[i].name;inputs.Add(input);
                var remove=Ui.Button(row,"Remove_"+i,"−",new Vector2(.86f,.12f),new Vector2(1,.88f),()=>{if(version!=screenVersion)return;ReadNames();roster.RemoveAt(index);Setup();},Ui.Orange);remove.interactable=roster.Count>2;
            }
            Button("AddPlayer","+ Agregar participante ("+roster.Count+"/8)",.03f,.20f,.97f,.28f,()=>{ReadNames();int n=roster.Count+1;while(roster.Any(p=>string.Equals(PartyGame.CleanName(p.name),"Jugador "+n,StringComparison.OrdinalIgnoreCase)))n++;roster.Add(new PartyPlayer {name="Jugador "+n,avatar=roster.Count});Setup();}).interactable=roster.Count<8;
            errorLabel=Label(message,.03f,.11f,.97f,.20f,21,Ui.Orange);
            Button("Back","Volver",.03f,.01f,.35f,.10f,()=>{ReadNames();Menu();});
            Button("StartRound","Comenzar ronda",.37f,.01f,.97f,.10f,()=>{
                ReadNames();var errors=PartyGame.ValidatePlayers(roster);if(errors.Count>0){errorLabel.text=errors[0];return;}
                Game=new PartyGame(catalog,roster,UnityEngine.Random.Range(0,int.MaxValue));ShowPhase();
            });
        }
        void ShowPhase()
        {
            switch(Game.Phase)
            {
                case PartyPhase.Handoff:Handoff();break;
                case PartyPhase.Choosing:TurnPage();break;
                case PartyPhase.Revealing:Reveal();break;
                default:Results();break;
            }
        }
        void Handoff()
        {
            selectedAction=null;choicePage=0;
            Page("Pasá el teléfono");
            Ui.Panel(screen,"AvatarBadge",new Vector2(.31f,.60f),new Vector2(.69f,.83f),Game.Current.avatar<4?Ui.Pale:new Color32(243,205,171,255));
            CharacterView.Create(screen,art,Avatars[Game.Current.avatar%4],new Vector2(.31f,.60f),new Vector2(.69f,.83f),false,fast);
            Label(Game.Current.name,.04f,.49f,.96f,.60f,42);
            Label(Game.Turn<Game.Players.Count?"Primera vuelta":"Última vuelta",.04f,.42f,.96f,.49f,25);
            Label("Leé el chisme, elegí cómo contarlo y compartilo en voz alta.",.04f,.24f,.96f,.40f,28);
            string token=Game.Token;
            Button("Ready","Estoy listo",.06f,.08f,.94f,.19f,()=>{Game.Ready(token);TurnPage();});
            Button("Home","Menú",.34f,0,.66f,.07f,Menu);
        }
        void TurnPage()
        {
            Page("Te toca, "+Game.Current.name);
            Label(Game.Branches[0].rumor.headline,.03f,.75f,.97f,.91f,28);
            Label(Game.News.question,.03f,.65f,.97f,.75f,27,Ui.Teal);
            choices.Clear();choices.AddRange(Game.Choices(choicePage));
            for(int i=0;i<choices.Count;i++)
            {
                var action=choices[i];var card=Instantiate(actionCardPrefab,screen);var rt=(RectTransform)card.transform;
                rt.anchorMin=new Vector2(.02f,.18f+(2-i)*.155f);rt.anchorMax=new Vector2(.98f,.18f+(3-i)*.155f);rt.offsetMin=new Vector2(4,4);rt.offsetMax=new Vector2(-4,-4);
                int version=screenVersion;
                card.Bind(action,Game.Preview(action.id).text,()=>{if(version!=screenVersion)return;selectedAction=action.id;RefreshChoice();});actionCards.Add(card);
            }
            actionHint=Label("Elegí la que más te divierta.",.03f,.11f,.54f,.18f,22);
            Button("MoreChoices","Ver otras 3",.55f,.11f,.98f,.18f,()=>{choicePage=1-choicePage;selectedAction=null;TurnPage();});
            Button("History","Lo anterior",.02f,.02f,.32f,.10f,()=>History(false));
            string token=Game.Token;
            confirm=Button("Confirm","Contarlo así",.34f,.02f,.98f,.10f,()=>{
                if(Game.Phase!=PartyPhase.Choosing||Game.Token!=token||selectedAction==null)return;
                confirm.interactable=false;Game.Confirm(token,selectedAction);Reveal();
            },Ui.Orange);
            RefreshChoice();
        }
        void RefreshChoice()
        {
            for(int i=0;i<actionCards.Count;i++)actionCards[i].SetSelected(choices[i].id==selectedAction);
            if(confirm!=null)confirm.interactable=selectedAction!=null;
            if(actionHint!=null&&selectedAction!=null)actionHint.text="¿Sale esta? Confirmala abajo.";
        }
        string StoryHistory()
        {
            var text=new StringBuilder("ASÍ EMPEZÓ\n");text.AppendLine(Game.News.original).AppendLine();
            foreach(var change in Game.Branches[0].rumor.history)
            {
                string player=Game.Players.First(p=>p.id==change.playerId).name;
                text.AppendLine(player+" contó:").AppendLine(change.text).AppendLine();
            }
            return text.ToString();
        }
        void History(bool fromResults)
        {
            Page("El chisme del grupo");var text=Ui.ScrollText(screen,StoryHistory(),new Vector2(.02f,.13f),new Vector2(.98f,.91f),26);text.supportRichText=false;
            Button("BackToTurn","Volver",.06f,.02f,.94f,.11f,()=>{if(fromResults)Results();else TurnPage();});
        }
        void Reveal()
        {
            Page("Leelo en voz alta");
            Label(Game.Current.name+" cuenta…",.03f,.80f,.97f,.91f,30,Ui.Teal);
            var paper=Ui.Panel(screen,"NewVersion",new Vector2(.03f,.39f),new Vector2(.97f,.78f),Ui.Pale);
            var headline=Ui.Text(paper.transform,Game.LastChange.text,new Vector2(.04f,.04f),new Vector2(.96f,.96f),32);headline.supportRichText=false;
            var character=CharacterView.Create(screen,art,Avatars[Game.Current.avatar%4],new Vector2(.03f,.21f),new Vector2(.22f,.37f),false,fast);
            var reaction=Game.LastChange.actionId=="panico"?CharacterReaction.Panic:
                Game.LastChange.actionId=="meme"?CharacterReaction.Laugh:CharacterReaction.Surprise;
            character.Motion.SpeakThen(reaction);
            var signal=Ui.Rect(screen,"RumorTransmission",new Vector2(.22f,.35f),new Vector2(.97f,.39f)).gameObject.AddComponent<RumorSignal>();signal.Play(false,fast);
            Label(Game.Turn+1==Game.TotalTurns?"¡Listo! Veamos en qué terminó el chisme.":"Cuando termines de leer, pasá el celu.",.25f,.21f,.97f,.35f,27);
            string token=Game.Token;
            Button("Next",Game.Turn+1==Game.TotalTurns?"Ver cómo terminó":"Pasar el celu",.04f,.07f,.96f,.18f,()=>{Game.Next(token);ShowPhase();});
            StartCoroutine(Pop(paper.rectTransform));
        }
        IEnumerator Pop(RectTransform target)
        {
            float duration=fast?.08f:.35f;
            for(float elapsed=0;elapsed<duration;elapsed+=Time.unscaledDeltaTime){target.localScale=Vector3.one*Mathf.Lerp(.92f,1,elapsed/duration);yield return null;}
            target.localScale=Vector3.one;
        }
        void Results()
        {
            Page("¡Mirá en qué terminó!");
            CharacterView.Create(screen,art,Avatars[Game.Players[0].avatar%4],new Vector2(.87f,.925f),new Vector2(.98f,.995f),false,fast).Motion.Play(CharacterReaction.Celebrate);
            Label("ASÍ EMPEZÓ",.03f,.85f,.97f,.91f,23,Ui.Teal);
            Label(Game.News.original,.03f,.68f,.97f,.85f,28);
            Label("ASÍ LO DEJARON",.03f,.61f,.97f,.68f,23,Ui.Orange);
            Label(Game.Branches[0].rumor.headline,.03f,.38f,.97f,.61f,30);
            Label("Para charlar entre todos:\n"+Game.News.icebreaker,.03f,.23f,.97f,.38f,26,Ui.Teal);
            Button("History","Ver el chisme completo",.03f,.16f,.97f,.23f,()=>History(true));
            Button("Rematch","¡Otra!",.03f,.075f,.97f,.155f,()=>{Game.Rematch();ShowPhase();},Ui.Orange);
            Button("ChangePlayers","Cambiar amigos",.03f,0,.62f,.075f,()=>Setup());
            Button("Home","Menú",.64f,0,.97f,.075f,Menu);
        }
    }
}
