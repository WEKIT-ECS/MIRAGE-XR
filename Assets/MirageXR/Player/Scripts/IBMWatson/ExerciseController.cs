using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExerciseController : MonoBehaviour
{
    //list of exercises; we remove them bit by bit
    public List<string> Exercises;

    public void Reset()
    {
        Exercises.Clear();
        Exercises.Add("A1");
        Exercises.Add("A14");
        Exercises.Add("A15");
        Exercises.Add("A21");
        Exercises.Add("B");
        Exercises.Add("B11");
        Exercises.Add("B12");
        Exercises.Add("B13");
        Exercises.Add("B14STS");
        Exercises.Add("B22");
        Exercises.Add("B32");
        Exercises.Add("B33");
        Exercises.Add("B41");
        Exercises.Add("B41Y");
        Exercises.Add("B42");
        Exercises.Add("B51");
        Exercises.Add("B51");
        Exercises.Add("B52");
        Exercises.Add("B61");
        Exercises.Add("B62");
        Exercises.Add("B72");
        Exercises.Add("B82");
        Exercises.Add("B83");
        Exercises.Add("B86");
        Exercises.Add("B1230");
        Exercises.Add("B4260");
        Exercises.Add("B7230");
        Exercises.Add("B8330");
        Exercises.Add("B8630");
        Exercises.Add("B62120");
        Exercises.Add("Plank");
        Exercises.Add("PushUp");
        Exercises.Add("Upright");
    }

    public void Start()
    {
        Reset();
    }
    public void RemoveExercise(string what)
    {
        if (Exercises.Contains(what)) Exercises.Remove(what);
    }
    }


