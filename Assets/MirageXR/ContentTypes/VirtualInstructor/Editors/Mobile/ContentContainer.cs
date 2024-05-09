using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentContainer : MonoBehaviour
{
    public enum ObjectDataFunctionEnum
    {
        DemoDataWithName,
        DemoDataWithNameAndDescription,
        VoiceEndpoint
        // todo add the name of the serves methode here
    }
    
    public ObjectDataFunctionEnum selectedFunctionEnum;
    private List<ObjectData> objectDataSet;
    public GameObject prefabTemplate;
    public Transform sceneContainer;
    public GameObject audioPlayer;

    void Start()
    {
        switch (selectedFunctionEnum)
        {
            case ObjectDataFunctionEnum.DemoDataWithName:
                objectDataSet = DemoWithOne();
                break;
            case ObjectDataFunctionEnum.DemoDataWithNameAndDescription:
                objectDataSet = DemoWithTow();
                break;
            // todo add the name of the serves methode here
        }

        if (selectedFunctionEnum == ObjectDataFunctionEnum.VoiceEndpoint)
        {
            objectDataSet = DemoWithTow();
            foreach (ObjectData objectData in objectDataSet)
            {
                InstantiateObjectDataWithTowButton(objectData);
            }
        }
        else
        {
            foreach (ObjectData objectData in objectDataSet)
            {
                InstantiateObjectDataWithOneButton(objectData);
            } 
        }
        
    }

    private void InstantiateObjectDataWithTowButton(ObjectData objectData)
    {
        UnityEngine.Debug.LogWarning("InstantiateObjectDataWithTowButton");
        GameObject instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
        TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();

        if (textComponents.Length == 2)
        {
            textComponents[1].text = objectData.Name;
            textComponents[0].text = objectData.Description;
        }
        else
        {
            UnityEngine.Debug.LogError("Wrong Prefab in ContentContainer");
        }
        
        Button[] buttons = instantiatedObject.GetComponentsInChildren<Button>();
        if (buttons[0] != null && buttons[1] != null)
        {
            buttons[0].onClick.AddListener(() => OpenAudioPrefab(objectData));
            buttons[1].onClick.AddListener(() =>  OnPrefabClicked(objectData));
        }
        else
        {
            UnityEngine.Debug.LogError("Button-Component one is missing in Prefab");
        }
    }

    private void OpenAudioPrefab(ObjectData objectData)
    {
        audioPlayer.SetActive(true);
        var tmpTextComponent = audioPlayer.GetComponentInChildren<TMP_Text>();
        if (tmpTextComponent != null)
        {
            tmpTextComponent.text = objectData.Name;
        }
        else
        {
            UnityEngine.Debug.LogError("TMP_Text component is missing in audioPlayer");
        }
    }

    private void InstantiateObjectDataWithOneButton(ObjectData objectData)
    {
        GameObject instantiatedObject = Instantiate(prefabTemplate, sceneContainer);
        TMP_Text[] textComponents = instantiatedObject.GetComponentsInChildren<TMP_Text>();

        if (textComponents.Length == 2)
        {
            textComponents[1].text = objectData.Name;
            textComponents[0].text = objectData.Description;
        }
        else if (textComponents.Length == 1)
        {
            textComponents[0].text = objectData.Name;
        }
        else
        {
            UnityEngine.Debug.LogError("Wrong Prefab in ContentContainer");
        }
        
        Button button = instantiatedObject.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnPrefabClicked(objectData));
        }
        else
        {
            UnityEngine.Debug.LogError("Button-Component is missing in Prefab");
        }
        
    }
    
    public void OnPrefabClicked(ObjectData objectData)
    {
        // Hier kannst du definieren, was passieren soll, wenn auf das Prefab geklickt wird
        UnityEngine.Debug.Log("You  klickt: " + objectData.Name);
        // Führe weitere Aktionen aus, basierend auf den Daten von objectData
    }
    

    private List<ObjectData> DemoWithOne()
    {
        return CreateDemo(false, 5); 
    }
    
    private List<ObjectData> DemoWithTow()
    {
        return CreateDemo(true, 5); 
    }
    
    private List<ObjectData> CreateDemo(bool b, int i)
    {
        List<ObjectData> temp = new List<ObjectData>();
        if (b)
        {
            for (int j = 0; j < i; j++)
            {
                ObjectData t = new ObjectData("Name" + j, "Lorem ipus"+j);
                temp.Add(t);
            }
            
        }
        else
        {
            for (int j = 0; j < i; j++)
            {
                ObjectData t = new ObjectData("Name" + j);
                temp.Add(t);
            }
            
        }

        return temp;
    }
}

// Demo todo remove when the AI backend is merged in. 
[System.Serializable]
public class ObjectData
{
    public string EndpointName { get; }
    public string Name { get; }
    public string? Description { get; }
    public string? ApiName { get; }

    public ObjectData(string name)
    {
        Name = name;
        EndpointName = null;
        Description = null;
        ApiName = null;
    }

    public ObjectData(string name, string description)
    {
        Name = name;
        EndpointName = null;
        Description = description;
        ApiName = null;
    }
}