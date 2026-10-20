using UnityEngine;

namespace Rumores.Presentation
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PageTransition : MonoBehaviour
    {
        CanvasGroup group;
        float elapsed,duration;
        bool reducedMotion;
        public bool Ready {get;private set;}
        public void Begin(bool reduced)
        {
            group=GetComponent<CanvasGroup>();reducedMotion=reduced;duration=reduced?.06f:.18f;elapsed=0;Ready=false;
            group.alpha=reduced?1:0;group.interactable=false;group.blocksRaycasts=false;
        }
        void Update()
        {
            if(Ready||group==null)return;
            elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/duration);
            group.alpha=reducedMotion?1:Mathf.SmoothStep(0,1,t);
            if(t>=1){Ready=true;group.interactable=true;group.blocksRaycasts=true;}
        }
    }
}
