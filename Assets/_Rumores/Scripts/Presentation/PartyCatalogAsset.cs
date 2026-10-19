using System;
using Rumores.Core.Party;
using UnityEngine;

namespace Rumores.Presentation
{
    [CreateAssetMenu(menuName="Rumores/Catálogo party",fileName="PartyCatalog")]
    public sealed class PartyCatalogAsset : ScriptableObject
    {
        [SerializeField] TextAsset content;
        public PartyCatalog Load()
        {
            if(content==null)throw new InvalidOperationException("Falta PartyContent.json en PartyCatalog.");
            var catalog=JsonUtility.FromJson<PartyCatalog>(content.text);
            if(catalog==null)throw new InvalidOperationException("El catálogo party está vacío.");
            var errors=catalog.Validate();
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
            return catalog;
        }
    }
}
