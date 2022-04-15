using System.Xml;
using UnityEditor.Android;

public class AndroidManifestPostprocessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 3;

    void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
    {
        OnPostGenerateGradleAndroidProject(path);
    }
    
    static readonly string k_AndroidManifestPath = "/src/main/AndroidManifest.xml";
    static readonly string k_AndroidURI = "http://schemas.android.com/apk/res/android";
    
    private static void OnPostGenerateGradleAndroidProject(string path)
    {
        var manifestPath = path + k_AndroidManifestPath;
        var manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        var manifestNode = FindFirstChild(manifestDoc, "manifest");
        if (manifestNode == null)
            return;

        var applicationNode = FindFirstChild(manifestNode, "application");
        if (applicationNode == null)
            return;
        
        FindOrCreateTagWithAttributes(manifestDoc, applicationNode, "meta-data", "name", "unityplayer.SkipPermissionsDialog", "value", "false");

        manifestDoc.Save(manifestPath);
    }
    
    private static XmlNode FindFirstChild(XmlNode node, string tag)
    {
        if (node.HasChildNodes)
        {
            for (int i = 0; i < node.ChildNodes.Count; ++i)
            {
                var child = node.ChildNodes[i];
                if (child.Name == tag)
                    return child;
            }
        }

        return null;
    }
    
    private static void FindOrCreateTagWithAttributes(XmlDocument doc, XmlNode containingNode, string tagName,
        string firstAttributeName, string firstAttributeValue, string secondAttributeName, string secondAttributeValue)
    {
        if (containingNode.HasChildNodes)
        {
            for (int i = 0; i < containingNode.ChildNodes.Count; ++i)
            {
                var childNode = containingNode.ChildNodes[i];
                if (childNode.Name == tagName)
                {
                    if (childNode is XmlElement childElement && childElement.HasAttributes)
                    {
                        var firstAttribute = childElement.GetAttributeNode(firstAttributeName, k_AndroidURI);
                        if (firstAttribute == null || firstAttribute.Value != firstAttributeValue)
                            continue;

                        var secondAttribute = childElement.GetAttributeNode(secondAttributeName, k_AndroidURI);
                        if (secondAttribute != null)
                        {
                            secondAttribute.Value = secondAttributeValue;
                            return;
                        }

                        // Create it
                        AppendNewAttribute(doc, childElement, secondAttributeName, secondAttributeValue);
                        return;
                    }
                }
            }
        }

        // Didn't find it, so create it
        var element = doc.CreateElement(tagName);
        AppendNewAttribute(doc, element, firstAttributeName, firstAttributeValue);
        AppendNewAttribute(doc, element, secondAttributeName, secondAttributeValue);
        containingNode.AppendChild(element);
    }
    
    private static void AppendNewAttribute(XmlDocument doc, XmlElement element, string attributeName, string attributeValue)
    {
        var attribute = doc.CreateAttribute(attributeName, k_AndroidURI);
        attribute.Value = attributeValue;
        element.Attributes.Append(attribute);
    }
}