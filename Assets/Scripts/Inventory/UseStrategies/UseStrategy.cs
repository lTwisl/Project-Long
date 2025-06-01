using UnityEngine;
using Zenject;


// ������� ����� ��� ��������� ������������� ���������.
public abstract class UseStrategy : ScriptableObject
{
    // ������ �� ������, �������� ����������� ���������.
    protected Player Player { get; private set; }

    // ��������� ������, ���������� ����������.
    protected PlayerParameters PlayerParameters { get; private set; }


    // ������������� ����������� ����� Zenject.
    [Inject]
    private void Construct(Player player)
    {
        Player = player ?? throw new System.NullReferenceException("Player �� ����� ���� null");

        if (!Player.TryGetComponent(out PlayerStatusManager statusManager))
        {
            throw new System.NullReferenceException("PlayerStatusManager �� ������ � ������");
        }

        PlayerParameters = statusManager.PlayerParameters;
    }


    // ��������� �������� ��� ������������� ��������.
    public virtual void Execute(InventorySlot slot) { }


    // �������� �������� ���������.
    public virtual void Cancel() { }
}