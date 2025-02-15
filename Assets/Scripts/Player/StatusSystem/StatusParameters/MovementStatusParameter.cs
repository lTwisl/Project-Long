using UnityEngine;

[System.Serializable]
public class MovementStatusParameter : StatusParameter
{
    [field: SerializeField] public float IdelChangeRate {  get; protected set; }
    [field: SerializeField] public float BaseMoveChangeRate { get; protected set; }
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
}

