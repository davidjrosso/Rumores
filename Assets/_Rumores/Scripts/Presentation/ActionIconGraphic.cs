using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Original vector icons. Labels stay in Text components; no gameplay dependency.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ActionIconGraphic : MaskableGraphic
    {
        public string ActionId {get;private set;}="mentir";
        public void SetAction(string id){ActionId=id;SetVerticesDirty();}
        public static ActionIconGraphic Create(Transform parent,string id,Vector2 min,Vector2 max)
        {
            var rect=Ui.Rect(parent,"Icon_"+id,min,max);
            var icon=rect.gameObject.AddComponent<ActionIconGraphic>();icon.raycastTarget=false;icon.SetAction(id);return icon;
        }
        public static string ForPersonality(string id)
        {
            switch(id){case "miedoso":return "panico";case "exagerado":return "exagerar";case "ignorante":return "confundir";default:return "mentir";}
        }
        Rect square;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=GetPixelAdjustedRect();float side=Mathf.Min(rect.width,rect.height);
            square=new Rect(rect.center-Vector2.one*side*.5f,Vector2.one*side);
            Disc(vh,.5f,.5f,.49f,.49f,Ui.Ink);Disc(vh,.5f,.5f,.43f,.43f,Ui.Cream);
            Color accent=Ui.Teal;
            switch(ActionId)
            {
                case "panico":
                    accent=new Color32(69,147,179,255);
                    Poly(vh,accent,new Vector2(.5f,.82f),new Vector2(.28f,.44f),new Vector2(.72f,.44f));
                    Disc(vh,.5f,.42f,.22f,.22f,accent);Disc(vh,.43f,.40f,.045f,.09f,Ui.Cream);break;
                case "exagerar":
                    accent=Ui.Orange;
                    Poly(vh,accent,new Vector2(.23f,.40f),new Vector2(.23f,.62f),new Vector2(.67f,.78f),new Vector2(.67f,.24f));
                    Line(vh,new Vector2(.36f,.43f),new Vector2(.42f,.20f),.10f,Ui.Ink);
                    Line(vh,new Vector2(.75f,.62f),new Vector2(.84f,.70f),.045f,accent);
                    Line(vh,new Vector2(.76f,.49f),new Vector2(.89f,.49f),.045f,accent);break;
                case "confundir":
                    accent=Ui.Teal;
                    Line(vh,new Vector2(.32f,.67f),new Vector2(.44f,.77f),.09f,accent);
                    Line(vh,new Vector2(.44f,.77f),new Vector2(.65f,.72f),.09f,accent);
                    Line(vh,new Vector2(.65f,.72f),new Vector2(.66f,.57f),.09f,accent);
                    Line(vh,new Vector2(.66f,.57f),new Vector2(.50f,.46f),.09f,accent);
                    Line(vh,new Vector2(.50f,.46f),new Vector2(.50f,.37f),.09f,accent);
                    Disc(vh,.5f,.23f,.055f,.055f,accent);break;
                case "teoria":
                    Disc(vh,.5f,.58f,.23f,.25f,Ui.Ink);Disc(vh,.5f,.60f,.17f,.18f,new Color32(243,201,91,255));
                    Line(vh,new Vector2(.4f,.30f),new Vector2(.6f,.30f),.07f,Ui.Ink);
                    Line(vh,new Vector2(.43f,.21f),new Vector2(.57f,.21f),.06f,Ui.Ink);
                    Line(vh,new Vector2(.18f,.60f),new Vector2(.24f,.60f),.045f,Ui.Orange);
                    Line(vh,new Vector2(.76f,.60f),new Vector2(.82f,.60f),.045f,Ui.Orange);break;
                case "meme":
                    Disc(vh,.5f,.50f,.30f,.30f,new Color32(243,201,91,255));
                    Line(vh,new Vector2(.30f,.59f),new Vector2(.39f,.65f),.05f,Ui.Ink);
                    Line(vh,new Vector2(.39f,.65f),new Vector2(.46f,.59f),.05f,Ui.Ink);
                    Line(vh,new Vector2(.55f,.59f),new Vector2(.63f,.65f),.05f,Ui.Ink);
                    Line(vh,new Vector2(.63f,.65f),new Vector2(.72f,.59f),.05f,Ui.Ink);
                    Disc(vh,.5f,.40f,.15f,.12f,Ui.Ink);break;
                default:
                    accent=new Color32(140,110,178,255);
                    Disc(vh,.50f,.55f,.29f,.22f,accent);
                    Poly(vh,accent,new Vector2(.30f,.47f),new Vector2(.26f,.22f),new Vector2(.50f,.41f));
                    for(int i=0;i<3;i++)Disc(vh,.35f+i*.15f,.55f,.035f,.035f,Ui.Cream);break;
            }
        }
        Vector2 Point(Vector2 p){return square.min+Vector2.Scale(square.size,p);}
        void Poly(VertexHelper vh,Color tint,params Vector2[] points)
        {
            int first=vh.currentVertCount;
            foreach(var point in points)vh.AddVert(Point(point),tint,Vector2.zero);
            for(int i=1;i<points.Length-1;i++)vh.AddTriangle(first,first+i,first+i+1);
        }
        void Disc(VertexHelper vh,float x,float y,float rx,float ry,Color tint)
        {
            int first=vh.currentVertCount;vh.AddVert(Point(new Vector2(x,y)),tint,Vector2.zero);
            for(int i=0;i<=24;i++){float angle=i*Mathf.PI/12;vh.AddVert(Point(new Vector2(x+rx*Mathf.Cos(angle),y+ry*Mathf.Sin(angle))),tint,Vector2.zero);}
            for(int i=0;i<24;i++)vh.AddTriangle(first,first+i+1,first+i+2);
        }
        void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint)
        {var delta=(b-a).normalized;var normal=new Vector2(-delta.y,delta.x)*width*.5f;Poly(vh,tint,a-normal,a+normal,b+normal,b-normal);}
    }
}
