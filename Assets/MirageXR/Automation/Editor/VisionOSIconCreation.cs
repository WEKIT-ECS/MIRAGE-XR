using System;
using System.IO;
using UnityEditor;

// Workaround for adding icons on visionos not working.
// The workaround can be found here: https://discussions.unity.com/t/script-for-adding-app-icon-to-xcode-project/315132

namespace MirageXR
{
    public static class VisionOSIconCreation {
        [Serializable] class XCodeInfo {
            public int version;
            public string author;
        }

        [Serializable] class XCodeImage {
            public string idiom;
            public string filename;
            public string scale;
        }

        [Serializable] class XCodeImageSet {
            public XCodeInfo info;
            public XCodeImage[] images;
        }

        [Serializable] class XCodeImageStackLayer {
            public string filename;
        }

        [Serializable] class XCodeImageStack {
            public XCodeInfo info;
            public XCodeImageStackLayer[] layers;
        }
        
        [Serializable] class XCodeContents {
            public XCodeInfo info;
        }

        public static void SetupVisionOSIcon(
            string buildPath,
            int version,
            string author,
            params Tuple<string, string>[] layers)
        {
            var assetPath = $"{buildPath}/Unity-VisionOS/Images.xcassets/";

            // Delete auto-generated blank App Icon
            var oldIconName = "AppIcon.appiconset";
            var iconPath = $"{assetPath}{oldIconName}/";
            if (Directory.Exists(iconPath)) Directory.Delete(iconPath, true);

            // Create ImageStack
            var newIconName = "AppIcon.solidimagestack";
            var imgStackPath = $"{assetPath}{newIconName}/";
            if (Directory.Exists(imgStackPath)) Directory.Delete(imgStackPath, true);
            Directory.CreateDirectory(imgStackPath);

            var imageStack = new XCodeImageStack {
                info = new XCodeInfo { version = version, author = author },
                layers = new XCodeImageStackLayer[layers.Length]
            };

            // Loop through layers
            for (var i = 0; i < layers.Length; ++i) {
                // Create ImageStackLayer
                var imgStackLayerFileName = $"{layers[i].Item1}.solidimagestacklayer";
                var imgStackLayerPath = $"{imgStackPath}{imgStackLayerFileName}/";
                Directory.CreateDirectory(imgStackLayerPath);
                
                // Add layer to ImageStack layers
                imageStack.layers[i] = new XCodeImageStackLayer {
                    filename = imgStackLayerFileName
                };

                // Create Contents.json for ImageStackLayer
                var imgStackLayer = new XCodeContents {
                    info = new XCodeInfo { version = version, author = author }
                };
                
                File.WriteAllText(
                    imgStackLayerPath + "Contents.json",
                    EditorJsonUtility.ToJson(imgStackLayer, prettyPrint: true)
                );
                
                // Create ImageSet for layer
                var filePath = AssetDatabase.GUIDToAssetPath(layers[i].Item2);
                var fileName = Path.GetFileName(filePath);
                
                var imgSetPath = $"{imgStackLayerPath}Content.imageset/";
                Directory.CreateDirectory(imgSetPath);
                
                // Copy Icon into ImageSet
                File.Copy(filePath, imgSetPath + fileName);
                
                var imageSet = new XCodeImageSet {
                    info = new XCodeInfo { version = version, author = author },
                    images = new [] {
                        new XCodeImage {
                            idiom = "vision",
                            filename = fileName,
                            scale = fileName.Contains("@2x") ? "2x" : "1x",
                        }
                    }
                };
                
                // Create Contents.json for ImageSet
                File.WriteAllText(
                    imgSetPath + "Contents.json",
                    EditorJsonUtility.ToJson(imageSet, prettyPrint: true)
                );
            }

            // Create Contents.json for ImageStack
            File.WriteAllText(
                imgStackPath + "Contents.json",
                EditorJsonUtility.ToJson(imageStack, prettyPrint: true)
            );
        }
    }
}