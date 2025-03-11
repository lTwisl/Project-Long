using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MedicineItem", menuName = "Items/Medicine")]
public class MedicineItem : InventoryItem
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<PairParamterAndValue> StatusParameterImpacts { get; private set; }
    [field: SerializeField] public List<ScriptableObject> HealsDisease { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesBonus { get; private set; }

    private void OnEnable()
    {
        Category = Category.Medicine;
        UseType = MethodOfUse.OnSelf;
    }

    public override string ToString()
    {
        string s = "StatusParameterImpacts:\n";

        foreach (var pair in StatusParameterImpacts)
        {
            s += "\t" + pair.ParameterType.ToString() + ": " + pair.Value.ToString() + "\n";
        }

        return base.ToString() + s;
    }
}
