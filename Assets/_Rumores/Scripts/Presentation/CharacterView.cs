using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // The wrapper stays independent of either the generated sprite or the temporary face.
    public sealed class CharacterView : MonoBehaviour
    {
        public CharacterMotion Motion {get;private set;}
        public static CharacterView Create(Transform parent,RumoresArt art,string id,Vector2 min,Vector2 max,bool compact,bool fast)
        {
            var frame=Ui.Rect(parent,"Character",min,max);
            var view=frame.gameObject.AddComponent<CharacterView>();
            var visual=Ui.Rect(frame,"CharacterMotion",Vector2.zero,Vector2.one);
            var sprite=art!=null?art.Find(id):null;
            if(sprite!=null)
            {
                // Crop the displayed portrait, keeping the original PNG and its alpha intact.
                if(compact)frame.gameObject.AddComponent<RectMask2D>();
                var image=Ui.Panel(visual,"Avatar",Vector2.zero,Vector2.one,Color.white);
                image.sprite=sprite;image.preserveAspect=true;
                if(compact)
                {
                    var rect=image.rectTransform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;
                    rect.pivot=new Vector2(.5f,1);rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                    var aspect=image.gameObject.AddComponent<AspectRatioFitter>();
                    aspect.aspectMode=AspectRatioFitter.AspectMode.WidthControlsHeight;
                    aspect.aspectRatio=sprite.rect.width/sprite.rect.height;
                }
            }
            else Ui.Avatar(visual,id,Vector2.zero,Vector2.one);
            view.Motion=visual.gameObject.AddComponent<CharacterMotion>();view.Motion.Fast=fast;
            return view;
        }
    }
}
