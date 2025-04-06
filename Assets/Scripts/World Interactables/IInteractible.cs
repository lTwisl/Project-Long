
public interface IInteractible
{
    InteractionType InteractionType { get; }
    public bool IsCanInteract { get; }

    void Interact(Player player);
}

public enum InteractionType
{
    Instant,   // Мгновенное
    Hold       // Требует удержания
}

