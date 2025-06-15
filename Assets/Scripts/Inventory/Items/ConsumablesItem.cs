using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumablesItem", menuName = "Items/Food and drink")]
public class ConsumablesItem : InventoryItem, IGiverPlayerParameters, IGiverOfEffects
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<GivesParameter> GivesParameters { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesEffects { get; private set; }

    private void OnEnable()
    {
        Category = Category.FoodAndDrink;
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
