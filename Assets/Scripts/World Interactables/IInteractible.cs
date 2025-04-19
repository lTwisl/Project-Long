
public interface IInteractible
{
    InteractionType InteractionType { get; }
    public bool IsCanInteract { get; }

    void Interact(Player player);
}

public enum InteractionType
{
    Click,   // Нажатие
    Hold     // Удерджание
}

