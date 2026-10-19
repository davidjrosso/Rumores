using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Short deterministic decoration, no random gameplay calls or input blocking.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ReactionBurstGraphic : MaskableGraphic
    {
        bool celebration,reduced;
        float elapsed;
        public bool IsPlaying {get;private set;}
        protected override void Awake(){base.Awake();raycastTarget=false;}
        public void Play(bool celebrate,bool reduceMotion)
        {celebration=celebrate;reduced=reduceMotion;elapsed=0;IsPlaying=true;raycastTarget=false;SetVerticesDirty();}
        public static ReactionBurstGraphic Create(Transform parent,Vector2 min,Vector2 max)
        {return Ui.Rect(parent,"ReactionBurst",min,max).gameObject.AddComponent<ReactionBurstGraphic>();}
        void Update()
        {
            if(!IsPlaying)return;
            elapsed+=Time.unscaledDeltaTime;
            if(elapsed>=(reduced?.12f:.65f))IsPlaying=false;
            SetVerticesDirty();
        }
        protected override void OnDisable(){IsPlaying=false;elapsed=0;base.OnDisable();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!IsPlaying||reduced)return;
            var rect=GetPixelAdjustedRect();float t=Mathf.Clamp01(elapsed/.65f);
            for(int i=0;i<(celebration?18:6);i++)
            {
                // Confetti stays at the edges, leaving a quiet central reading area.
                float x=celebration?(i%2==0?.03f+(i%5)*.025f:.97f-(i%5)*.025f):.25f+(i%3)*.25f;
                float y=.85f-(i%6)*.08f-t*.35f;
                Color tint=celebration?(i%3==0?(Color)new Color32(243,201,91,255):i%3==1?Ui.Orange:Ui.Teal):(Color)new Color32(131,199,222,255);
                tint.a=1-t;
                Vector2 center=rect.min+Vector2.Scale(rect.size,new Vector2(x,y));
                float angle=(i*47+t*180)*Mathf.Deg2Rad;
                Vector2 side=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(celebration?5:3);
                Vector2 up=new Vector2(-side.y,side.x)*(celebration?.5f:2);
                int start=vh.currentVertCount;
                vh.AddVert(center-side-up,tint,Vector2.zero);vh.AddVert(center-side+up,tint,Vector2.zero);
                vh.AddVert(center+side+up,tint,Vector2.zero);vh.AddVert(center+side-up,tint,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
