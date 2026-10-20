using NUnit.Framework;
using Rumores.Presentation;
using UnityEditor;
using UnityEngine;

namespace Rumores.Tests
{
    public class ArtImportTests
    {
        [Test]
        public void AppIconImportsAndIsAssignedAsDefault()
        {
            const string path="Assets/_Rumores/Art/AppIcon.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.NotNull(importer);Assert.AreEqual(TextureImporterType.Default,importer.textureType);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.NotNull(icon);Assert.AreEqual(icon.width,icon.height);Assert.LessOrEqual(icon.width,1024);
            CollectionAssert.Contains(PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown),icon);
        }
        [TestCase("Miedoso",1024,1536,0f)]
        [TestCase("Exagerado",1024,1536,0f)]
        [TestCase("Mentiroso",1024,1536,0f)]
        [TestCase("Ignorante",1024,1536,0f)]
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
            if(name!="Pueblo"){Assert.IsTrue(importer.alphaIsTransparency);Assert.IsTrue(importer.DoesSourceTextureHaveAlpha());}
        }
        [Test]
        public void ArtLibraryResolvesFourDistinctCharacters()
        {
            var art=Resources.Load<RumoresArt>("RumoresArt");Assert.NotNull(art);Assert.NotNull(art.town);
            var sprites=new System.Collections.Generic.HashSet<Sprite>();
            foreach(var id in new[]{"miedoso","exagerado","mentiroso","ignorante"})
            {var sprite=art.Find(id);Assert.NotNull(sprite,id);Assert.IsTrue(sprites.Add(sprite),"Sprite repetido: "+id);}
            Assert.IsNull(art.Find("unknown"));
        }
    }
}
