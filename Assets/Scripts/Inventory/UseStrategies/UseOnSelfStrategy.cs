using UnityEngine;

[CreateAssetMenu(fileName = "UseOnSelfStrategy", menuName = "Use Item Strategy/UseOnSelfStrategy")]
public class UseOnSelfStrategy : UseStrategy
{
    public override void Execute(InventoryItem item)
    {
        if (item is ConsumablesItem consumables)
        {
            foreach (var parameter in consumables.StatusParameterImpacts)
            {
                if (parameter.ParameterType == ParameterType.Capacity)
                {
                    Debug.LogWarning($"Предмет {item.Name} пытается начислить ParameterType.Capacity");
                    continue;
                }

                _player.GetComponent<PlayerStatusManager>().AdjustParameter(parameter.ParameterType, parameter.Value);
            }

            Debug.Log($"Use item (ConsumablesItem) {consumables.Name} on self");
            return;
        }

        if (item is MedicineItem medicine)
        {
            foreach (var parameter in medicine.StatusParameterImpacts)
            {
                if (parameter.ParameterType == ParameterType.Capacity)
                {
                    Debug.LogWarning($"Предмет {item.Name} пытается начислить ParameterType.Capacity");
                    continue;
                }

                _player.GetComponent<PlayerStatusManager>().AdjustParameter(parameter.ParameterType, parameter.Value);
            }

            Debug.Log($"Use item (MedicineItem) {item.Name} on self");
            return;
        }
    }
}
