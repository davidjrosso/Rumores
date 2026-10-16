using System;
using Rumores.Core;
using UnityEngine;

namespace Rumores.Presentation
{
    [CreateAssetMenu(menuName="Rumores/Catálogo",fileName="RumoresCatalog")]
    public sealed class RumoresCatalog : ScriptableObject
    {
        [SerializeField] TextAsset content;
        public Catalog Load()
        {
            if(content==null) throw new InvalidOperationException("Falta Content.json en el catálogo.");
            var catalog=JsonUtility.FromJson<Catalog>(content.text);
            var errors=CatalogValidator.Validate(catalog);
            if(errors.Count>0) throw new InvalidOperationException(string.Join("\n",errors));
            return catalog;
        }
    }
}
