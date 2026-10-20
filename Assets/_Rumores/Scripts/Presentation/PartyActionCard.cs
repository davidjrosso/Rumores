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
        Text title,description,selection,explanation;
        ActionIconGraphic icon;
        void Awake()
        {
            background=gameObject.AddComponent<Image>();
            button=gameObject.AddComponent<Button>();button.targetGraphic=background;
            PartyLayout.Stack((RectTransform)transform,16,10);
            var header=Ui.Rect(transform,"Header",Vector2.zero,Vector2.one);header.gameObject.AddComponent<LayoutElement>().preferredHeight=88;
            icon=ActionIconGraphic.Create(header,"mentir",new Vector2(0,.18f),new Vector2(.10f,.82f));
            title=Ui.Text(header,"",new Vector2(.12f,0),new Vector2(.74f,1),30,Ui.Cream);
            selection=Ui.Text(header,"",new Vector2(.75f,0),Vector2.one,26,Ui.Cream);
            explanation=PartyLayout.Paragraph(transform,"",28,Ui.Cream);
            description=PartyLayout.Paragraph(transform,"",32,Ui.Cream);
            title.supportRichText=false;description.supportRichText=false;
        }
        public void Bind(PartyAction action,string preview,Action choose)
        {
            gameObject.name="Action_"+action.id;title.text=action.title;icon.SetAction(action.id);
            explanation.text=action.description;description.text=preview;
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>choose());SetSelected(false);
        }
        public void SetSelected(bool value)
        {background.color=value?PartyLayout.Accent:Ui.Teal;title.fontStyle=value?FontStyle.Bold:FontStyle.Normal;selection.text=value?UiStrings.T("card.selected"):"";}
    }
}
