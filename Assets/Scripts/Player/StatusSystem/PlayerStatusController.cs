using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;


public class PlayerStatusController : MonoBehaviour
{
    public float Health = 100;

    [SerializeField] private float _damageHunger = 0.1f;
    [SerializeField] private float _damageThirst = 0.2f;
    [SerializeField] private float _damageCold = 0.4f;

    public DecreasingStatusParameter Hunger { get; private set; }
    public DecreasingStatusParameter Thirst { get; private set; }
    public DecreasingStatusParameter Fatigue { get; private set; }
    public DecreasingStatusParameter Cold { get; private set; }
    public DecreasingStatusParameter Infection { get; private set; }

    private List<IStatusParameter> _allParameters = new();



    private void Awake()
    {
        Hunger = new DecreasingStatusParameter(0.5f, 0.1f);
        Thirst = new DecreasingStatusParameter(1, 0.1f);
        Fatigue = new DecreasingStatusParameter(100, 0.1f);
        Cold = new DecreasingStatusParameter(1.5f, 0.1f);
        Infection = new DecreasingStatusParameter(100, 0.1f);

        _allParameters.AddRange(new IStatusParameter[] { Hunger, Thirst, Fatigue, Cold, Infection });

        Hunger.OnValueChanged += ApplayDamageHunger;
        Thirst.OnValueChanged += ApplayDamageThirst;
        Cold.OnValueChanged += ApplayDamageCold;
    }


    private void Update()
    {
        foreach (var parameter in _allParameters)
        {
            parameter.UpdateParameter(Time.deltaTime);
        }
    }


    private void OnDestroy()
    {
        Hunger.OnValueChanged -= ApplayDamageHunger;
        Thirst.OnValueChanged -= ApplayDamageThirst;
        Cold.OnValueChanged -= ApplayDamageCold;
    }

    [ContextMenu("RestoreHunger")]
    public void RestoreHunger()
    {
        Hunger.Restore();
    }

    private void ApplayDamageHunger(float value)
    {
        if (!Mathf.Approximately(Hunger.Current, 0.0f))
            return;

        EveryUpdateTakeDamage(Hunger, _damageHunger);
    }

    private void ApplayDamageThirst(float value)
    {
        if (!Mathf.Approximately(Thirst.Current, 0.0f))
            return;

        EveryUpdateTakeDamage(Thirst, _damageThirst);
    }

    private void ApplayDamageCold(float value)
    {
        if (!Mathf.Approximately(Cold.Current, 0.0f))
            return;

        EveryUpdateTakeDamage(Cold, _damageCold);
    }

    async void EveryUpdateTakeDamage(IStatusParameter parametr, float damage)
    {
        while (Mathf.Approximately(parametr.Current, 0.0f))
        {
            Health -= damage * Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
    }
}
