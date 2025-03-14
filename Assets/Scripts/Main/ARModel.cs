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

    private void Start()
    {
        _indicatorParent = transform.Find("indicator");
        _indicatorText = _indicatorParent.Find("name");

        Show(true);
    }

    public void Show(bool show)
    {
        transform.GetChild(0).DOLocalMoveY(.15f, .75f).SetEase(Ease.InOutSine);
        transform.DOScale(show ? Vector3.one : Vector3.zero, .75f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (!show)
                Destroy(gameObject);
        });
    }

    private void Update()
    {
        
        Vector3 direction = Camera.main.transform.position - _indicatorParent.position;
        direction.y = 0;
        _indicatorParent.rotation = Quaternion.LookRotation(-direction);

        direction = Camera.main.transform.position - _indicatorText.position;
        _indicatorText.rotation = Quaternion.LookRotation(-direction);

    }
}
