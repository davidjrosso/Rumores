using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Vector placeholder mark: text remains editable in a separate Text component.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SpeechBubbleGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=GetPixelAdjustedRect();
            Ellipse(vh,rect,new Vector2(.5f,.58f),new Vector2(.49f,.38f),Ui.Ink);
            Triangle(vh,rect,new Vector2(.19f,.36f),new Vector2(.17f,.02f),new Vector2(.47f,.30f),Ui.Ink);
            Ellipse(vh,rect,new Vector2(.5f,.58f),new Vector2(.43f,.31f),color);
            for(int i=0;i<3;i++)Ellipse(vh,rect,new Vector2(.32f+.18f*i,.58f),new Vector2(.045f,.055f),Ui.Ink);
        }
        static Vector2 Point(Rect rect,Vector2 p){return rect.min+Vector2.Scale(rect.size,p);}
        static void Ellipse(VertexHelper vh,Rect rect,Vector2 center,Vector2 radius,Color tint)
        {
            int first=vh.currentVertCount;vh.AddVert(Point(rect,center),tint,Vector2.zero);
            for(int i=0;i<=32;i++)
            {float a=i*Mathf.PI*2/32;vh.AddVert(Point(rect,center+Vector2.Scale(radius,new Vector2(Mathf.Cos(a),Mathf.Sin(a)))),tint,Vector2.zero);}
            for(int i=0;i<32;i++)vh.AddTriangle(first,first+i+1,first+i+2);
        }
        static void Triangle(VertexHelper vh,Rect rect,Vector2 a,Vector2 b,Vector2 c,Color tint)
        {int start=vh.currentVertCount;vh.AddVert(Point(rect,a),tint,Vector2.zero);vh.AddVert(Point(rect,b),tint,Vector2.zero);vh.AddVert(Point(rect,c),tint,Vector2.zero);vh.AddTriangle(start,start+1,start+2);}
        public static RectTransform Create(Transform parent,string name,Vector2 min,Vector2 max,Color tint)
        {
            var rect=Ui.Rect(parent,name,min,max);var graphic=rect.gameObject.AddComponent<SpeechBubbleGraphic>();
            graphic.color=tint;graphic.raycastTarget=false;return rect;
        }
    }
}
