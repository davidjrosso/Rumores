using UnityEngine;

namespace Rumores.Presentation
{
    public enum CharacterReaction {Idle,Talk,Surprise,Panic,Laugh,Celebrate,Defeat}

    // Presentation only: no turn tokens, model references or completion callbacks.
    [RequireComponent(typeof(RectTransform))]
    public sealed class CharacterMotion : MonoBehaviour
    {
        RectTransform rect;
        Vector3 restScale;
        Vector2 restPosition;
        Quaternion restRotation;
        float elapsed;
        CharacterReaction queued;
        public bool Fast {get;set;}
        public CharacterReaction Reaction {get;private set;}
        public bool IsReacting {get {return Reaction!=CharacterReaction.Idle;}}
        void Awake()
        {
            rect=(RectTransform)transform;restScale=rect.localScale;
            restPosition=rect.anchoredPosition;restRotation=rect.localRotation;
        }
        public void Play(CharacterReaction reaction) {Restore();Reaction=reaction;queued=CharacterReaction.Idle;elapsed=0;}
        public void SpeakThen(CharacterReaction reaction) {Play(CharacterReaction.Talk);queued=reaction;}
        public static float Duration(CharacterReaction reaction)
        {
            switch(reaction)
            {
                case CharacterReaction.Idle:return 2f;
                case CharacterReaction.Surprise:return .3f;
                case CharacterReaction.Celebrate:return .65f;
                case CharacterReaction.Defeat:return .5f;
                default:return .45f;
            }
        }
        void Update()
        {
            elapsed+=Time.unscaledDeltaTime;
            float duration=Fast?.12f:Duration(Reaction);
            if(IsReacting&&elapsed>=duration){Reaction=queued;queued=CharacterReaction.Idle;elapsed=0;}
            float phase=elapsed/duration;
            if(Fast){Restore();return;}
            float envelope=Mathf.Sin(Mathf.Clamp01(phase)*Mathf.PI);
            float x=1,y=1,angle=0,lift=0;
            switch(Reaction)
            {
                case CharacterReaction.Idle:y+=.012f*Mathf.Sin(elapsed*Mathf.PI);break;
                case CharacterReaction.Talk:y+=.04f*Mathf.Sin(phase*6*Mathf.PI)*envelope;break;
                case CharacterReaction.Surprise:x-=.09f*envelope;y+=.12f*envelope;lift=8*envelope;break;
                case CharacterReaction.Panic:angle=5*Mathf.Sin(phase*12*Mathf.PI)*envelope;break;
                case CharacterReaction.Laugh:y-=.07f*Mathf.Abs(Mathf.Sin(phase*6*Mathf.PI))*envelope;angle=4*envelope;break;
                case CharacterReaction.Celebrate:lift=18*Mathf.Abs(Mathf.Sin(phase*2*Mathf.PI))*envelope;angle=7*Mathf.Sin(phase*4*Mathf.PI)*envelope;break;
                case CharacterReaction.Defeat:y-=.12f*envelope;angle=-9*envelope;break;
            }
            rect.localScale=Vector3.Scale(restScale,new Vector3(x,y,1));
            // Anchors can move when safe area/orientation changes; never restore an old local position.
            rect.anchoredPosition=restPosition+new Vector2(0,lift);
            rect.localRotation=restRotation*Quaternion.Euler(0,0,angle);
        }
        void Restore()
        {
            if(rect==null)return;
            rect.localScale=restScale;rect.anchoredPosition=restPosition;rect.localRotation=restRotation;
        }
        void OnDisable(){Restore();Reaction=queued=CharacterReaction.Idle;elapsed=0;}
    }
}
