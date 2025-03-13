using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class ARUIManager : MonoBehaviour
{
    private static ARUIManager _instance;
    public static ARUIManager Instance { get { if (_instance == null) _instance = FindAnyObjectByType<ARUIManager>(); return _instance; } }

    [BoxGroup("")] public CanvasGroup _bottomBar;
    [BoxGroup("")] public CanvasGroup _mode;

    [BoxGroup("Texts")] public TextMeshProUGUI _modeText;
    [BoxGroup("Texts")] public TextMeshProUGUI _hintText;

    [BoxGroup("Buttons")] public Button _backButton;
    [BoxGroup("Buttons")] public Button _addModelButton;
    [BoxGroup("Buttons")] public Button _removeModelButton;
    [BoxGroup("Buttons")] public Button _scanModelButton;

    [BoxGroup("Buttons")] public Button _exitModeButton;

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
        _backButton.onClick.AddListener(BackClicked);
        _removeModelButton.onClick.AddListener(() => DeleteClicked());
        _scanModelButton.onClick.AddListener(() => ScanClicked());
        _exitModeButton.onClick.AddListener(() => ExitModeClicked());

        _bottomBar.DOFade(1, .25f).SetEase(Ease.InOutSine);

        _exitModeButton.gameObject.SetActive(false);

        _bottomBar.DOFade(1, 1f);

        _removeModelButton.gameObject.SetActive(false);

        ShowHint(true, "Selecione o modo de interação, utilizando os botões na parte inferior da tela");
    }

    private void ShowModelsMenu()
    {
        PopulateModels();

        _modelsMenu.gameObject.SetActive(true);
        ShowHint(false, "");
    }

    private void PopulateModels()
    {
        ARModel[] models = _arManager.GetModels();

        foreach (ARModel model in models)
        {
            bool skip = false;
            foreach (ARModel instantiatedModel in _arManager.InstantiatedModels)
            {
                if (model.Name == instantiatedModel.Name)
                    skip = true;
            }

            if (skip)
                continue;

            GameObject modelButton = Instantiate(_modelsButtonPrefab.gameObject, _modelButtonsContainer);
            modelButton.GetComponentInChildren<TextMeshProUGUI>().text = model.Name;
            modelButton.GetComponentInChildren<RawImage>().texture = model.ModelImage;
            modelButton.GetComponent<Button>().onClick.AddListener(() => SelectModel(model));
        }
    }

    private void BackClicked()
    {
        PopupContent popupContent = new PopupContent("Deseja realmente sair da área de realidade aumentada?", "Sim", "Não", () =>
        {
            SceneManager.LoadScene("scene_menu");
        });
        PopupsManager.ShowPopup(popupContent);
    }

    private void ScanClicked()
    {
        ShowHint(true, "Aponte a câmera para a imagem do modelo que deseja adicionar");
        ShowMode(true, "Escaneando");

        _arManager.ToggleScanMode(true);
    }

    private void SelectModel(ARModel model)
    {
        _arManager.SelectModel(model);
        HideModelsMenu();
        ShowHint(true, "Toque na tela para posicionar o modelo selecionado");

        ShowMode(true, "Posicionando");

        _exitModeButton.gameObject.SetActive(true);
    }

    private void DeleteClicked()
    {
        ShowMode(true, "Removendo");
        ShowHint(true, "Toque no modelo que deseja excluir");
        _arManager.ToggleDeleteMode(true);
    }

    private void ExitModeClicked()
    {
        _arManager.SelectModel(null);
        _arManager.ToggleScanMode(false);
        _arManager.ToggleDeleteMode(false);

        ShowMode(false, "");
        ShowHint(false, "");

        _modeText.text = "";
    }

    public void ShowMode(bool show, string mode)
    {
        if (show)
            _mode.gameObject.SetActive(true);

        _bottomBar.interactable = !show;
        _bottomBar.blocksRaycasts = !show;

        _bottomBar.DOFade(show ? 0 : 1, .25f).SetEase(Ease.InOutSine);

        float alpha = show ? 1 : 0;

        if (mode != "")
            _modeText.text = mode;

        _mode.DOFade(alpha, .25f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (!show)
                _mode.gameObject.SetActive(false);
        });

    }

    public void ShowDeleteButton(bool show)
    {
        _removeModelButton.gameObject.SetActive(show);
    }

    public void ShowHint(bool show, string text = "")
    {
        if (text != "")
            _hintText.text = text;

        DOTween.Kill(_hintText);
        _hintText.GetComponent<CanvasGroup>().DOFade(show ? 1 : 0, 0.5f);

    }

    public void HideModelsMenu()
    {
        _modelsMenu.gameObject.SetActive(false);

        foreach (Transform child in _modelButtonsContainer)
            Destroy(child.gameObject);
    }

    public bool IsPointerOverUIObject(int fingerId)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.GetTouch(fingerId).position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
