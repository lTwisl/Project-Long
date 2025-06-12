// ================== СИСТЕМЫ ВЗАИМОДЕЙСТВИЯ ==================
using System;

public class InventoryInteractionSystem : IDisposable
{
    private PlayerParameters _parameters;
    private Inventory _inventory;

    public void Initialize(PlayerParameters parameters, Inventory inventory)
    {
        _parameters = parameters;
        _inventory = inventory;

        _inventory.OnChangedWeight += UpdateCurrentCapacity;
    }

    private void UpdateCurrentCapacity(float weight)
    {
        _parameters.Capacity.Current = weight;
    }

    public void Cleanup()
    {
        _inventory.OnChangedWeight -= UpdateCurrentCapacity;
    }

    public void Dispose()
    {
        Cleanup();
    }
}
