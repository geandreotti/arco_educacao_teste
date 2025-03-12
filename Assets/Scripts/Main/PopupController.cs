using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    [BoxGroup("References")][SerializeField] private CanvasGroup _background;

    [Space]
    [BoxGroup("References")][SerializeField] private TextMeshProUGUI _messageText;
    [BoxGroup("References")][SerializeField] private TextMeshProUGUI _confirmText;
    [BoxGroup("References")][SerializeField] private TextMeshProUGUI _cancelText;
    [Space]
    [BoxGroup("References")][SerializeField] private Button _confirmButton;
    [BoxGroup("References")][SerializeField] private Button _cancelButton;

    public void Setup(PopupContent content)
    {
        _messageText.text = content.message;
        _confirmText.text = content.confirm;
        _cancelText.text = content.cancel;

        _confirmButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.RemoveAllListeners();

        _confirmButton.onClick.AddListener(() =>
        {
            content.OnConfirm?.Invoke();
            Hide();

        });

        _cancelButton.onClick.AddListener(() =>
        {
            content.OnCancel?.Invoke();
            Hide();
        });

        Show();
    }

    public void Show()
    {
        _background.DOFade(1, 1f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}


