using System.Collections.Generic;
using Rumores.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BoardGraphics : MaskableGraphic
    {
        public Encounter encounter;
        public List<Edge> active=new List<Edge>();
        public string selected,lit;
        public readonly HashSet<string> reached=new HashSet<string>();
        public void Refresh() {SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if(encounter==null)return;
            var rect=rectTransform.rect;
            foreach(var e in encounter.potential)
            {
                var from=encounter.Person(e.from);var to=encounter.Person(e.to);
                Vector2 a=new Vector2(rect.xMin+from.x*rect.width,rect.yMin+from.y*rect.height);
                Vector2 b=new Vector2(rect.xMin+to.x*rect.width,rect.yMin+to.y*rect.height);
                var direction=(b-a).normalized; a+=direction*42;b-=direction*45;
                bool on=active.Exists(x=>x.from==e.from&&x.to==e.to);
                var c=on?new Color(.15f,.34f,.34f,1):new Color(.29f,.43f,.41f,.18f);
                if(e.from==selected&&!on)c=new Color(.2f,.45f,.75f,.65f);
                if(on&&reached.Contains(e.to))c=new Color(.84f,.4f,.12f,1);
                if(on&&e.to==lit)c=new Color(1,.65f,.1f,1);
                if(on) Line(vh,a,b,5,c); else {for(float t=0;t<1;t+=.13f)Line(vh,Vector2.Lerp(a,b,t),Vector2.Lerp(a,b,Mathf.Min(1,t+.065f)),2,c);}
                var side=new Vector2(-direction.y,direction.x);
                Triangle(vh,b,b-direction*13+side*7,b-direction*13-side*7,c);
            }
        }
        static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c)
        {
            var side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
            int i=vh.currentVertCount;vh.AddVert(a+side,c,Vector2.zero);vh.AddVert(b+side,c,Vector2.zero);vh.AddVert(b-side,c,Vector2.zero);vh.AddVert(a-side,c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
        static void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color color)
        {int i=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
    }
}
