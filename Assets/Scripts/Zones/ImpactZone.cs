using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ImpactZone : MonoBehaviour
{
    [SerializeField] private List<GivesParameter> _givesParameters;

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

        foreach (var param in _givesParameters)
            _parameters.GetParameter(param.ParameterType).Current += param.Value;
    }
}
