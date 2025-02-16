using UnityEngine;

[System.Serializable]
public class MovementStatusParameter : StatusParameter
{
    [field: Tooltip("Скорость изменения в покое [ед/м]")]
    [field: SerializeField] public float IdelChangeRate {  get; protected set; }

    [field: Tooltip("Скорость изменения при хотьбе [ед/м]")]
    [field: SerializeField] public float BaseMoveChangeRate { get; protected set; }

    [field: Tooltip("Скорость изменения при беге [ед/м]")]
    [field: SerializeField] public float SprintChangeRate { get; protected set; }

    public virtual void SetChangeRateByMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        ChangeRate = mode switch
        {
            PlayerMovement.PlayerMoveMode.BaseMove => BaseMoveChangeRate,
            PlayerMovement.PlayerMoveMode.Sprint => SprintChangeRate,
            _ => IdelChangeRate,
        };
    }

    public override void Reset()
    {
        base.Reset();
        ChangeRate = IdelChangeRate;
    }
}

