using UnityEngine;
using Zenject;

public class ImpactZone : MonoBehaviour
{
    [SerializeField] private ParameterType _parameterType;
    [SerializeField] private float _impactValue;

    [Space(10)]
    [SerializeField] private float _addWetValue;
    [SerializeField] private float _addConditionValue;

    [Inject] private Player _player;
    [Inject] private PlayerParameters _parameters;

    private void OnTriggerEnter(Collider other)
    {
        _player.ClothingSystem.ForEachInventorySlot(slot =>
        {
            if (slot != null)
            {
                slot.Condition += _addConditionValue;
                
            }
        });

        _player.ClothingSystem.ForEachInventorySlot(slot =>
        {
            if (slot != null)
            {
                slot.Wet += _addWetValue;
                
            }
        });

        _parameters.GetParameter(_parameterType).Current += _impactValue;
    }
}
