using UnityEngine;

public class Storage : MonoBehaviour, IInteractible
{
    [field: SerializeField] public Inventory Inventory { get; protected set; }

    [field: SerializeField] public InteractionType InteractionType { get; protected set; }

    private string _storageId;
    public virtual bool IsCanInteract { get; protected set; } = true;

    private void Awake()
    {
        _storageId = gameObject.name + transform.position.ToString();
        Inventory.Init();
        //LoadData();
    }


    private void LoadData()
    {
        string json = PlayerPrefs.GetString(_storageId);
        Inventory = JsonUtility.FromJson<Inventory>(json);
    }

    private void OnDestroy()
    {
        string json = JsonUtility.ToJson(Inventory);
        PlayerPrefs.SetString(_storageId, json);
    }

    public virtual void Interact(Player player)
    {
        IsCanInteract = false;

        foreach (var slot in Inventory.Slots)
        {
            player.Inventory.AddItem(slot.Item, slot.Capacity, slot.Condition);
        }

        Inventory.Clear();
    }
}
