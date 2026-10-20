using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rumores.Core.Party;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Rumores.Presentation.UiStrings;

namespace Rumores.Presentation
{
    public sealed class PartyApp : MonoBehaviour
    {
        [SerializeField] PartyCatalogAsset catalogAsset;
        [SerializeField] PartyActionCard actionCardPrefab;
        [SerializeField] RumoresArt art;
        PartyCatalog catalog;
        RectTransform safeRoot,screen;
        PageTransition transition;
        PartyAudio sound;
        ScrollRect rosterScroll;
        float rosterPosition=1;
        Rect lastSafeArea;
        int lastWidth,lastHeight;
        readonly List<PartyPlayer> roster=new List<PartyPlayer>();
        readonly List<InputField> inputs=new List<InputField>();
        readonly List<PartyActionCard> actionCards=new List<PartyActionCard>();
        readonly List<PartyAction> choices=new List<PartyAction>();
        string selectedAction;
        bool fast,privateGameView;
        int screenVersion,choicePage;
        Text errorLabel;
        InputField lastFocused;
        Button confirm;
        const string FastKey="Rumores.Party.FastAnimations";
        static readonly string[] Avatars={"miedoso","exagerado","mentiroso","ignorante"};
        public PartyGame Game {get;private set;}
        public bool FastAnimations {get {return fast;}set {fast=value;}}
        public bool IsTransitioning {get {return transition!=null&&!transition.Ready;}}

        void Start()
        {
            try
            {
                Screen.orientation=ScreenOrientation.Portrait;Application.targetFrameRate=60;
                BuildCanvas();
                if(catalogAsset==null||actionCardPrefab==null)throw new InvalidOperationException("Missing Main references.");
                catalog=catalogAsset.Load();fast=PlayerPrefs.GetInt(FastKey,0)==1;
                sound=gameObject.AddComponent<PartyAudio>();
                roster.Add(new PartyPlayer {name=T("players.default",1),avatar=0});
                roster.Add(new PartyPlayer {name=T("players.default",2),avatar=1});
                Menu();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                if(safeRoot!=null){Page(T("app.error"));Ui.ScrollText(screen,ex.Message,new Vector2(.05f,.2f),new Vector2(.95f,.8f));}
            }
        }
        void BuildCanvas()
        {
            var cameraObject=new GameObject("PartyCamera",typeof(Camera));cameraObject.transform.SetParent(transform,false);
            var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ui.Cream;camera.cullingMask=0;
            var canvasObject=new GameObject("PartyCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.transform.SetParent(transform,false);
            canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(720,1280);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=0;
            Ui.Panel(canvasObject.transform,"Background",Vector2.zero,Vector2.one,Ui.Cream);
            safeRoot=Ui.Rect(canvasObject.transform,"SafeArea",Vector2.zero,Vector2.one);ApplySafeArea();
            if(FindObjectOfType<EventSystem>()==null){var events=new GameObject("PartyEventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform,false);}
        }
        void Update()
        {
            if(safeRoot!=null&&(Screen.safeArea!=lastSafeArea||Screen.width!=lastWidth||Screen.height!=lastHeight))ApplySafeArea();
            KeepNameAboveKeyboard();
        }
        void KeepNameAboveKeyboard()
        {
            if(rosterScroll==null||screen==null)return;
            float bottom=.34f;var keyboard=TouchScreenKeyboard.area;
            if(TouchScreenKeyboard.visible&&keyboard.height>0)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(screen,new Vector2(0,keyboard.yMax),null,out var point);
                bottom=Mathf.Clamp((point.y-screen.rect.yMin)/screen.rect.height+.02f,.34f,.65f);
            }
            var viewport=rosterScroll.viewport;bool changed=Mathf.Abs(viewport.anchorMin.y-bottom)>.001f;
            viewport.anchorMin=new Vector2(.03f,bottom);
            var focused=inputs.FirstOrDefault(input=>input!=null&&input.isFocused);
            if(bottom>.34f&&focused!=null&&(changed||focused!=lastFocused))
            {
                Canvas.ForceUpdateCanvases();float overflow=rosterScroll.content.rect.height-viewport.rect.height;
                rosterScroll.verticalNormalizedPosition=overflow<=0?1:1-Mathf.Clamp01(inputs.IndexOf(focused)*170/overflow);
            }
            lastFocused=focused;
        }
        void ApplySafeArea()
        {
            lastSafeArea=Screen.safeArea;lastWidth=Screen.width;lastHeight=Screen.height;
            if(lastWidth==0||lastHeight==0)return;
            safeRoot.anchorMin=new Vector2(lastSafeArea.xMin/lastWidth,lastSafeArea.yMin/lastHeight);
            safeRoot.anchorMax=new Vector2(lastSafeArea.xMax/lastWidth,lastSafeArea.yMax/lastHeight);
        }
        void Page(string title)
        {
            StopAllCoroutines();screenVersion++;privateGameView=false;
            if(sound!=null)sound.StopEffects();
            // Never cross-fade the old player's content into the privacy screen.
            if(screen!=null){screen.gameObject.SetActive(false);Destroy(screen.gameObject);}
            screen=Ui.Rect(safeRoot,"Page",new Vector2(.025f,.015f),new Vector2(.975f,.985f));
            transition=screen.gameObject.AddComponent<PageTransition>();transition.Begin(fast);
            var heading=Label(title,.03f,.93f,.97f,.995f,36,Ui.Teal);
            heading.resizeTextForBestFit=true;heading.resizeTextMinSize=30;heading.resizeTextMaxSize=36;
            inputs.Clear();actionCards.Clear();rosterScroll=null;
        }
        bool Accepts(int version){return version==screenVersion&&!IsTransitioning;}
        Text Label(string text,float x0,float y0,float x1,float y1,int size=32,Color? color=null)
        {var label=Ui.Text(screen,text,new Vector2(x0,y0),new Vector2(x1,y1),size,color);label.supportRichText=false;return label;}
        Button Button(string name,string title,float x0,float y0,float x1,float y1,Action callback,Color? color=null)
        {
            int version=screenVersion;
            return Ui.Button(screen,name,title,new Vector2(x0,y0),new Vector2(x1,y1),()=>{
                if(!Accepts(version))return;callback();if(sound!=null)sound.Play(PartySound.Select);
            },color,size:30);
        }
        void Progress()
        {Label(T("progress.turn",Game.Turn+1,Game.TotalTurns,Game.Turn/Game.Players.Count+1),.03f,.87f,.97f,.925f,28,Ui.Teal);}
        void Menu()
        {
            Page(T("app.title"));
            SpeechBubbleGraphic.Create(screen,"RumoresMarkBack",new Vector2(.76f,.93f),new Vector2(.87f,.99f),new Color32(131,199,222,255));
            SpeechBubbleGraphic.Create(screen,"RumoresMarkFront",new Vector2(.85f,.92f),new Vector2(.97f,.98f),new Color32(243,201,91,255));
            Label(T("menu.tagline"),.04f,.82f,.96f,.92f,32);
            var town=Ui.Rect(screen,"Town",new Vector2(.06f,.54f),new Vector2(.94f,.81f));
            if(art!=null)art.Town(town);else Ui.Town(town);
            town.gameObject.AddComponent<TownAmbience>().Reduced=fast;
            Button("Play",T("menu.play"),.06f,.43f,.94f,.53f,()=>Setup(),PartyLayout.Accent);
            Button("HowToPlay",T("menu.how"),.06f,.32f,.94f,.42f,HowToPlay);
            Button("Settings",T("menu.settings"),.06f,.21f,.94f,.31f,Settings);
            if(Game!=null)Button("Continue",T("menu.continue"),.06f,.10f,.94f,.20f,()=>{if(Game.Phase==PartyPhase.Finished)Results();else Handoff(true);});
            Label(T("menu.footer"),.05f,.01f,.95f,.09f,28);
        }
        void HowToPlay()
        {
            Page(T("menu.how"));var flow=PartyLayout.Flow(screen,"HelpScroll",new Vector2(.03f,.14f),new Vector2(.97f,.91f));
            PartyLayout.Paragraph(flow,T("how.body"));
            Button("Back",T("back"),.06f,.02f,.94f,.12f,Menu);
        }
        void Settings()
        {
            Page(T("menu.settings"));int version=screenVersion;
            var musicLabel=Label(T("settings.music",Mathf.RoundToInt(sound.MusicVolume*100)),.05f,.79f,.95f,.87f);
            PartyLayout.VolumeSlider(screen,"MusicVolume",new Vector2(.05f,.68f),new Vector2(.95f,.78f),sound.MusicVolume,value=>{
                if(!Accepts(version))return;sound.SetMusic(value);musicLabel.text=T("settings.music",Mathf.RoundToInt(value*100));
            });
            var effectsLabel=Label(T("settings.effects",Mathf.RoundToInt(sound.EffectsVolume*100)),.05f,.54f,.95f,.62f);
            PartyLayout.VolumeSlider(screen,"EffectsVolume",new Vector2(.05f,.43f),new Vector2(.95f,.53f),sound.EffectsVolume,value=>{
                if(!Accepts(version))return;sound.SetEffects(value);effectsLabel.text=T("settings.effects",Mathf.RoundToInt(value*100));sound.Play(PartySound.Select);
            });
            Button("ToggleAnimations",T(fast?"settings.reduced":"settings.normal"),.05f,.30f,.95f,.41f,()=>{
                fast=!fast;PlayerPrefs.SetInt(FastKey,fast?1:0);sound.SavePreferences();Settings();
            });
            Label(T("settings.help"),.05f,.14f,.95f,.29f,28);
            Button("Back",T("back"),.06f,.02f,.94f,.12f,()=>{sound.SavePreferences();Menu();});
        }
        void ReadNames()
        {for(int i=0;i<inputs.Count&&i<roster.Count;i++)roster[i].name=inputs[i].text;}
        void SaveRosterPosition(){if(rosterScroll!=null)rosterPosition=rosterScroll.verticalNormalizedPosition;}
        string NameError()
        {
            var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for(int i=0;i<roster.Count;i++)
            {var name=PartyGame.CleanName(roster[i].name);if(name.Length==0)return T("players.empty",i+1);if(!names.Add(name))return T("players.duplicate",name);}
            return T("players.error");
        }
        void Setup(string message="")
        {
            Page(T("players.title"));Label(T("players.summary",roster.Count,roster.Count*catalog.turnsPerPlayer),.03f,.87f,.97f,.93f,28,Ui.Teal);
            Label(T("players.hint"),.03f,.81f,.97f,.87f,28);
            var viewport=Ui.Rect(screen,"RosterScroll",new Vector2(.03f,.34f),new Vector2(.97f,.81f));
            viewport.gameObject.AddComponent<Image>().color=Ui.Pale;
            rosterScroll=viewport.gameObject.AddComponent<ScrollRect>();rosterScroll.horizontal=false;rosterScroll.movementType=ScrollRect.MovementType.Clamped;
            viewport.gameObject.AddComponent<RectMask2D>();rosterScroll.viewport=viewport;
            var content=Ui.Rect(viewport,"Roster",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,roster.Count*170);rosterScroll.content=content;
            for(int i=0;i<roster.Count;i++)
            {
                int index=i,version=screenVersion;float top=1f-(float)i/roster.Count,bottom=1f-(float)(i+1)/roster.Count;
                var row=Ui.Rect(content,"Player_"+i,new Vector2(0,bottom),new Vector2(1,top),4);
                var avatar=Ui.Button(row,"Avatar_"+i,"",new Vector2(0,0),new Vector2(.23f,1),()=>{
                    if(!Accepts(version))return;ReadNames();SaveRosterPosition();roster[index].avatar=(roster[index].avatar+1)%8;Setup();sound.Play(PartySound.Select);
                },roster[i].avatar<4?Ui.Teal:PartyLayout.Accent);
                CharacterView.Create(avatar.transform,art,Avatars[roster[i].avatar%4],new Vector2(.05f,.30f),new Vector2(.95f,.99f),true,true);
                Ui.Text(avatar.transform,T("avatar."+roster[i].avatar),Vector2.zero,new Vector2(1,.30f),24,Ui.Cream,TextAnchor.MiddleCenter);
                var input=Ui.Input(row,roster[i].name,new Vector2(.24f,.12f),new Vector2(.80f,.88f));
                input.gameObject.name="PlayerName_"+i;input.contentType=InputField.ContentType.Standard;input.characterLimit=16;input.lineType=InputField.LineType.SingleLine;
                input.textComponent.supportRichText=false;input.text=roster[i].name;inputs.Add(input);
                var remove=Ui.Button(row,"Remove_"+i,T("players.remove"),new Vector2(.81f,.12f),new Vector2(1,.88f),()=>{
                    if(!Accepts(version))return;ReadNames();SaveRosterPosition();roster.RemoveAt(index);Setup();sound.Play(PartySound.Select);
                },PartyLayout.Accent,size:26);remove.interactable=roster.Count>2;
            }
            Canvas.ForceUpdateCanvases();rosterScroll.verticalNormalizedPosition=rosterPosition;
            Button("AddPlayer",T("players.add",roster.Count),.03f,.23f,.97f,.33f,()=>{
                ReadNames();int n=roster.Count+1;while(roster.Any(p=>string.Equals(PartyGame.CleanName(p.name),T("players.default",n),StringComparison.OrdinalIgnoreCase)))n++;
                roster.Add(new PartyPlayer {name=T("players.default",n),avatar=roster.Count});rosterPosition=0;Setup();
            }).interactable=roster.Count<8;
            errorLabel=Label(message,.03f,.12f,.97f,.23f,28,PartyLayout.Accent);
            Button("Back",T("back"),.03f,.02f,.35f,.12f,()=>{ReadNames();SaveRosterPosition();Menu();});
            Button("StartRound",T("players.start"),.37f,.02f,.97f,.12f,()=>{
                ReadNames();var errors=PartyGame.ValidatePlayers(roster);if(errors.Count>0){errorLabel.text=NameError();return;}
                Game=new PartyGame(catalog,roster,UnityEngine.Random.Range(0,int.MaxValue));ShowPhase();
            },PartyLayout.Accent);
        }
        void ShowPhase()
        {
            switch(Game.Phase)
            {case PartyPhase.Handoff:Handoff();break;case PartyPhase.Choosing:TurnPage();break;case PartyPhase.Revealing:Reveal();break;default:Results(true);break;}
        }
        void Handoff(bool resume=false)
        {
            if(!resume){selectedAction=null;choicePage=0;}
            Page(T("handoff.title"));privateGameView=true;Progress();
            Ui.Panel(screen,"AvatarBadge",new Vector2(.31f,.60f),new Vector2(.69f,.84f),Game.Current.avatar<4?Ui.Pale:new Color32(243,205,171,255));
            CharacterView.Create(screen,art,Avatars[Game.Current.avatar%4],new Vector2(.31f,.60f),new Vector2(.69f,.84f),false,fast);
            Label(Game.Current.name,.04f,.49f,.96f,.60f,42);
            Label(T(Game.Turn<Game.Players.Count?"handoff.first":"handoff.last"),.04f,.42f,.96f,.49f,30);
            Label(T("handoff.help",Game.Current.name),.04f,.24f,.96f,.41f,32);
            string token=Game.Token;
            Button("Ready",T("handoff.ready"),.06f,.13f,.94f,.24f,()=>{
                if(Game.Phase==PartyPhase.Handoff)Game.Ready(token);
                if(Game.Phase==PartyPhase.Revealing)Reveal();else TurnPage();
            });
            Button("Home",T("home"),.25f,.02f,.75f,.12f,Menu);
        }
        void TurnPage()
        {
            Page(T("turn.title",Game.Current.name));privateGameView=true;Progress();
            var flow=PartyLayout.Flow(screen,"ChoicesScroll",new Vector2(.03f,.23f),new Vector2(.97f,.86f));
            var story=PartyLayout.Card(flow,"CurrentStory",Ui.Pale);
            PartyLayout.Paragraph(story,T("turn.current"),28,Ui.Teal);PartyLayout.Paragraph(story,Game.Branches[0].rumor.headline);
            PartyLayout.Paragraph(flow,Game.News.question,32,Ui.Teal);
            PartyLayout.Paragraph(flow,T("turn.instructions"),28);
            choices.Clear();choices.AddRange(Game.Choices(choicePage));
            for(int i=0;i<choices.Count;i++)
            {
                var action=choices[i];var card=Instantiate(actionCardPrefab,flow);int version=screenVersion;
                card.Bind(action,Game.Preview(action.id).text,()=>{
                    if(!Accepts(version))return;selectedAction=action.id;RefreshChoice();sound.Play(PartySound.Select);
                });actionCards.Add(card);
            }
            Button("MoreChoices",T("turn.more"),.51f,.12f,.97f,.22f,()=>{choicePage=1-choicePage;selectedAction=null;TurnPage();});
            Button("History",T("turn.history"),.03f,.12f,.49f,.22f,()=>History(false));
            string token=Game.Token;
            confirm=Button("Confirm",T("turn.confirm"),.03f,.02f,.97f,.12f,()=>{
                if(Game.Phase!=PartyPhase.Choosing||Game.Token!=token||selectedAction==null)return;
                confirm.interactable=false;Game.Confirm(token,selectedAction);Reveal();
            },PartyLayout.Accent);
            RefreshChoice();
        }
        void RefreshChoice()
        {
            for(int i=0;i<actionCards.Count;i++)actionCards[i].SetSelected(choices[i].id==selectedAction);
            if(confirm!=null)confirm.interactable=selectedAction!=null;
            if(confirm!=null)confirm.GetComponentInChildren<Text>().text=selectedAction==null?T("turn.confirm"):T("turn.confirmSelected",catalog.Action(selectedAction).title);
        }
        void AddHistory(Transform flow)
        {
            foreach(var branch in Game.Branches)
            {
                if(Game.Branches.Count>1)PartyLayout.Paragraph(flow,T("history.branch",branch.id),32,Ui.Teal);
                foreach(var change in branch.rumor.history)
                {
                    var item=PartyLayout.Card(flow,"Contribution_"+branch.id+"_"+change.turn,Ui.Pale);
                    var player=Game.Players.First(p=>p.id==change.playerId);
                    PartyLayout.Paragraph(item,T("history.contribution",change.turn+1,player.name),30,Ui.Teal);
                    PartyLayout.Paragraph(item,change.text);
                    PartyLayout.Paragraph(item,T("history.reason",change.reason),28);
                }
            }
        }
        void History(bool fromResults)
        {
            Page(T("history.title"));privateGameView=!fromResults;var flow=PartyLayout.Flow(screen,"HistoryScroll",new Vector2(.03f,.14f),new Vector2(.97f,.91f));
            PartyLayout.Paragraph(flow,T("history.original"),28,Ui.Teal);PartyLayout.Paragraph(flow,Game.News.original);AddHistory(flow);
            Button("BackToTurn",T("back"),.06f,.02f,.94f,.12f,()=>{if(fromResults)Results();else TurnPage();});
        }
        void Reveal()
        {
            Page(T("reveal.title"));privateGameView=true;Progress();
            var flow=PartyLayout.Flow(screen,"RevealScroll",new Vector2(.03f,.37f),new Vector2(.97f,.86f));
            PartyLayout.Paragraph(flow,T("reveal.speaker",Game.Current.name),32,Ui.Teal);
            var paper=PartyLayout.Card(flow,"NewVersion",Ui.Pale);PartyLayout.Paragraph(paper,Game.LastChange.text,36);
            PartyLayout.Paragraph(flow,T("history.reason",Game.LastChange.reason),28);
            var character=CharacterView.Create(screen,art,Avatars[Game.Current.avatar%4],new Vector2(.03f,.16f),new Vector2(.22f,.35f),false,fast);
            var reaction=Game.LastChange.actionId=="panico"?CharacterReaction.Panic:Game.LastChange.actionId=="meme"?CharacterReaction.Laugh:CharacterReaction.Surprise;
            character.Motion.SpeakThen(reaction);
            Ui.Rect(screen,"RumorTransmission",new Vector2(.24f,.32f),new Vector2(.97f,.37f)).gameObject.AddComponent<RumorSignal>().Play(false,fast);
            Label(T(Game.Turn+1==Game.TotalTurns?"reveal.last":"reveal.next"),.25f,.16f,.97f,.32f,30);
            string token=Game.Token;
            Button("Next",T(Game.Turn+1==Game.TotalTurns?"reveal.finish":"reveal.pass"),.04f,.02f,.96f,.13f,()=>{Game.Next(token);ShowPhase();});
            sound.Play(PartySound.Transmit);StartCoroutine(ReactionSound(reaction));
        }
        IEnumerator ReactionSound(CharacterReaction reaction)
        {yield return new WaitForSecondsRealtime(.38f);sound.Play(reaction==CharacterReaction.Panic?PartySound.Panic:PartySound.Surprise);}
        void Results(bool celebrate=false)
        {
            Page(T("results.title"));
            Label(T("results.group",Game.Players.Count,Game.TotalTurns),.03f,.86f,.97f,.93f,28,Ui.Teal);
            var flow=PartyLayout.Flow(screen,"ResultsScroll",new Vector2(.03f,.23f),new Vector2(.97f,.86f));
            PartyLayout.Paragraph(flow,T("results.original"),28,Ui.Teal);PartyLayout.Paragraph(flow,Game.News.original);
            foreach(var branch in Game.Branches)
            {
                var final=PartyLayout.Card(flow,"Final_"+branch.id,Ui.Pale);
                PartyLayout.Paragraph(final,T("results.final")+(Game.Branches.Count>1?" · "+branch.id:""),30,PartyLayout.Accent);
                PartyLayout.Paragraph(final,branch.rumor.headline,36);
            }
            PartyLayout.Paragraph(flow,T("results.chat"),30,Ui.Teal);PartyLayout.Paragraph(flow,Game.News.icebreaker);
            AddHistory(flow);
            Button("History",T("results.history"),.03f,.12f,.49f,.22f,()=>History(true));
            Button("ChangePlayers",T("results.players"),.51f,.12f,.97f,.22f,()=>Setup());
            Button("Rematch",T("results.rematch"),.03f,.02f,.62f,.12f,()=>{Game.Rematch();ShowPhase();},PartyLayout.Accent);
            Button("Home",T("home"),.64f,.02f,.97f,.12f,Menu);
            if(celebrate)
            {
                ReactionBurstGraphic.Create(screen,Vector2.zero,Vector2.one).Play(true,fast);
                sound.Play(PartySound.Reveal);StartCoroutine(FinaleSound());
            }
        }
        IEnumerator FinaleSound(){yield return new WaitForSecondsRealtime(.66f);sound.Play(PartySound.Celebrate);}
        void OnApplicationPause(bool paused)
        {if(paused&&privateGameView&&Game!=null&&safeRoot!=null&&Game.Phase!=PartyPhase.Finished)Handoff(true);}
    }
}
