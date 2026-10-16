using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Rumores.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    public sealed class RumoresApp : MonoBehaviour
    {
        public GameSession Session {get;private set;}
        public Catalog Catalog {get;private set;}
        RectTransform safe,screen;
        Rect lastSafe;
        LocalSave storage;
        AudioSource audioSource;
        AudioClip tick;
        Text context,budget,pauseLabel;
        BoardGraphics graph;
        readonly Dictionary<string,RectTransform> people=new Dictionary<string,RectTransform>();
        readonly Dictionary<string,Button> personButtons=new Dictionary<string,Button>();
        string selected,message;
        bool playing,paused,skip,review;
        int shown;

        void Start()
        {
            Application.targetFrameRate=60;
            var camera=Camera.main;
            if(camera==null){var go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));go.tag="MainCamera";camera=go.GetComponent<Camera>();}
            camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ui.Cream;
            var canvasObject=new GameObject("RumoresCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform,false);var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=.5f;
            safe=Ui.Rect(canvas.transform,"SafeArea",Vector2.zero,Vector2.one);ApplySafeArea();
            if(EventSystem.current==null){var es=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));es.transform.SetParent(transform,false);}
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;
            var wave=new float[4410];for(int i=0;i<wave.Length;i++)wave[i]=Mathf.Sin(i*2*Mathf.PI*660/44100)*.12f*(1f-i/(float)wave.Length);
            tick=AudioClip.Create("Susurro",wave.Length,1,44100,false);tick.SetData(wave,0);
            try
            {
                var asset=Resources.Load<RumoresCatalog>("RumoresCatalog");if(asset==null)throw new InvalidOperationException("Falta RumoresCatalog.asset.");
                Catalog=asset.Load();storage=new LocalSave(Application.persistentDataPath);Session=new GameSession(Catalog,storage.Load(Catalog));message=storage.Notice;ShowHome();
            }
            catch(Exception ex){NewScreen();Ui.ScrollText(screen,"No se pudo iniciar Rumores.\n\n"+ex.Message,new Vector2(.06f,.08f),new Vector2(.94f,.92f));Debug.LogException(ex);}
        }
        void Update()
        {
            if(safe!=null&&lastSafe!=Screen.safeArea)ApplySafeArea();
            if(Input.GetKeyDown(KeyCode.Escape)&&Session!=null){if(playing){paused=!paused;UpdatePauseLabel();}else ShowHome();}
        }
        void ApplySafeArea()
        {
            lastSafe=Screen.safeArea;if(Screen.width<=0||Screen.height<=0)return;
            safe.anchorMin=new Vector2(lastSafe.xMin/Screen.width,lastSafe.yMin/Screen.height);safe.anchorMax=new Vector2(lastSafe.xMax/Screen.width,lastSafe.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        void OnApplicationPause(bool value){if(value){paused=true;Persist();}UpdatePauseLabel();}
        void OnApplicationQuit(){Persist();}
        void OnDestroy(){if(tick!=null)Destroy(tick);}
        void Persist(){if(Session!=null&&storage!=null&&!storage.Write(Session.Save))message=storage.Notice;}
        void Sound(float pitch=1){if(Session==null)return;audioSource.pitch=pitch;audioSource.PlayOneShot(tick,Session.Save.settings.volume);}
        void Act(Action action)
        {
            try{action();Persist();Sound();ShowCurrent();}
            catch(InvalidOperationException ex){message=ex.Message;if(context!=null)context.text=message;}
        }
        void NewScreen()
        {
            if(screen!=null){screen.gameObject.SetActive(false);Destroy(screen.gameObject);}
            screen=Ui.Rect(safe,"Screen",Vector2.zero,Vector2.one);Ui.Panel(screen,"Background",Vector2.zero,Vector2.one,Ui.Cream);
            context=null;budget=null;graph=null;pauseLabel=null;people.Clear();personButtons.Clear();selected=null;
        }
        void Heading(string eyebrow,string title)
        {
            Ui.Text(screen,eyebrow.ToUpperInvariant(),new Vector2(.06f,.91f),new Vector2(.94f,.96f),18,Ui.Teal);
            Ui.Text(screen,title,new Vector2(.06f,.83f),new Vector2(.94f,.92f),36,Ui.Ink);
        }
        public void ShowHome()
        {
            if(playing)return;
            review=false;NewScreen();
            Ui.Text(screen,"UN PUEBLO. MIL VERSIONES.",new Vector2(.08f,.88f),new Vector2(.92f,.94f),19,Ui.Teal,TextAnchor.MiddleCenter);
            Ui.Text(screen,"RUMORES",new Vector2(.04f,.77f),new Vector2(.96f,.9f),71,Ui.Ink,TextAnchor.MiddleCenter);
            Ui.Text(screen,"El chisme empieza con vos.",new Vector2(.1f,.71f),new Vector2(.9f,.78f),25,Ui.Muted,TextAnchor.MiddleCenter);
            var town=Ui.Rect(screen,"VillaSusurro",new Vector2(.1f,.43f),new Vector2(.9f,.69f));Ui.Town(town);
            for(int i=0;i<4;i++)Ui.Avatar(town,Catalog.personalities[i].id,new Vector2(.13f+i*.19f,.25f),new Vector2(.30f+i*.19f,.64f));
            Ui.Button(screen,"NewRun","Nueva run",new Vector2(.13f,.33f),new Vector2(.87f,.405f),ShowNewRun,Ui.Orange);
            if(Session.Run!=null)Ui.Button(screen,"Continue","Continuar · semilla "+Session.Run.seed,new Vector2(.13f,.245f),new Vector2(.87f,.32f),ShowCurrent);
            Ui.Button(screen,"Settings","Ajustes y cómo jugar",new Vector2(.13f,.16f),new Vector2(.87f,.235f),ShowSettings,Ui.Ink);
            Ui.Text(screen,message??(Session.Save.mayorTitle?"Título desbloqueado: Cronista de Villa Susurro":"Estrategia de redes · seis encuentros · sin conexión"),new Vector2(.08f,.035f),new Vector2(.92f,.14f),18,Ui.Muted,TextAnchor.MiddleCenter);
        }
        void ShowNewRun()
        {
            NewScreen();Heading("Villa Susurro te espera","Una nueva historia");
            Ui.Text(screen,"La semilla permite volver al mismo mapa.\nTus decisiones escriben el resto.",new Vector2(.07f,.64f),new Vector2(.93f,.8f),26);
            Ui.Text(screen,"SEMILLA",new Vector2(.08f,.56f),new Vector2(.92f,.62f),18,Ui.Teal);
            var seed=Ui.Input(screen,(Environment.TickCount & int.MaxValue).ToString(CultureInfo.InvariantCulture),new Vector2(.08f,.46f),new Vector2(.92f,.56f));
            context=Ui.Text(screen,Session.Run!=null&&Session.Run.phase!=Phase.Finished?"Empezar reemplaza la run actual. Tus ajustes y títulos se conservan.":"Empezás con 3 Paciencias. Cada fracaso cuesta una.",new Vector2(.08f,.29f),new Vector2(.92f,.43f),23);
            Ui.Button(screen,"StartRun","Que corra el rumor",new Vector2(.08f,.17f),new Vector2(.92f,.26f),()=>{int value;if(!int.TryParse(seed.text,NumberStyles.Integer,CultureInfo.InvariantCulture,out value)){context.text="Usá una semilla entera entre −2147483648 y 2147483647.";return;}StartRun(value);},Ui.Orange);
            Ui.Button(screen,"Back","Volver",new Vector2(.08f,.06f),new Vector2(.92f,.15f),ShowHome,Ui.Ink);
        }
        public void StartRun(int seed){Session.NewRun(seed);message=null;Persist();ShowCurrent();}
        public void ShowCurrent()
        {
            review=false;if(Session.Run==null){ShowHome();return;}
            switch(Session.Run.phase)
            {
                case Phase.Map:ShowMap();break;case Phase.Build:ShowBoard();break;case Phase.Result:ShowResult();break;
                case Phase.Reward:ShowRewards();break;case Phase.Event:ShowEvent();break;case Phase.Finished:ShowFinal();break;
            }
        }
        void ShowMap()
        {
            NewScreen();var r=Session.Run;Heading("Semilla "+r.seed+" · Paciencia "+r.patience+"/3","Elegí el próximo chisme");
            Ui.Text(screen,"ETAPA "+(r.stage+1)+" / 6    ·    Susurros extra +"+r.bonus.whispers,new Vector2(.06f,.78f),new Vector2(.94f,.84f),20,Ui.Teal);
            var panel=Ui.Rect(screen,"RunMap",new Vector2(.04f,.13f),new Vector2(.96f,.78f));
            // Each layer connects to both alternatives in the next one; data, not UI order, controls reachability.
            Canvas.ForceUpdateCanvases();
            Func<MapNode,Vector2> pos=n=>new Vector2((n.stage==5?.5f:n.id.EndsWith("0")?.25f:.75f)*panel.rect.width,(.91f-n.stage*.158f)*panel.rect.height);
            foreach(var node in r.map)foreach(var id in node.next){var target=r.map.Find(n=>n.id==id);Ui.Line(panel,pos(node),pos(target),3,Ui.Pale);}
            var available=Session.AvailableNodes();
            foreach(var n in r.map)
            {
                var node=n;float x=n.stage==5?.25f:n.id.EndsWith("0")?0:.5f;float top=.98f-n.stage*.158f;
                string label=n.kind=="event"?"RONDA DE MATES\nDescanso o +1 Susurro":Catalog.Encounter(n.encounterId).name+"\n"+(n.kind=="boss"?"JEFE FINAL":"Riesgo "+Catalog.Encounter(n.encounterId).difficulty+" · mejora al ganar");
                bool visited=r.visited.Contains(n.id);if(visited)label="✓ "+label;
                var b=Ui.Button(panel,"Node_"+n.id,label,new Vector2(x,top-.125f),new Vector2(x+.5f,top),()=>Act(()=>Session.SelectNode(node.id)),n.stage==r.stage?Ui.Teal:visited?Ui.Orange:Ui.Muted,18);
                b.interactable=available.Any(a=>a.id==n.id);
            }
            Ui.Button(screen,"Home","Inicio",new Vector2(.06f,.04f),new Vector2(.34f,.11f),ShowHome,Ui.Ink,21);
            Ui.Text(screen,"Cada ruta lleva a la asamblea.\nLas mejoras duran esta run.",new Vector2(.37f,.035f),new Vector2(.94f,.115f),18,Ui.Muted);
        }
        void ShowBoard()
        {
            NewScreen();var e=Session.Encounter;var r=Session.Run;var n=Catalog.News(Session.Node.newsId);
            Ui.Text(screen,e.name,new Vector2(.04f,.94f),new Vector2(.76f,.995f),28);
            Ui.Button(screen,"BoardHome","Inicio",new Vector2(.78f,.94f),new Vector2(.98f,.995f),ShowHome,Ui.Ink,18).interactable=!playing;
            Ui.Text(screen,"“"+n.text+"”",new Vector2(.04f,.865f),new Vector2(.96f,.94f),23);
            Ui.Text(screen,string.Join("  ·  ",e.required.Select(o=>o.label)),new Vector2(.04f,.79f),new Vector2(.96f,.865f),20,Ui.Teal);
            budget=Ui.Text(screen,"",new Vector2(.04f,.745f),new Vector2(.96f,.79f),19);
            var board=Ui.Panel(screen,"VillageBoard",new Vector2(.025f,.305f),new Vector2(.975f,.745f),new Color32(231,232,203,255));
            // Restrained roof strips frame the board without covering the graph or touch targets.
            Ui.Panel(board.transform,"Street",new Vector2(.03f,.03f),new Vector2(.97f,.075f),new Color32(212,203,169,255));
            Ui.Panel(board.transform,"TownRoof",new Vector2(0,.91f),new Vector2(.25f,1),new Color32(208,154,107,255));
            Ui.Panel(board.transform,"TownRoof",new Vector2(.75f,.91f),new Vector2(1,1),new Color32(136,172,151,255));
            var lines=Ui.Rect(board.transform,"Connections",Vector2.zero,Vector2.one);graph=lines.gameObject.AddComponent<BoardGraphics>();graph.raycastTarget=false;graph.encounter=e;graph.active=r.edges;
            foreach(var p in e.residents)
            {
                var person=p;var rt=Ui.Rect(board.transform,"Resident_"+p.id,new Vector2(p.x,p.y),new Vector2(p.x,p.y));rt.sizeDelta=new Vector2(103,100);rt.anchoredPosition=Vector2.zero;
                var bg=rt.gameObject.AddComponent<Image>();bg.color=new Color(1,1,1,.10f);var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=bg;button.onClick.AddListener(()=>SelectResident(person.id));
                Ui.Avatar(rt,p.personality,new Vector2(.14f,.24f),new Vector2(.86f,.98f));
                Ui.Text(rt,(p.id==e.source?"• ":"")+p.name,new Vector2(-.08f,.08f),new Vector2(1.08f,.30f),16,Ui.Ink,TextAnchor.MiddleCenter);
                Ui.Text(rt,Catalog.Personality(p.personality).name,new Vector2(-.15f,-.09f),new Vector2(1.15f,.12f),13,Ui.Ink,TextAnchor.MiddleCenter);
                people[p.id]=rt;personButtons[p.id]=button;
            }
            context=Ui.Text(screen,Session.Save.settings.tutorial&&r.stage==0?"1. Tocá a Nora (• origen).\n2. Tocá un destino unido por línea punteada.\n3. Agregá ramas y pulsá Simular.":"Tocá emisor y destino para crear o quitar un enlace.\nTocá otro habitante para cambiar de emisor.",new Vector2(.04f,.17f),new Vector2(.96f,.295f),19);
            if(playing)
            {
                var pause=Ui.Button(screen,"Pause","Pausa",new Vector2(.03f,.085f),new Vector2(.35f,.16f),()=>{paused=!paused;UpdatePauseLabel();},Ui.Ink,20);pauseLabel=pause.GetComponentInChildren<Text>();
                Ui.Button(screen,"Speed",Session.Save.settings.fast?"Velocidad ×3":"Velocidad ×1",new Vector2(.35f,.085f),new Vector2(.68f,.16f),()=>{Session.Save.settings.fast=!Session.Save.settings.fast;Persist();var t=screen.Find("Speed").GetComponentInChildren<Text>();t.text=Session.Save.settings.fast?"Velocidad ×3":"Velocidad ×1";},Ui.Teal,18);
                Ui.Button(screen,"Skip","Saltar",new Vector2(.68f,.085f),new Vector2(.97f,.16f),()=>skip=true,Ui.Orange,20);
            }
            else if(review)
            {
                foreach(var step in r.lastSimulation.steps)graph.reached.Add(step.residentId);
                context.text="Tocá un habitante para revisar su versión de la noticia.";
                Ui.Button(screen,"BackToResult","Volver al resultado",new Vector2(.04f,.07f),new Vector2(.96f,.155f),ShowCurrent);
            }
            else
            {
                Ui.Button(screen,"Undo","Deshacer",new Vector2(.03f,.085f),new Vector2(.33f,.16f),()=>{Session.Undo();RefreshBoard();Persist();},Ui.Ink,20);
                Ui.Button(screen,"Clear","Reiniciar",new Vector2(.34f,.085f),new Vector2(.64f,.16f),()=>{Session.Clear();RefreshBoard();Persist();},Ui.Teal,20);
                Ui.Button(screen,"Simulate","Simular",new Vector2(.65f,.085f),new Vector2(.97f,.16f),BeginSimulation,Ui.Orange,20);
                Ui.Button(screen,"Inspect","Ficha del habitante",new Vector2(.12f,.015f),new Vector2(.88f,.08f),()=>{if(selected!=null)ShowResident(selected);else context.text="Seleccioná un habitante para abrir su ficha.";},Ui.Muted,19);
            }
            RefreshBoard();
        }
        void RefreshBoard()
        {
            if(graph==null)return;graph.active=Session.Run.edges;graph.selected=selected;graph.Refresh();
            foreach(var p in personButtons)p.Value.image.color=p.Key==selected?new Color(1,.75f,.2f,.45f):new Color(1,1,1,.10f);
            budget.text=playing?"RECIBIERON "+shown+" / "+Session.Encounter.residents.Count+"     ·     La red está bloqueada":"SUSURROS "+(Session.Encounter.budget+Session.Run.bonus.whispers-Session.Run.edges.Count)+" / "+(Session.Encounter.budget+Session.Run.bonus.whispers)+"     ·     PACIENCIA "+Session.Run.patience+"/3";
        }
        public void SelectResident(string id)
        {
            if(playing)return;
            if(review){ShowResident(id);return;}
            var person=Session.Encounter.Person(id);
            if(selected==id){selected=null;context.text="Seleccioná otro emisor.";}
            else if(selected!=null&&Session.Encounter.potential.Any(e=>e.from==selected&&e.to==id))
            {
                bool removing=Session.Run.edges.Any(e=>e.from==selected&&e.to==id);
                string error=Session.ToggleEdge(selected,id);if(error==null&&!removing)selected=id;
                context.text=error??"Red actualizada. Emisor seleccionado: "+Session.Encounter.Person(selected).name+". Tocá el origen para crear otra rama.";Sound();Persist();
            }
            else {selected=id;context.text=person.name+" · "+Catalog.Personality(person.personality).name+"\n"+Catalog.Personality(person.personality).description+"\nElegí un destino por una relación resaltada.";}
            RefreshBoard();
        }
        void ShowResident(string id)
        {
            var p=Session.Encounter.Person(id);var step=Session.Run.lastSimulation==null?null:Session.Run.lastSimulation.At(id);
            bool wasReview=review;NewScreen();Heading(Catalog.Personality(p.personality).name,p.name);
            var text=new StringBuilder(Catalog.Personality(p.personality).description+"\n\n");
            if(step==null)text.Append("Todavía no recibió esta noticia.\n\nNoticia inicial:\n"+Catalog.News(Session.Node.newsId).text+"\n"+Catalog.News(Session.Node.newsId).stats);
            else
            {
                text.Append("<b>Su versión</b>\n"+step.after.originalText+"\n\n"+step.after.stats+"\n\n");
                foreach(var claim in step.after.claims)text.Append("• "+claim.text+" ["+claim.kind+"]\n\n");
                text.Append("<b>Cómo llegó hasta acá</b>\n"+string.Join(" → ",step.after.path.Select(s=>Session.Encounter.Person(s).name))+"\n\n");
                foreach(var h in step.after.history)text.Append(h+"\n\n");
            }
            Ui.ScrollText(screen,text.ToString(),new Vector2(.05f,.16f),new Vector2(.95f,.81f),23);
            Ui.Button(screen,"BackToBoard","Volver al pueblo",new Vector2(.06f,.045f),new Vector2(.94f,.13f),()=>{review=wasReview;ShowBoard();});
        }
        public void BeginSimulation()
        {
            if(playing)return;
            try{Session.Simulate();}
            catch(InvalidOperationException ex){context.text=ex.Message;return;}
            Persist();playing=true;paused=false;skip=false;shown=0;ShowBoard();StartCoroutine(PlaySteps());
        }
        IEnumerator PlaySteps()
        {
            foreach(var step in Session.Run.lastSimulation.steps)
            {
                if(skip)break;
                graph.lit=step.residentId;graph.reached.Add(step.residentId);shown++;
                var p=Session.Encounter.Person(step.residentId);
                context.text=p.name+" · "+step.effect+"\n"+step.after.stats+"\n"+step.after.claims.Last().text;
                Sound(1+step.depth*.12f);RefreshBoard();float elapsed=0;
                while(elapsed<1.25f)
                {
                    if(skip)break;
                    if(!paused){elapsed+=Time.unscaledDeltaTime*(Session.Save.settings.fast?3:1);people[p.id].localScale=Vector3.one*(1+.16f*Mathf.Sin(Mathf.Min(1,elapsed/1.25f)*Mathf.PI));}
                    yield return null;
                }
                people[p.id].localScale=Vector3.one;
            }
            playing=false;paused=false;ShowCurrent();
        }
        void UpdatePauseLabel(){if(pauseLabel!=null)pauseLabel.text=paused?"Seguir":"Pausa";}
        void ShowResult()
        {
            NewScreen();var r=Session.Run;var result=r.lastEvaluation;
            Heading("Encuentro "+(r.stage+1)+" · "+Session.Encounter.name,result.won?"¡El pueblo habla!":"No salió como esperabas");
            Ui.Text(screen,result.won?new string('★',result.stars)+new string('☆',3-result.stars):"−1 Paciencia · quedan "+r.patience,new Vector2(.06f,.75f),new Vector2(.94f,.83f),32,result.won?Ui.Orange:Ui.Ink);
            var lines=new StringBuilder();
            foreach(var condition in result.conditions)lines.Append((condition.passed?"✓ ":"× ")+condition.label+"\nObtenido: "+condition.actual.ToString("0.#",CultureInfo.InvariantCulture)+"\n\n");
            lines.Append("Alcance: "+r.lastSimulation.reach+" / "+Session.Encounter.residents.Count+"\nSusurros usados: "+r.edges.Count+"\n\n");
            lines.Append("<b>Estrellas extra</b>\nEficiencia: usar como máximo "+Session.Encounter.efficientEdges+" enlaces.\n");
            foreach(var secondary in Session.Encounter.secondary){var check=Objectives.Check(secondary,r.lastSimulation,Session.Encounter.residents.Count);lines.Append((check.passed?"✓ ":"× ")+secondary.label+"\n");}
            lines.Append("\nLa importancia y el pánico colectivos se dividen entre todos los habitantes, incluidos los que no recibieron la noticia.");
            Ui.ScrollText(screen,lines.ToString(),new Vector2(.05f,.30f),new Vector2(.95f,.75f),23);
            Ui.Button(screen,"ReviewBoard","Ver versiones y recorrido",new Vector2(.06f,.215f),new Vector2(.94f,.29f),()=>{review=true;ShowBoard();},Ui.Ink,21);
            if(!result.won)Ui.Button(screen,"Retry","Reintentar · otro fallo cuesta 1",new Vector2(.06f,.13f),new Vector2(.94f,.205f),()=>Act(Session.Retry),Ui.Teal,21);
            Ui.Button(screen,"AcceptResult",result.won?(Session.Encounter.boss?"Ver final":"Elegir mejora"):(Session.Encounter.boss?"Abandonar run":"Seguir sin recompensa"),new Vector2(.06f,.04f),new Vector2(.94f,.12f),()=>Act(Session.AcceptResult),Ui.Orange,22);
        }
        void ShowRewards()
        {
            NewScreen();Heading("El barrio te da una mano","Elegí una mejora");
            Ui.Text(screen,"Solo una. Sus efectos duran esta run.",new Vector2(.06f,.75f),new Vector2(.94f,.82f),23,Ui.Muted);
            for(int i=0;i<Session.Run.rewardIds.Count;i++)
            {
                string id=Session.Run.rewardIds[i];var reward=Catalog.Reward(id);float y=.72f-i*.19f;
                Ui.Button(screen,"Reward_"+id,"<b>"+reward.name+"</b>\n\n"+reward.description,new Vector2(.06f,y-.17f),new Vector2(.94f,y),()=>Act(()=>Session.ChooseReward(id)),i==0?Ui.Teal:i==1?Ui.Ink:Ui.Orange,22);
            }
            Ui.Text(screen,"La elección se guarda inmediatamente.",new Vector2(.06f,.07f),new Vector2(.94f,.14f),20,Ui.Muted,TextAnchor.MiddleCenter);
        }
        void ShowEvent()
        {
            NewScreen();Heading("Encuentro tranquilo","Una ronda de mates");
            var town=Ui.Rect(screen,"Cafe",new Vector2(.16f,.52f),new Vector2(.84f,.79f));Ui.Town(town);
            Ui.Text(screen,"La vecina ofrece una pausa o un nuevo contacto.\n\nPaciencia actual: "+Session.Run.patience+" / 3",new Vector2(.07f,.33f),new Vector2(.93f,.5f),26);
            Ui.Button(screen,"EventRest","Tomar mate · +1 Paciencia (máx. 3)",new Vector2(.06f,.21f),new Vector2(.94f,.31f),()=>Act(()=>Session.ChooseEvent(true)),Ui.Teal,23);
            Ui.Button(screen,"EventContact","Conocer a alguien · +1 Susurro",new Vector2(.06f,.09f),new Vector2(.94f,.19f),()=>Act(()=>Session.ChooseEvent(false)),Ui.Orange,23);
        }
        void ShowFinal()
        {
            NewScreen();var r=Session.Run;Heading("Semilla "+r.seed,r.won?"La leyenda de Villa Susurro":"El pueblo pide silencio");
            Ui.Avatar(screen,r.won?"exagerado":"miedoso",new Vector2(.34f,.58f),new Vector2(.66f,.81f));
            string summary=(r.won?"¡La asamblea escuchó tus versiones!\nTítulo: Cronista de Villa Susurro":"Se terminó la run. Probá otras ramas en la próxima.")+"\n\nDesafíos ganados: "+r.wins+"\nFracasos: "+r.failures+"\nEstrellas: "+r.totalStars+"\nMejoras elegidas: "+r.chosenRewards.Count;
            if(r.lastEvaluation!=null&&!r.lastEvaluation.won)summary+="\n\nFaltó:\n"+string.Join("\n",r.lastEvaluation.conditions.Where(x=>!x.passed).Select(x=>x.label+" (obtenido "+x.actual.ToString("0.#",CultureInfo.InvariantCulture)+")"));
            Ui.ScrollText(screen,summary,new Vector2(.06f,.32f),new Vector2(.94f,.57f),24);
            if(r.lastSimulation!=null)Ui.Button(screen,"ReviewFinal","Revisar la última red",new Vector2(.06f,.23f),new Vector2(.94f,.31f),()=>{review=true;ShowBoard();},Ui.Teal,21);
            Ui.Button(screen,"NewRun","Otra historia",new Vector2(.06f,.13f),new Vector2(.94f,.22f),ShowNewRun,Ui.Orange);
            Ui.Button(screen,"Home","Inicio",new Vector2(.06f,.03f),new Vector2(.94f,.12f),ShowHome,Ui.Ink);
        }
        void ShowSettings()
        {
            NewScreen();Heading("A tu ritmo","Ajustes y reglas");var settings=Session.Save.settings;
            Ui.Button(screen,"Volume","Sonido: "+Mathf.RoundToInt(settings.volume*100)+"%",new Vector2(.06f,.72f),new Vector2(.94f,.81f),()=>{settings.volume=settings.volume>=1?0:settings.volume+.25f;Persist();Sound();ShowSettings();});
            Ui.Button(screen,"Fast","Animación rápida: "+(settings.fast?"sí":"no"),new Vector2(.06f,.62f),new Vector2(.94f,.71f),()=>{settings.fast=!settings.fast;Persist();ShowSettings();});
            Ui.Button(screen,"Tutorial","Ayuda inicial: "+(settings.tutorial?"sí":"no"),new Vector2(.06f,.52f),new Vector2(.94f,.61f),()=>{settings.tutorial=!settings.tutorial;Persist();ShowSettings();});
            Ui.ScrollText(screen,"<b>Cómo se juega</b>\n\nTocá el emisor y luego un destino disponible. Las flechas muestran la dirección. Volvé a tocar un enlace activo para quitar su rama. Tocá otro habitante para cambiar de emisor.\n\nCada enlace cuesta 1 Susurro. Máximo dos salidas y una entrada. Sin ciclos. El origen (•) también transforma el rumor.\n\nIMP: Importancia · PAN: Pánico\nCRE: Credibilidad · DIS: Distorsión\n\nCumplí TODOS los objetivos. Cada fracaso quita 1 Paciencia, incluso al reintentar. Las recompensas duran una run. Revisá las fichas para entender las transformaciones.\n\nPausa, velocidad y saltar solo cambian la animación. El resultado se guarda antes de mostrarla.\n\nArte y sonidos procedurales originales. Fuente DejaVu Sans (licencia incluida).",new Vector2(.06f,.15f),new Vector2(.94f,.51f),23);
            Ui.Button(screen,"Back","Volver al inicio",new Vector2(.06f,.04f),new Vector2(.94f,.13f),ShowHome,Ui.Ink);
        }
    }
}
