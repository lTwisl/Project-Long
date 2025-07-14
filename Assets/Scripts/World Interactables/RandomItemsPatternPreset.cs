using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RandomItemsPatternPreset", menuName = "RandomItemsPatternPreset")]
public class RandomItemsPatternPreset : ScriptableObject
{
    [SerializeField, TextArea] private string _description;

    [field: SerializeField, Min(0)] public int MaxCountItems { get; private set; }

    [SerializeField] private InitializerListRandSlots _initializerRandSlotLists = new();

    public IReadOnlyList<RandItem> GetRandItems() => _initializerRandSlotLists.Items;
}
