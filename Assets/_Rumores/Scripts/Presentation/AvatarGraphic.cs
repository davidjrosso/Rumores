using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    public sealed class AvatarGraphic : MaskableGraphic
    {
        public string personality;
        static Color Ink=new Color(.16f,.22f,.24f);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var center=r.center;
            float scale=Mathf.Min(r.width,r.height)/90f;
            var color=personality=="miedoso"?new Color(.37f,.64f,.75f):personality=="exagerado"?new Color(.94f,.55f,.29f):personality=="mentiroso"?new Color(.68f,.49f,.73f):new Color(.56f,.68f,.37f);
            Disc(vh,center+new Vector2(0,-12)*scale,31*scale,Ink);
            Disc(vh,center+new Vector2(0,-11)*scale,28*scale,color);
            Disc(vh,center+new Vector2(0,8)*scale,25*scale,Ink);
            Disc(vh,center+new Vector2(0,8)*scale,22*scale,new Color(1,.83f,.61f));
            Disc(vh,center+new Vector2(-8,12)*scale,3*scale,Ink);
            Disc(vh,center+new Vector2(8,12)*scale,3*scale,Ink);
            if(personality=="miedoso") Disc(vh,center+new Vector2(0,0)*scale,5*scale,Ink);
            else if(personality=="exagerado") {Disc(vh,center+new Vector2(0,-1)*scale,7*scale,Ink);Disc(vh,center+new Vector2(0,2)*scale,5*scale,Color.white);}
            else if(personality=="mentiroso") {Disc(vh,center+new Vector2(7,1)*scale,3*scale,Ink);Disc(vh,center+new Vector2(13,30)*scale,8*scale,color);}
            else {Disc(vh,center+new Vector2(-8,12)*scale,7*scale,Ink);Disc(vh,center+new Vector2(8,12)*scale,7*scale,Ink);Disc(vh,center+new Vector2(-8,12)*scale,4*scale,Color.white);Disc(vh,center+new Vector2(8,12)*scale,4*scale,Color.white);}
        }
        static void Disc(VertexHelper vh,Vector2 center,float radius,Color c)
        {
            int start=vh.currentVertCount;vh.AddVert(center,c,Vector2.zero);
            const int sides=24;
            for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
        }
    }
}
