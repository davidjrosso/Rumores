using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    [CreateAssetMenu(menuName="Rumores/Art library")]
    public sealed class RumoresArt : ScriptableObject
    {
        [Serializable] public sealed class Character
        {
            public string id;
            public Sprite sprite;
        }
        public Sprite town;
        public Character[] characters=Array.Empty<Character>();
        public Sprite Find(string id)
        {
            foreach(var character in characters)
                if(character!=null&&character.id==id)return character.sprite;
            return null;
        }
        public void Town(Transform parent)
        {
            if(town==null){Ui.Town(parent);return;}
            var image=Ui.Panel(parent,"TownIllustration",Vector2.zero,Vector2.one,Color.white);
            image.sprite=town;image.preserveAspect=true;
        }
    }

}
