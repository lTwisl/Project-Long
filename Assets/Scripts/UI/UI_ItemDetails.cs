using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemDetails : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _text;

    public void UpdateView(InventoryItem item)
    {
        if (item == null)
        {
            _icon.sprite = null;
            _text.text = "";
            return;
        }

        _icon.sprite = item.Icon;
        _text.text = item.ToString();
    }
}
