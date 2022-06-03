using Microsoft.MixedReality.Toolkit.UI;
using System.IO;
using UnityEngine;

/// <summary>
/// @bibeg
/// editor for 3D model based annotations.
/// Inherits from MirageXRAnnotationBaseEditor.
/// </summary>

public class MirageXRModelAnnotationEditor : MonoBehaviour
{
    /// <summary>
    /// Gameobject holder for the model to be loaded from the resources
    /// </summary>
    GameObject placeableModel;

    private string selectedModel = "";
    private static string MODEL_HUMAN = "HumanAnatomy/Anatomy";
    private static string MODEL_PLANE = "Aeroplane/Plane";

    bool large = false;


    // List<string> modelsCollection = new List<string>();
    // /// <summary>
    // /// Automatically genertate menu item by reading the names of the objects in resource folder
    // /// </summary>
    // void GenerateFileList()
    // {

    //    string myPath = "Assets/Resources/Medieval/Models";
    //    DirectoryInfo dir = new DirectoryInfo(myPath);
    //    FileInfo[] fileInfo = dir.GetFiles("*.*");
    //    foreach (FileInfo file in fileInfo)
    //    {
    //        if (file.Extension == ".FBX" || file.Extension == ".prefab")
    //        {
    //            string tempName = file.Name;
    //            string extension = file.Extension;
    //            string strippedName = tempName.Replace(extension, "");
    //            modelsCollection.Add(strippedName);
    //        }
    //    }
    //}

    // void populateMenuItem()
    // {
    //     for (int i=0; i>=modelsCollection.Count;i++)
    //     {
    //         gameObject.GetComponent<MirageXRMenuBase>().menuItemTexts[i] = modelsCollection[i];
    //         gameObject.GetComponent<MirageXRMenuBase>().menuItemNames[i]= "PlaceModel";
    //     }
       
    // }


    /// <summary>
    /// method to load a prefab.
    /// </summary>
    public void PlacePlane()
    {
        Place3DModel(MODEL_PLANE, true);
    }
    /// <summary>
    /// loads the human Prefab
    /// </summary>
    public void PlaceHuman()
    {
        Place3DModel(MODEL_HUMAN, false);
    }

    public void Place3DModel(string modelResourceLocator, bool isLarge)
    {
        Debug.Log("3d model Annotation changed: " + modelResourceLocator);
        selectedModel = modelResourceLocator;
        DestroyChild();
        large = isLarge;
        placeableModel = Instantiate(Resources.Load(modelResourceLocator, typeof(GameObject))) as GameObject;
        placeableModel.transform.localPosition = Vector3.zero;
        //Debug.Log(placeableModel.transform.localPosition);
        RenderModel();
    }

    void RenderModel()
    {

        //assign the parent mesh from the resouce to the parent of this object
        //gameObject.GetComponent<MeshFilter>().mesh = placeableModel.GetComponent<MeshFilter>().mesh;

        placeableModel.transform.SetParent(gameObject.transform, false);

        if (large == true)
        {
            gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            large = false;
        }
        else
        {
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        //gameObject.AddComponent<MirageXRModelExpander>();
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// destroy any child object that is not directly parent to the gameobject
    /// </summary>
    void DestroyChild()
    {
        foreach (Transform T in gameObject.GetComponentsInChildren<Transform>())
        {
            //foreach (Renderer rend in T.GetComponentsInChildren<Renderer>()){
            //     rend.enabled = false;
            // }
            if (T.parent == gameObject.transform)
            {
                GameObject.Destroy(T.gameObject);
            }

        }
    }

    /// <summary>
    /// method to call the expand model 
    /// </summary>

    public void ExpandModel()
    {
        Debug.Log("3d model expanded");
        gameObject.GetComponentInChildren<MirageXRModelExpander>().SendMessage("ExpandModel", SendMessageOptions.DontRequireReceiver);
    }

    public void CompressModel()
    {
        Debug.Log("3d model compressed");
        gameObject.GetComponentInChildren<MirageXRModelExpander>().SendMessage("CompressModel", SendMessageOptions.DontRequireReceiver);
    }
   
    public void RotateModel()
    {
        Destroy(gameObject.GetComponent<ObjectManipulator>());
        // gameObject.GetComponent<HandDraggable>().IsDraggingEnabled = false
        // gameObject.GetComponent<MirageXRModelManipulation>().rotatingEnabled = true;
        gameObject.AddComponent<MirageXRModelManipulation>();
    }

    public void Save(string filename)
    {
        // Write the data in an .xml file and save it locally.
        FileStream file = File.Create(filename);
        TextWriter tW = new StreamWriter(file);

        tW.WriteLine(large);
        tW.WriteLine(selectedModel);
        tW.Flush();
        //tW.Close();

        Debug.Log("Saved");
    }

    public void Load(string filename)
    {
        FileStream file = File.Open(filename, FileMode.Open);
        TextReader tR = new StreamReader(file);
        string largeS = tR.ReadLine();
        bool isLarge = bool.Parse(largeS);
        string text = tR.ReadLine();

        Place3DModel(text, isLarge);

        Debug.Log("Loaded");
    }
}
