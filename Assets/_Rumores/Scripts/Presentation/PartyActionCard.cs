using System;
using Rumores.Core.Party;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Reusable prefab view. No simulation takes place in this component.
    [RequireComponent(typeof(RectTransform))]
    public sealed class PartyActionCard : MonoBehaviour
    {
        Button button;
        Image background;
        Text title,description;
        ActionIconGraphic icon;
        void Awake()
        {
            background=gameObject.AddComponent<Image>();
            button=gameObject.AddComponent<Button>();button.targetGraphic=background;
            icon=ActionIconGraphic.Create(transform,"mentir",new Vector2(.035f,.77f),new Vector2(.11f,.97f));
            title=Ui.Text(transform,"",new Vector2(.12f,.76f),new Vector2(.96f,.97f),25,Ui.Cream);
            description=Ui.Text(transform,"",new Vector2(.04f,.04f),new Vector2(.96f,.76f),24,Ui.Cream);
            title.supportRichText=false;description.supportRichText=false;
        }
        public void Bind(PartyAction action,string preview,Action choose)
        {
            gameObject.name="Action_"+action.id;title.text=action.title;icon.SetAction(action.id);
            description.text=preview;description.resizeTextForBestFit=true;description.resizeTextMinSize=20;description.resizeTextMaxSize=24;
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>choose());SetSelected(false);
        }
        public void SetSelected(bool value)
        {background.color=value?Ui.Orange:Ui.Teal;title.fontStyle=value?FontStyle.Bold:FontStyle.Normal;}
    }
}
