using UnityEngine;
using Zenject;


// ������� ����� ��� ��������� ������������� ���������.
public abstract class UseStrategy : ScriptableObject
{
    // ��������� ������, ���������� ����������.
    protected PlayerParameters PlayerParameters { get; private set; }

    [Inject]
    private void Construct(PlayerParameters playerParameters)
    {
        PlayerParameters = playerParameters;
    }


    // ��������� �������� ��� ������������� ��������.
    public virtual void Execute(InventorySlot slot) { }


    // �������� �������� ���������.
    public virtual void Cancel() { }
}