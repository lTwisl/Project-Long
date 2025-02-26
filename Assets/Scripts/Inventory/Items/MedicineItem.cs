using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MedicineItem", menuName = "Items/Medicine")]
public class MedicineItem : InventoryItem
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public List<PairParamterAndValue> PairsParamterAndValue { get; private set; }
    [field: SerializeField] public List<ScriptableObject> HealsDisease { get; private set; }
    [field: SerializeField] public List<ScriptableObject> GivesBonus { get; private set; }

    private void OnEnable()
    {
        Category = ItemType.Medicine;
        UseType = MethodOfUse.OnSelf;
    }
}
