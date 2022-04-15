using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TiltBrush;
using UnityEngine;
using static TiltBrush.SketchMemoryScript;

public class InstantiateTiltbrush : MonoBehaviour
{
    public GameObject TiltbrushPrefab;
    public bool instantiateOnStart = false;

    private SimpleSnapshot snapshot;

    public bool clearScene;
    public bool createSnapshot;
    public bool play;
    public bool pause;

    public bool import;
    public bool export;
    public string exportPath = "./Test/export.tilt";

    public bool viewonly;

    private Tiltbrush tiltbrush;
    public GameObject playspace;
    public Camera mainCamera;

    void Update(){
        if(clearScene){
            clearScene = false;
            tiltbrush.ClearScene();
        }

        if(createSnapshot){
            createSnapshot = false;
            snapshot = tiltbrush.GetNewSnapshot();
        }

        if(play){
            play = false;
            if(snapshot == null)
                snapshot = tiltbrush.GetNewSnapshot();

            snapshot.Play();
        }

        if(pause){
            pause = false;
            snapshot.Pause();
        }

        if(export){
            export = false;
            tiltbrush.ExportFile(exportPath);
        }

        if(import){
            import = false;
            snapshot = tiltbrush.ImportSnapshotFromFile(exportPath);
        }

        if(viewonly){
            viewonly = false;
            tiltbrush.SetViewOnly(true);
        }
    }

    private void Start()
    {
        if (instantiateOnStart)
            Instantiate();
    }

    [ContextMenu("Intantiate")]
    public void Instantiate()
    {
        tiltbrush = Instantiate(TiltbrushPrefab, transform).GetComponentInChildren<Tiltbrush>();
    }

    [ContextMenu("Destroy")]
    public void Destroy()
    {
        tiltbrush.Destroy();
        tiltbrush = null;
    }
}
