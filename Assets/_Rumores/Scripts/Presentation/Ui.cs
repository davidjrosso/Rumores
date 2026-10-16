using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    public static class Ui
    {
        public static readonly Color Ink=new Color32(31,53,57,255), Cream=new Color32(250,244,225,255), Teal=new Color32(44,107,101,255), Orange=new Color32(218,100,58,255), Muted=new Color32(104,119,110,255), Pale=new Color32(223,231,208,255);
        static Font font;
        public static Font Font {get {if(font==null)font=Resources.Load<Font>("RumoresSans");return font;}}
        public static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max,float inset=0)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=new Vector2(inset,inset);rt.offsetMax=new Vector2(-inset,-inset);return rt;
        }
        public static Image Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color,float inset=0)
        {var rt=Rect(parent,name,min,max,inset);var im=rt.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;}
        public static Text Text(Transform parent,string text,Vector2 min,Vector2 max,int size=24,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var rt=Rect(parent,"Label",min,max,4);var t=rt.gameObject.AddComponent<Text>();t.font=Font;t.text=text;t.fontSize=size;t.color=color??Ink;t.alignment=align;t.supportRichText=true;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        public static Button Button(Transform parent,string name,string label,Vector2 min,Vector2 max,Action action,Color? color=null,int size=24)
        {
            var rt=Rect(parent,name,min,max,5);var img=rt.gameObject.AddComponent<Image>();img.color=color??Teal;
            var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=img;var colors=b.colors;colors.highlightedColor=new Color(.9f,.95f,.9f);colors.pressedColor=new Color(.65f,.8f,.75f);colors.disabledColor=new Color(.7f,.73f,.7f,.55f);b.colors=colors;
            Text(rt,label,new Vector2(.03f,.05f),new Vector2(.97f,.95f),size,Cream,TextAnchor.MiddleCenter);
            b.onClick.AddListener(()=>action());return b;
        }
        public static InputField Input(Transform parent,string initial,Vector2 min,Vector2 max)
        {
            var rt=Rect(parent,"SeedInput",min,max,5);var bg=rt.gameObject.AddComponent<Image>();bg.color=Color.white;
            var input=rt.gameObject.AddComponent<InputField>();var t=Text(rt,initial,new Vector2(.04f,0),new Vector2(.96f,1),32);input.textComponent=t;input.targetGraphic=bg;input.contentType=InputField.ContentType.IntegerNumber;input.characterLimit=11;input.text=initial;return input;
        }
        public static Text ScrollText(Transform parent,string message,Vector2 min,Vector2 max,int size=23)
        {
            var rt=Rect(parent,"Scroll",min,max,5);var bg=rt.gameObject.AddComponent<Image>();bg.color=new Color(1,1,1,.3f);var scroll=rt.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var view=Rect(rt,"Viewport",Vector2.zero,Vector2.one,12);view.gameObject.AddComponent<RectMask2D>();
            var content=Rect(view,"Content",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;content.anchoredPosition=Vector2.zero;
            var text=content.gameObject.AddComponent<Text>();text.font=Font;text.text=message;text.fontSize=size;text.color=Ink;text.supportRichText=true;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;text.raycastTarget=false;
            var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=view;scroll.content=content;return text;
        }
        public static void Line(Transform parent,Vector2 a,Vector2 b,float thickness,Color color)
        {
            var rt=Rect(parent,"Route",Vector2.zero,Vector2.zero);rt.pivot=new Vector2(0,.5f);rt.anchoredPosition=a;rt.sizeDelta=new Vector2(Vector2.Distance(a,b),thickness);rt.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);var img=rt.gameObject.AddComponent<Image>();img.color=color;img.raycastTarget=false;
        }
        public static void Avatar(Transform parent,string personality,Vector2 min,Vector2 max)
        {var rt=Rect(parent,"Avatar",min,max);var a=rt.gameObject.AddComponent<AvatarGraphic>();a.personality=personality;a.raycastTarget=false;}
        public static void Town(Transform parent)
        {
            Panel(parent,"Road",new Vector2(.03f,.40f),new Vector2(.97f,.49f),new Color32(215,206,171,255));
            Panel(parent,"Road",new Vector2(.45f,.05f),new Vector2(.55f,.95f),new Color32(215,206,171,255));
            for(int i=0;i<6;i++)
            {
                float x=i%3*.31f+.05f;float y=i<3?.60f:.12f;
                var h=Panel(parent,"House",new Vector2(x,y),new Vector2(x+.22f,y+.22f),i%2==0?new Color32(225,165,109,255):new Color32(160,191,166,255));
                Panel(h.transform,"Roof",new Vector2(-.05f,.68f),new Vector2(1.05f,1.05f),i%2==0?Orange:Teal);
                Panel(h.transform,"Door",new Vector2(.38f,0),new Vector2(.62f,.37f),Ink);
                Panel(h.transform,"Window",new Vector2(.12f,.34f),new Vector2(.29f,.55f),Cream);
            }
        }
    }
}
