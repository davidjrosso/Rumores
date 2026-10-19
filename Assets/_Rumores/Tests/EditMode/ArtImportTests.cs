using NUnit.Framework;
using Rumores.Presentation;
using UnityEditor;
using UnityEngine;

namespace Rumores.Tests
{
    public class ArtImportTests
    {
        [TestCase("Miedoso",1024,1536,0f)]
        [TestCase("Pueblo",1536,1024,.5f)]
        public void PreviewSpritesImportWithExpectedSizePivotAndFiltering(string name,int width,int height,float pivotY)
        {
            string path="Assets/_Rumores/Resources/Art/Preview/"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);Assert.NotNull(importer);
            Assert.AreEqual(TextureImporterType.Sprite,importer.textureType);
            Assert.AreEqual(SpriteImportMode.Single,importer.spriteImportMode);
            Assert.AreEqual(FilterMode.Bilinear,importer.filterMode);Assert.AreEqual(TextureWrapMode.Clamp,importer.wrapMode);
            Assert.IsFalse(importer.mipmapEnabled);Assert.IsTrue(importer.sRGBTexture);
            Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
            Assert.AreEqual(100,importer.spritePixelsPerUnit);
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);Assert.NotNull(sprite);
            Assert.AreEqual(width,sprite.rect.width);Assert.AreEqual(height,sprite.rect.height);
            Assert.AreEqual(new Vector2(width*.5f,height*pivotY),sprite.pivot);
            if(name=="Miedoso"){Assert.IsTrue(importer.alphaIsTransparency);Assert.IsTrue(importer.DoesSourceTextureHaveAlpha());}
        }
        [Test]
        public void ArtLibraryResolvesGeneratedSpritesAndLeavesUnproducedCharactersExplicit()
        {
            var art=Resources.Load<RumoresArt>("RumoresArt");Assert.NotNull(art);Assert.NotNull(art.town);
            Assert.NotNull(art.Find("miedoso"));
            Assert.IsNull(art.Find("exagerado"));Assert.IsNull(art.Find("mentiroso"));Assert.IsNull(art.Find("ignorante"));
            Assert.IsNull(art.Find("unknown"));
        }
    }
}
