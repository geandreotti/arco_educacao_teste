using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ARModel : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] private Texture2D _modelImage;

    private Transform _indicatorParent;
    private Transform _indicatorText;

    public string Name { get => _name; }
    public Texture2D ModelImage { get => _modelImage; }

    private bool _isDragging = false;

    private void Start()
    {
        _indicatorParent = transform.Find("indicator");
        _indicatorText = _indicatorParent.Find("name");

        Show(true);
    }

    public void Show(bool show)
    {
        transform.GetChild(0).DOLocalMoveY(.15f, .75f).SetEase(Ease.InOutSine);
        transform.DOScale(show ? Vector3.one : Vector3.zero, .75f).SetEase(Ease.InOutSine);
    }

    private void Update()
    {
        Vector3 direction = Camera.main.transform.position - _indicatorParent.position;
        direction.y = 0; // Keep only the horizontal direction
        _indicatorParent.rotation = Quaternion.LookRotation(direction);

        Vector3 textDirection = _indicatorText.position - Camera.main.transform.position;
        _indicatorText.rotation = Quaternion.LookRotation(textDirection);
    }
}
