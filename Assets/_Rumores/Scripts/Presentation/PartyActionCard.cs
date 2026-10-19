using System;
using Rumores.Core.Party;
using UnityEngine;
using UnityEngine.UI;

namespace Rumores.Presentation
{
    // Reusable prefab view. No simulation or scoring takes place in this component.
    [RequireComponent(typeof(RectTransform))]
    public sealed class PartyActionCard : MonoBehaviour
    {
        Button button;
        Image background;
        Text title,description;
        void Awake()
        {
            background=gameObject.AddComponent<Image>();
            button=gameObject.AddComponent<Button>();button.targetGraphic=background;
            title=Ui.Text(transform,"",new Vector2(.04f,.65f),new Vector2(.96f,.97f),25,Ui.Cream);
            description=Ui.Text(transform,"",new Vector2(.04f,.04f),new Vector2(.96f,.65f),20,Ui.Cream);
            title.supportRichText=false;description.supportRichText=false;
        }
        public void Bind(PartyAction action,Action choose)
        {
            gameObject.name="Action_"+action.id;title.text=action.title;
            description.text=Summary(action);
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>choose());SetSelected(false);
        }
        public void SetSelected(bool value)
        {background.color=value?Ui.Orange:Ui.Teal;title.fontStyle=value?FontStyle.Bold:FontStyle.Normal;}
        static string Summary(PartyAction a)
        {
            switch(a.kind)
            {
                case "fear":return "Miedoso\nVe amenazas; sube el pánico.";
                case "exaggerate":return "Exagerado\nAgranda las consecuencias.";
                case "lie":return "Mentiroso\nInventa un dato; pierde credibilidad.";
                case "misread":return "Ignorante\nConfunde al protagonista.";
                case "calm":return "Pide pruebas\nBaja el pánico; recupera credibilidad.";
                default:return "Corre la voz\nLlega a cuatro vecinos más.";
            }
        }
    }
}
