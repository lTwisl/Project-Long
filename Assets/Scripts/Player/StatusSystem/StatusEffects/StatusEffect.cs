using EditorAttributes;
using System;
using UnityEngine;


public abstract class StatusEffect : ScriptableObject, IDisposable
{
    [field: SerializeField] public string Name { get; private set; } = "Empty";
    [field: SerializeField, TextArea] public string Description { get; private set; } = "Empty";
    [field: SerializeField, AssetPreview] public Sprite Icon { get; private set; }

    protected Player _player;
    public bool IsActive { get; protected set; }

    public virtual void Init(Player player)
    {
        IsActive = false;
        _player = player;
    }

    public abstract void OnApply();
    public abstract void OnRemove();

    public virtual void Dispose() { }
}
