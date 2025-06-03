// ================== СИСТЕМЫ ВЗАИМОДЕЙСТВИЯ ==================
using FirstPersonMovement;
using System;

public class InventoryInteractionSystem : IDisposable
{
    private PlayerParameters _parameters;
    private Inventory _inventory;

    public void Initialize(PlayerParameters parameters, Inventory inventory)
    {
        _parameters = parameters;
        _inventory = inventory;

        _inventory.OnItemAdded += UpdateCurrentCapacity;
        _inventory.OnItemRemoved += UpdateCurrentCapacity;
    }

    private void UpdateCurrentCapacity(InventorySlot _)
    {
        _parameters.Capacity.Current = _inventory.Weight;
    }

    public void Cleanup()
    {
        _inventory.OnItemAdded -= UpdateCurrentCapacity;
        _inventory.OnItemRemoved -= UpdateCurrentCapacity;
    }

    public void Dispose()
    {
        Cleanup();
    }
}
