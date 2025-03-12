using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private static MenuController _instance;

    public static MenuController Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<MenuController>();
            return _instance;
        }
    }

    [BoxGroup("References")][SerializeField] private RectTransform _logo;
    [BoxGroup("References")][SerializeField] private Button _ARButton;
    [BoxGroup("References")][SerializeField] private Button _exitButton;


    private void Awake()
    {
        _ARButton.onClick.RemoveAllListeners();
        _exitButton.onClick.RemoveAllListeners();

        _ARButton.onClick.AddListener(ARButtonClicked);
        _exitButton.onClick.AddListener(ExitButtonClicked);
    }

    private void ARButtonClicked()
    {
        LoadAR();
    }

    private void ExitButtonClicked()
    {
        ShowExitPopup();
        _ARButton.GetComponent<CanvasGroup>().DOFade(0, .5f);
        _logo.DOAnchorPosY(400, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => { });
    }

    private void ShowExitPopup()
    {
        PopupsManager.ShowPopup(PopupsManager.SetContent("Deseja sair do aplicativo?", "Sim", "Não", Application.Quit, Reset));
    }

    private void Reset()
    {
        _logo.DOAnchorPosY(200, .5f);
        _ARButton.GetComponent<CanvasGroup>().DOFade(1, .5f);
    }

    private void LoadAR()
    {
        //em um projeto real, o load da cena seria feito em um script independente, com funções de carregamento e transição de cena
        SceneManager.LoadScene("scene_ar");
    }
}
