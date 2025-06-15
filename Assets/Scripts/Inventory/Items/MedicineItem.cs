using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MedicineItem", menuName = "Items/Medicine")]
public class MedicineItem : InventoryItem, IGiverPlayerParameters, IGiverOfEffects, IRemovableDiseases
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<GivesParameter> GivesParameters { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesEffects { get; private set; }
    [field: SerializeField] public List<ScriptableObject> RemoveDisease { get; private set; }

    private void OnEnable()
    {
        Category = Category.Medicine;
        UseType = MethodOfUse.OnSelf;
    }

    public override string ToString()
    {
        string s = "StatusParameterImpacts:\n";

        foreach (var pair in GivesParameters)
        {
            s += "\t" + pair.ParameterType.ToString() + ": " + pair.Value.ToString() + "\n";
        }

        return base.ToString() + s;
    }
}
