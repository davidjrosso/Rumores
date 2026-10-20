using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    public static class PartyLayout
    {
        public static readonly Color Accent=new Color32(163,58,31,255);
        public static RectTransform Flow(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var viewport=Ui.Rect(parent,name,min,max);
            var background=viewport.gameObject.AddComponent<Image>();background.color=new Color(1,1,1,.12f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            scroll.viewport=viewport;scroll.scrollSensitivity=35;
            var content=Ui.Rect(viewport,"Content",new Vector2(0,1),Vector2.one);
            content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;scroll.content=content;
            Stack(content,12,16);content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            return content;
        }
        public static void Stack(RectTransform rect,int padding,int spacing)
        {
            var layout=rect.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(padding,padding,padding,padding);
            layout.spacing=spacing;layout.childControlHeight=true;layout.childControlWidth=true;
            layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
        }
        public static Text Paragraph(Transform parent,string value,int size=32,Color? color=null)
        {
            var text=Ui.Text(parent,value,Vector2.zero,Vector2.one,size,color);text.supportRichText=false;
            text.verticalOverflow=VerticalWrapMode.Overflow;text.lineSpacing=1.08f;return text;
        }
        public static RectTransform Card(Transform parent,string name,Color color)
        {
            var rect=Ui.Panel(parent,name,Vector2.zero,Vector2.one,color).rectTransform;Stack(rect,18,10);return rect;
        }
        public static Slider VolumeSlider(Transform parent,string name,Vector2 min,Vector2 max,float initial,Action<float> changed)
        {
            var rect=Ui.Rect(parent,name,min,max);rect.gameObject.AddComponent<Image>().color=Color.clear;
            var slider=rect.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;
            var track=Ui.Panel(rect,"Track",new Vector2(.04f,.40f),new Vector2(.96f,.60f),Ui.Pale);
            var fill=Ui.Panel(track.transform,"Fill",Vector2.zero,Vector2.one,Ui.Teal);slider.fillRect=fill.rectTransform;
            var area=Ui.Rect(rect,"HandleArea",new Vector2(.06f,0),new Vector2(.94f,1));
            var handle=Ui.Panel(area,"Handle",new Vector2(0,.5f),new Vector2(0,.5f),Ui.Teal);
            handle.rectTransform.sizeDelta=new Vector2(88,88);slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
            slider.direction=Slider.Direction.LeftToRight;slider.value=initial;
            slider.onValueChanged.AddListener(value=>changed(value));return slider;
        }
    }
}
