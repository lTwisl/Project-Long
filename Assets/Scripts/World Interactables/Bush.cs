using UnityEngine;
using EditorAttributes;

public class Bush : Storage
{
    [SerializeField] private GameObject[] _hideAfterPickup;

    [Suffix("days"), TimeConversion]
    [SerializeField] private float _reload = 3;

    private float timer = 0;

    public override void AfterInteract() 
    {
        timer = _reload * 24 * 60; // Перевод из дней в минуты
        GameTime.OnMinuteChanged += HandleChangedMinute;

        foreach (var obj in _hideAfterPickup)
        {
            obj.SetActive(false);
        }
    }

    private void HandleChangedMinute()
    {
        timer -= 1;

        if (timer <= 0)
        {
            IsCanInteract = true;

            foreach (var obj in _hideAfterPickup)
            {
                obj.SetActive(true);
            }
            GameTime.OnMinuteChanged -= HandleChangedMinute;
        }
    }
}
