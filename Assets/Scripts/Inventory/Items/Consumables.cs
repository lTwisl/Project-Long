using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumables", menuName = "Items/Food and drink")]
public class Consumables : InventoryItem
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<PairParamterAndValue> StatusParameterImpacts { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesBonus { get; private set; }

    private void OnEnable()
    {
        Category = ItemType.FoodAndDrink;
        UseType = MethodOfUse.OnSelf;
    }
}
