using UnityEngine;

namespace Rumores.Presentation
{
    // Small camera-like drift of the existing illustration; stopped in reduced-motion mode.
    public sealed class TownAmbience : MonoBehaviour
    {
        public bool Reduced;
        float elapsed;
        void Update()
        {
            elapsed+=Time.unscaledDeltaTime;
            transform.localScale=Vector3.one*(Reduced?1:1.015f+.01f*Mathf.Sin(elapsed*.5f));
            transform.localRotation=Reduced?Quaternion.identity:Quaternion.Euler(0,0,.25f*Mathf.Sin(elapsed*.4f));
        }
        void OnDisable(){transform.localScale=Vector3.one;transform.localRotation=Quaternion.identity;}
    }
}
