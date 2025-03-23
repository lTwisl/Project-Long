using UnityEngine;

[CreateAssetMenu(fileName = "UseOnSelfStrategy", menuName = "Scriptable Objects/UseOnSelfStrategy")]
public class UseOnSelfStrategy : UseStrategy
{
    public override void Execute(InventoryItem item)
    {
        if (item is ConsumablesItem consumables)
        {
            foreach (var parameter in consumables.StatusParameterImpacts)
                _player.GetComponent<PlayerStatusController>().ModifyParameter(parameter.ParameterType, parameter.Value);

            Debug.Log($"Use item (ConsumablesItem) {consumables.Name} on self");
            return;
        }

        if (item is MedicineItem medicine)
        {
            foreach (var parameter in medicine.StatusParameterImpacts)
                _player.GetComponent<PlayerStatusController>().ModifyParameter(parameter.ParameterType, parameter.Value);

            Debug.Log($"Use item (MedicineItem) {item.Name} on self");
            return;
        }
    }
}
