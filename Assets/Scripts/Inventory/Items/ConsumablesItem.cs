using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumablesItem", menuName = "Items/Food and drink")]
public class ConsumablesItem : InventoryItem, ReplenishingPlayerParameters, GiverOfBonuses
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<PairParamterAndValue> ReplenishmentParameters { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesBonus { get; private set; }

    private void OnEnable()
    {
        Category = Category.FoodAndDrink;
        UseType = MethodOfUse.OnSelf;
    }

    public override string ToString()
    {
        string s = "StatusParameterImpacts:\n";

        foreach (var pair in ReplenishmentParameters)
        {
            s += "\t" + pair.ParameterType.ToString() + ": " + pair.Value.ToString() + "\n";
        }

        return base.ToString() + s;
    }
}
