using NaughtyAttributes;
using UnityEngine;

public class ARManager : MonoBehaviour
{
    private static ARManager _instance;
    public static ARManager Instance { get { if (_instance == null) _instance = FindAnyObjectByType<ARManager>(); return _instance; } }

    public ARModel[] Models { get; internal set; }

    [BoxGroup("")] [ReorderableList] [SerializeField] private ARModel[] _models;

    public ARModel[] GetModels()
    {
        return _models;
    }

    public void SelectModel(ARModel model)
    {
        Debug.Log("Selected model: " + model.Name);
    }

}

[System.Serializable]
public class ARModel
{   
    [SerializeField] private string _name;
    [SerializeField] private GameObject _modelPrefab;
    [SerializeField] private Texture2D _modelImage;

    public string Name { get => _name; }
    public GameObject ModelPrefab { get => _modelPrefab; }
    public Texture2D ModelImage { get => _modelImage; }
}
