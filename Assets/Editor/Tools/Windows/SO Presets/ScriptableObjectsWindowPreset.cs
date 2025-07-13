using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO Window Preset", menuName = "Scriptable Objects/SO Window Preset")]
public class ScriptableObjectsWindowPreset : ScriptableObject
{
    [field: SerializeField, TextArea(3, 50)] public string Description { get; private set; } = "What is a Scriptable Objects contains in this Preset?";
    [field: SerializeField] public List<ScriptableObject> ScriptableObjects { get; private set; } = new();

    public int GetSoCount => ScriptableObjects.Count;
}