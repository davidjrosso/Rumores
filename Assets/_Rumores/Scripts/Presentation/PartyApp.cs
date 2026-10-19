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
        PartyCatalog catalog;
        RectTransform safeRoot,screen;
        Rect lastSafeArea;
        int lastWidth,lastHeight;
        readonly List<PartyPlayer> roster=new List<PartyPlayer>();
        readonly List<InputField> inputs=new List<InputField>();
        readonly List<PartyActionCard> actionCards=new List<PartyActionCard>();
        string selectedAction,selectedBranch="A";
        bool split,fast;
        int screenVersion;
        Text errorLabel,actionHint;
        Button confirm;
        const string FastKey="Rumores.Party.FastAnimations";
        const string ScoreHelp="Hasta 3 puntos por turno: +1 por un aporte tuyo nuevo en esa rama; +1 por una acción que todavía no usaste; +1 por cumplir el objetivo. Dos turnos por persona. Los empates se comparten.";
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
            Label("Un pueblo. Demasiadas versiones.",.04f,.83f,.96f,.91f,27);
            var town=Ui.Rect(screen,"Town",new Vector2(.09f,.55f),new Vector2(.91f,.81f));Ui.Town(town);
            Button("Play","Jugar",.09f,.42f,.91f,.51f,()=>Setup());
            Button("HowToPlay","Cómo jugar",.09f,.31f,.91f,.40f,HowToPlay);
            Button("Settings","Ajustes",.09f,.20f,.91f,.29f,Settings);
            if(Game!=null)Button("Continue","Volver a la ronda",.09f,.09f,.91f,.18f,ShowPhase,Ui.Orange);
            Label("2–8 personas · un teléfono · sin conexión",.05f,0,.95f,.07f,21);
        }
        void HowToPlay()
        {
            Page("Cómo jugar");
            var text=Ui.ScrollText(screen,"1. Anoten de 2 a 8 apodos y elijan sus avatares. El avatar es decorativo: todos pueden usar todas las personalidades.\n\n2. Pasen el teléfono a quien indique la pantalla. Cada persona juega dos veces, en el orden de la lista.\n\n3. Leé el rumor y el objetivo común. Elegí una acción y confirmá. Las tarjetas explican sus efectos.\n\n4. Una vez por ronda podés enviar una copia al mercado. Ambas ramas empiezan iguales; después elegís cuál modificar. Dividir no da puntos extra ni turnos extra.\n\n5. Leé qué cambió y pasá al siguiente participante. Al final se muestran las versiones, sus historias y los ganadores.\n\n"+ScoreHelp+"\n\nAporte nuevo: una transformación que vos todavía no hiciste sobre ese protagonista en esa rama. Cambiar solo las palabras no suma otro aporte. Los aportes de otras personas no te quitan puntos.\n\nLa ronda se conserva mientras la aplicación esté abierta. Al cerrarla se pierde; los ajustes sí se recuerdan.",new Vector2(.03f,.13f),new Vector2(.97f,.90f),25);text.supportRichText=false;
            Button("Back","Volver",.09f,.02f,.91f,.11f,Menu);
        }
        void Settings()
        {
            Page("Ajustes");Label("Presentación de los cambios",.05f,.74f,.95f,.85f,28);
            Button("ToggleAnimations",fast?"Animaciones: rápidas":"Animaciones: normales",.05f,.59f,.95f,.71f,()=>{fast=!fast;PlayerPrefs.SetInt(FastKey,fast?1:0);PlayerPrefs.Save();Settings();});
            Label("Este ajuste solo cambia la presentación. No altera las reglas, los turnos ni los puntos.",.05f,.35f,.95f,.55f,26);
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
                Ui.Avatar(avatar.transform,Avatars[roster[i].avatar%4],new Vector2(.05f,.27f),new Vector2(.95f,.99f));
                Ui.Text(avatar.transform,AvatarNames[roster[i].avatar],new Vector2(0,0),new Vector2(1,.27f),15,Ui.Cream,TextAnchor.MiddleCenter);
                var input=Ui.Input(row,roster[i].name,new Vector2(.24f,.12f),new Vector2(.84f,.88f));input.gameObject.name="PlayerName_"+i;input.contentType=InputField.ContentType.Standard;input.characterLimit=16;input.textComponent.supportRichText=false;input.text=roster[i].name;inputs.Add(input);
                var remove=Ui.Button(row,"Remove_"+i,"−",new Vector2(.86f,.12f),new Vector2(1,.88f),()=>{if(version!=screenVersion)return;ReadNames();roster.RemoveAt(index);Setup();},Ui.Orange);remove.interactable=roster.Count>2;
            }
            Button("AddPlayer","+ Agregar participante ("+roster.Count+"/8)",.03f,.20f,.97f,.28f,()=>{ReadNames();int n=roster.Count+1;while(roster.Any(p=>string.Equals(PartyGame.CleanName(p.name),"Jugador "+n,StringComparison.OrdinalIgnoreCase)))n++;roster.Add(new PartyPlayer {name="Jugador "+n,avatar=roster.Count});Setup();}).interactable=roster.Count<8;
            errorLabel=Label(message,.03f,.11f,.97f,.20f,21,Ui.Orange);
            Button("Back","Volver",.03f,.01f,.35f,.10f,()=>{ReadNames();Menu();});
            Button("StartRound","Comenzar ronda",.37f,.01f,.97f,.10f,()=>{
                ReadNames();var errors=PartyGame.ValidatePlayers(roster);if(errors.Count>0){errorLabel.text=errors[0];return;}
                Game=new PartyGame(catalog,roster,UnityEngine.Random.Range(0,int.MaxValue));selectedBranch="A";ShowPhase();
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
            selectedAction=null;split=false;selectedBranch="A";
            Page("Pasá el teléfono");
            Ui.Panel(screen,"AvatarBadge",new Vector2(.31f,.60f),new Vector2(.69f,.83f),Game.Current.avatar<4?Ui.Pale:new Color32(243,205,171,255));
            Ui.Avatar(screen,Avatars[Game.Current.avatar%4],new Vector2(.31f,.60f),new Vector2(.69f,.83f));
            Label(Game.Current.name,.04f,.49f,.96f,.60f,42);
            Label("Turno "+(Game.Turn+1)+" de "+Game.TotalTurns+" · "+Game.Current.score+" puntos",.04f,.42f,.96f,.49f,25);
            Label("Objetivo: "+Game.Goal.title+"\n"+Game.Goal.description,.04f,.22f,.96f,.40f,25);
            string token=Game.Token;
            Button("Ready","Estoy listo",.06f,.08f,.94f,.19f,()=>{Game.Ready(token);TurnPage();});
            Button("Home","Menú",.34f,0,.66f,.07f,Menu);
        }
        void TurnPage()
        {
            Page(Game.Current.name+" · turno "+(Game.Turn+1)+"/"+Game.TotalTurns);
            var branch=Game.Branches.First(b=>b.id==selectedBranch);
            if(Game.Branches.Count==1)Label("Rama A · La plaza",.03f,.86f,.67f,.92f,24,Ui.Teal);
            else foreach(var b in Game.Branches){var id=b.id;float x=id=="A"?.02f:.50f;Button("Branch_"+id,(id==selectedBranch?"● ":"")+id+" · "+b.label,x,.86f,x+.48f,.92f,()=>{selectedBranch=id;TurnPage();},id==selectedBranch?Ui.Orange:Ui.Teal);}
            Label(branch.rumor.headline,.03f,.75f,.97f,.86f,26);
            Label(Game.Goal.title+"\n"+Game.Goal.description,.03f,.66f,.97f,.75f,21,Ui.Teal);
            for(int i=0;i<catalog.actions.Count;i++)
            {
                var a=catalog.actions[i];var card=Instantiate(actionCardPrefab,screen);var rt=(RectTransform)card.transform;
                int col=i%2,row=i/2,rows=(catalog.actions.Count+1)/2;float height=.39f/rows;rt.anchorMin=new Vector2(col*.5f+.01f,.26f+(rows-1-row)*height);rt.anchorMax=new Vector2(col*.5f+.49f,.26f+(rows-row)*height);rt.offsetMin=new Vector2(4,4);rt.offsetMax=new Vector2(-4,-4);
                int version=screenVersion;
                card.Bind(a,()=>{if(version!=screenVersion)return;selectedAction=a.id;RefreshChoice();});actionCards.Add(card);
            }
            actionHint=Label("Elegí una acción para ver su efecto.",.03f,.17f,.97f,.26f,21);
            if(Game.CanSplit)Button("Split",split?"✓ Crear también rama B · El mercado":"○ Dividir: copiar esta versión al mercado",.02f,.105f,.98f,.17f,()=>{split=!split;TurnPage();},split?Ui.Orange:Ui.Teal);
            else Label(Game.Branches.Count==2?"Las ramas cambian por separado. Elegí A o B arriba.":"Último turno: ya no se puede dividir.",.03f,.105f,.97f,.17f,20);
            Button("History","Historia",.02f,.02f,.32f,.10f,History);
            string token=Game.Token;
            confirm=Button("Confirm","Confirmar acción",.34f,.02f,.98f,.10f,()=>{
                if(Game.Phase!=PartyPhase.Choosing||Game.Token!=token||selectedAction==null)return;
                confirm.interactable=false;Game.Confirm(token,selectedAction,selectedBranch,split);Reveal();
            },Ui.Orange);
            RefreshChoice();
        }
        void RefreshChoice()
        {
            for(int i=0;i<actionCards.Count;i++)actionCards[i].SetSelected(catalog.actions[i].id==selectedAction);
            if(confirm!=null)confirm.interactable=selectedAction!=null;
            if(actionHint!=null&&selectedAction!=null)actionHint.text=catalog.Action(selectedAction).description;
        }
        string BranchHistory(PartyBranch branch)
        {
            var text=new StringBuilder();text.AppendLine(branch.id+" · "+branch.label).AppendLine(branch.rumor.headline).AppendLine(branch.rumor.values.ToString()).AppendLine();
            foreach(var change in branch.rumor.history)
            {
                string player=Game.Players.First(p=>p.id==change.playerId).name;
                text.AppendLine("Turno "+(change.turn+1)+" · "+player+" · "+catalog.Action(change.actionId).title+(change.split?" · DIVISIÓN A → A + B":""));
                text.AppendLine(change.text).AppendLine(change.reason).AppendLine(change.before+" → "+change.after).AppendLine("+"+change.points+" puntos: "+change.ScoreExplanation).AppendLine();
            }
            return text.ToString();
        }
        void History()
        {
            Page("Historia de la noticia");var text=Ui.ScrollText(screen,"ORIGINAL\n"+Game.News.original+"\n\n"+string.Join("\n────────\n",Game.Branches.Select(BranchHistory))+"\nEl prefijo anterior a la división aparece en ambas ramas. Sus puntos se cuentan una sola vez.",new Vector2(.02f,.13f),new Vector2(.98f,.91f),23);text.supportRichText=false;
            Button("BackToTurn","Volver al turno",.06f,.02f,.94f,.11f,TurnPage);
        }
        void Reveal()
        {
            Page("Así cambió el rumor");var change=Game.LastChange;
            var paper=Ui.Panel(screen,"NewVersion",new Vector2(.03f,.66f),new Vector2(.97f,.90f),Ui.Pale);
            var headline=Ui.Text(paper.transform,change.text,new Vector2(.04f,.04f),new Vector2(.96f,.96f),30);headline.supportRichText=false;
            Label(change.reason,.03f,.48f,.97f,.64f,25);
            Label("ANTES: "+change.before+"\nAHORA: "+change.after,.03f,.34f,.97f,.47f,22);
            Label("+"+change.points+" puntos para "+Game.Current.name+"\n"+change.ScoreExplanation,.03f,.21f,.97f,.33f,24,Ui.Teal);
            Label(change.split?"A · La plaza  →  A + B · El mercado\nDesde ahora cada rama puede cambiar por su cuenta.":"Cambió solo la rama "+change.branchId+". "+PartyTransform.Reaction(change.after),.03f,.10f,.97f,.21f,22);
            string token=Game.Token;
            Button("Next",Game.Turn+1==Game.TotalTurns?"Ver resultados":"Siguiente participante",.04f,.01f,.96f,.10f,()=>{Game.Next(token);ShowPhase();});
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
            Page("Fin de la ronda");var winners=Game.Winners();
            Label(winners.Count==1?"Ganó "+winners[0].name:"Victoria compartida: "+winners.Count+" ganadores",.03f,.79f,.97f,.91f,30,Ui.Orange);
            var report=new StringBuilder();report.AppendLine("GANADORES: "+string.Join(", ",winners.Select(p=>p.name))).AppendLine().AppendLine("PUNTUACIÓN · "+Game.Goal.title);
            foreach(var p in Game.Players.OrderByDescending(p=>p.score))report.AppendLine(p.name+": "+p.score+" puntos");
            report.AppendLine().AppendLine(ScoreHelp).AppendLine().AppendLine("NOTICIA ORIGINAL").AppendLine(Game.News.original).AppendLine();
            foreach(var branch in Game.Branches)report.AppendLine("VERSIÓN FINAL · "+branch.id+" · "+branch.label).AppendLine(branch.rumor.headline).AppendLine(PartyTransform.Reaction(branch.rumor.values)).AppendLine();
            report.AppendLine("HISTORIAL DE CAMBIOS").AppendLine("Los puntos de un evento compartido se cuentan una sola vez.").AppendLine();
            foreach(var branch in Game.Branches)report.AppendLine(BranchHistory(branch));
            var text=Ui.ScrollText(screen,report.ToString(),new Vector2(.02f,.19f),new Vector2(.98f,.79f),24);text.supportRichText=false;
            Button("Rematch","Nueva ronda · mismos jugadores",.03f,.10f,.97f,.18f,()=>{Game.Rematch();selectedBranch="A";ShowPhase();},Ui.Orange);
            Button("ChangePlayers","Cambiar jugadores",.03f,.01f,.62f,.09f,()=>Setup());
            Button("Home","Menú",.64f,.01f,.97f,.09f,Menu);
        }
    }
}
