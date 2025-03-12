using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ARUIManager : MonoBehaviour
{
    private static ARUIManager _instance;
    public static ARUIManager Instance { get { if (_instance == null) _instance = FindAnyObjectByType<ARUIManager>(); return _instance; } }

    [BoxGroup("")] public Button _addModelButton;
    [BoxGroup("Models Menu")] public Transform _modelsMenu;
    [BoxGroup("Models Menu")] public Transform _modelButtonsContainer;
    [BoxGroup("Models Menu")] public Transform _modelsButtonPrefab;

    private ARManager _arManager;

    private void Awake()
    {

    }

    private void Start()
    {
        _arManager = ARManager.Instance;

        _addModelButton.onClick.AddListener(ShowModelsMenu);
        PopulateModels();
    }

    private void ShowModelsMenu()
    {
        _modelsMenu.gameObject.SetActive(true);
    }

    private void PopulateModels()
    {
        ARModel[] models = _arManager.GetModels();

        foreach (ARModel model in models)
        {
            GameObject modelButton = Instantiate(_modelsButtonPrefab.gameObject, _modelButtonsContainer);
            modelButton.GetComponentInChildren<TextMeshProUGUI>().text = model.Name;
            modelButton.GetComponentInChildren<RawImage>().texture = model.ModelImage;
            modelButton.GetComponent<Button>().onClick.AddListener(() => _arManager.SelectModel(model));
        }
    }

    public void HideModelsMenu()
    {
        _modelsMenu.gameObject.SetActive(false);
    }

}
