using UnityEngine;

namespace Rumores.Presentation
{
    // A visual path only. Split is demonstrated in ArtPreview, never added to party rules.
    public sealed class RumorSignal : MonoBehaviour
    {
        RectTransform first,second;
        bool split,fast;
        float elapsed;
        public bool IsPlaying {get;private set;}
        public void Play(bool divide,bool quick)
        {
            split=divide;fast=quick;elapsed=0;IsPlaying=true;
            if(first==null)
            {
                first=SpeechBubbleGraphic.Create(transform,"RumorA",Vector2.zero,Vector2.zero,new Color32(243,201,91,255));
                second=SpeechBubbleGraphic.Create(transform,"RumorB",Vector2.zero,Vector2.zero,new Color32(175,150,205,255));
                first.sizeDelta=second.sizeDelta=new Vector2(36,30);
            }
            first.gameObject.SetActive(true);second.gameObject.SetActive(split);Position(0);
        }
        void Update()
        {
            if(!IsPlaying)return;
            elapsed+=Time.unscaledDeltaTime;float progress=Mathf.Clamp01(elapsed/(fast?.12f:.45f));
            Position(progress);
            if(progress>=1){IsPlaying=false;first.gameObject.SetActive(false);second.gameObject.SetActive(false);}
        }
        void Position(float progress)
        {
            Set(first,new Vector2(Mathf.Lerp(.1f,.9f,progress),split?Mathf.Lerp(.5f,.8f,progress):.5f));
            Set(second,new Vector2(Mathf.Lerp(.1f,.9f,progress),Mathf.Lerp(.5f,.2f,progress)));
        }
        static void Set(RectTransform rect,Vector2 anchor){rect.anchorMin=rect.anchorMax=anchor;rect.anchoredPosition=Vector2.zero;}
        void OnDisable()
        {IsPlaying=false;elapsed=0;if(first!=null)first.gameObject.SetActive(false);if(second!=null)second.gameObject.SetActive(false);}
    }
}
