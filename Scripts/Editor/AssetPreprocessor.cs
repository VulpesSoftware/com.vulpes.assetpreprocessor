using UnityEditor;
using UnityEngine;

namespace Vulpes.Editor.AssetPreprocessor
{
    public sealed class AssetPreprocessor : AssetPostprocessor
    {
        private const string AUDIO_AMBIANCE = "AMB_";
        private const string AUDIO_MUSIC = "MUS_";
        private const string TEXTURE_COLOR = "_c"; // Invalid suffix, we use this to point that out.
        private const string TEXTURE_DIFFUSE = "_d";
        private const string TEXTURE_NORMAL = "_n";
        private const string TEXTURE_SPECULAR = "_s";
        private const string TEXTURE_EMISSION = "_e";
        private const string TEXTURE_SPRITE = "_ui";
        private const string ASSETS = "Assets";

        private void OnPreprocessAudio()
        {
            AudioImporter audioImport = (AudioImporter)assetImporter;
            string[] path = assetPath.Split('/');
            string file = path[path.Length - 1].Split('.')[0];
            // Don't do anything for assets not inside the Assets folder (i.e. Those included with Packages).
            if (path[0] != ASSETS)
            {
                return;
            }
            // At the moment Music and Ambiance are the only things that allow stereo.
            if (file.StartsWith(AUDIO_MUSIC) || file.StartsWith(AUDIO_AMBIANCE))
            {
                audioImport.forceToMono = false;
            } else
            {
                audioImport.forceToMono = true;
            }
        }

        private void OnPreprocessModel()
        {
            ModelImporter modelImport = (ModelImporter)assetImporter;
            string[] path = assetPath.Split('/');
            // Don't do anything for assets not inside the Assets folder (i.e. Those included with Packages).
            if (path[0] != ASSETS)
            {
                return;
            }
            // Models are easy, we just want to turn off material and animation import.
            modelImport.materialImportMode = ModelImporterMaterialImportMode.None;
            modelImport.importAnimation = false;
        }

        private void OnPreprocessTexture()
        {
            TextureImporter textureImport = (TextureImporter)assetImporter;
            string[] path = assetPath.Split('/');
            string file = path[path.Length - 1].Split('.')[0];
            // Don't do anything for assets not inside the Assets folder (i.e. Those included with Packages).
            if (path[0] != ASSETS)
            {
                return;
            }
            if (file.EndsWith(TEXTURE_DIFFUSE) || file.EndsWith(TEXTURE_SPECULAR) || file.EndsWith(TEXTURE_EMISSION)) // Diffuse, Specular and Emission can for the most part be treated the same way.
            {
                textureImport.textureType = TextureImporterType.Default;
                textureImport.filterMode = FilterMode.Bilinear;
                textureImport.textureCompression = TextureImporterCompression.Uncompressed;
                // Only disable sRGBTexture if we're in Linear Color Space.
                textureImport.sRGBTexture = (PlayerSettings.colorSpace != ColorSpace.Linear);
                textureImport.alphaIsTransparency = file.EndsWith(TEXTURE_SPECULAR) ? false : true;
            } else if (file.EndsWith(TEXTURE_NORMAL)) // Normal Maps need to be configured separately.
            {
                textureImport.textureType = TextureImporterType.NormalMap;
                textureImport.filterMode = FilterMode.Bilinear;
                textureImport.textureCompression = TextureImporterCompression.Uncompressed;
            } else if (file.EndsWith(TEXTURE_COLOR)) // Catch for old '_c' diffuse map suffix.
            {
                Debug.LogWarning(string.Format("File suffix '{0}' is not supported, please use '{1}' for diffuse/color maps instead.", TEXTURE_COLOR, TEXTURE_DIFFUSE));
            } else if (file.EndsWith(TEXTURE_SPRITE)) // Sprites now use '_ui' suffix, anything that doesn't match a rule is left alone.
            {
                textureImport.textureType = TextureImporterType.Sprite;
                textureImport.filterMode = FilterMode.Trilinear;
                textureImport.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }
}