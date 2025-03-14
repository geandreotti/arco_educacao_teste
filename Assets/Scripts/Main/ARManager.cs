using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARManager : MonoBehaviour
{
    private static ARManager _instance;
    public static ARManager Instance { get { if (_instance == null) _instance = FindAnyObjectByType<ARManager>(); return _instance; } }

    [BoxGroup("")][ReorderableList][SerializeField] private ARModel[] _models;
    [BoxGroup("")][SerializeField] private Transform _indicator;
    [BoxGroup("")][SerializeField] private float _interactionRotationSpeed = 1f;


    private List<ARModel> _instantiatedModels = new List<ARModel>();

    private bool _hasPlane;

    private bool _scanMode = false;
    private bool _deleteMode = false;
    private bool _spawnIndicatorActive = false;

    private ARModel _selecting;
    private ARModel _interacting;

    private Vector3 _indicatorScale;
    private Vector3 _targetIndicatorPosition;

    private Camera _camera;

    private ARPlaneManager _planeManager;
    private ARRaycastManager _raycastManager;
    private ARTrackedImageManager _trackedImageManager;


    private ARModel _trackedModel;

    public ARModel[] Models => _models;
    public ARModel Selecting => _selecting;

    public List<ARModel> InstantiatedModels => _instantiatedModels;

    private void Awake()
    {

        _camera = Camera.main;
        _planeManager = FindAnyObjectByType<ARPlaneManager>();
        _raycastManager = FindAnyObjectByType<ARRaycastManager>();
        _trackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();

        _indicatorScale = _indicator.localScale;
        _indicator.DOScale(0, 0);

        Screen.SetResolution(Mathf.RoundToInt(Screen.width / 1.4f), Mathf.RoundToInt(Screen.height / 1.4f), true);


        Application.targetFrameRate = 30;
    }

    private void Update()
    {
        UpdateIndicator();
        CheckSpawn();
        CheckInteraction();
        UpdateInteraction();
    }

    public ARModel[] GetModels()
    {
        return _models;
    }

    public void SelectModel(ARModel model)
    {
        _selecting = model;
    }

    private void CheckSpawn()
    {
        if (_selecting == null || !_spawnIndicatorActive)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (ARUIManager.Instance.IsPointerOverUIObject(Input.GetTouch(0).fingerId))
                return;
            else
                SpawnModel(_selecting, _indicator.position, Quaternion.identity);
        }

    }

    private ARModel SpawnModel(ARModel model, Vector3 position = default, Quaternion rotation = default)
    {
        DOTween.Kill(_indicator);
        _spawnIndicatorActive = false;
        _indicator.DOScale(0, .25f).SetEase(Ease.InOutSine);

        ARModel arModel = Instantiate(model, position, rotation);

        _instantiatedModels.Add(arModel);

        if (_selecting != null)
            ARUIManager.Instance.ShowMode(false, "");
        ARUIManager.Instance.ShowHint(false, "");
        ARUIManager.Instance.ShowDeleteButton(_instantiatedModels.Count > 0);
        _selecting = null;

        return arModel;
    }

    private void DeleteAllModels()
    {
        foreach (ARModel model in _instantiatedModels)
            model.Show(false);

        _instantiatedModels.Clear();

        ARUIManager.Instance.ShowDeleteButton(false);
    }

    public void ToggleDeleteMode(bool value = false)
    {
        _deleteMode = value;
    }

    public void ToggleScanMode(bool value = false)
    {
        _scanMode = value;

        _selecting = null;

        _raycastManager.enabled = !_scanMode;
        _planeManager.enabled = !_scanMode;

        DOTween.Kill(_indicator);
        _spawnIndicatorActive = false;
        _indicator.DOScale(0, .25f).SetEase(Ease.InOutSine);

        if (!_deleteMode)
            DeleteAllModels();
    }

    private async void UpdateIndicator()
    {
        if (_selecting == null)
        {
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (_raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds))
        {
            _targetIndicatorPosition = hits[0].pose.position;
            if (!_spawnIndicatorActive)
            {
                DOTween.Kill(_indicator);
                _indicator.position = _targetIndicatorPosition;
                _indicator.DOScale(_indicatorScale, .25f).SetEase(Ease.InOutSine).OnComplete(() => { });

                await Task.Delay(250);
                _spawnIndicatorActive = true;
            }
        }
        else
        {
            DOTween.Kill(_indicator);
            if (_spawnIndicatorActive)
            {
                _indicator.DOScale(0, .25f).SetEase(Ease.InOutSine);
                _spawnIndicatorActive = false;
            }
        }

        _indicator.position = _targetIndicatorPosition; //Vector3.Lerp(_indicator.position, _targetIndicatorPosition, Time.deltaTime * 15);
    }

    private void CheckInteraction()
    {
        if (_instantiatedModels.Count == 0)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (ARUIManager.Instance.IsPointerOverUIObject(touch.fingerId))
                    return;

                Ray ray = _camera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    ARModel model = hit.collider.GetComponentInParent<ARModel>();

                    if (model != null)
                    {
                        if (_deleteMode)
                        {
                            _instantiatedModels.Remove(model);
                            model.Show(false);
                            ARUIManager.Instance.ShowMode(_instantiatedModels.Count != 0, "");

                            if (_instantiatedModels.Count == 0)
                                ToggleDeleteMode(false);

                            ARUIManager.Instance.ShowDeleteButton(_instantiatedModels.Count > 0);
                        }
                        else
                            SetInteraction(model);
                    }
                }
            }
        }
    }

    private void SetInteraction(ARModel model)
    {
        if (model == null)
            return;

        _interacting = model;
    }

    private void UpdateInteraction()
    {
        if (_interacting == null)
            return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {

                Vector2 touchDeltaPosition = touch.deltaPosition;
                float targetTorqueY = -touchDeltaPosition.x * 0.1f;

                Rigidbody rb = _interacting.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    float currentTorqueY = rb.angularVelocity.y;
                    float smoothTorqueY = Mathf.Lerp(currentTorqueY, targetTorqueY * _interactionRotationSpeed, Time.deltaTime * 5f);
                    rb.AddTorque(rb.transform.up * smoothTorqueY, ForceMode.Force);
                }
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                Vector3 targetScale = _interacting.transform.localScale - (Vector3.one * deltaMagnitudeDiff * 0.01f) / 5;
                targetScale = Vector3.Max(targetScale, Vector3.one * 0.1f);
                targetScale = Vector3.Min(targetScale, Vector3.one * 10f);

                _interacting.transform.localScale = Vector3.Lerp(_interacting.transform.localScale, targetScale, Time.deltaTime * 5f);
            }
        }

        if (Input.touchCount == 0 && _interacting != null)
            _interacting = null;
    }


    #region AR Callbacks
    public void UpdateImageTracking(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!_scanMode)
            return;

        foreach (ARTrackedImage image in eventArgs.updated)
        {
            Debug.LogError(image.referenceImage.name);

            if (image.trackingState == TrackingState.Tracking)
            {
                if (_trackedModel == null)
                {
                    foreach (ARModel model in _models)
                    {
                        if (model.Name == image.referenceImage.name)
                        {
                            _trackedModel = SpawnModel(model, image.transform.position, image.transform.rotation);
                            break;
                        }
                    }
                }
                else
                {
                    if (image.referenceImage.name != _trackedModel.Name)
                    {
                        DeleteAllModels();
                        return;
                    }

                    if (image.referenceImage.name == _trackedModel.Name)
                        _trackedModel.transform.position = Vector3.Lerp(_trackedModel.transform.position, image.transform.position, Time.deltaTime * 15);

                }

                return;
            }

            if (image.trackingState == TrackingState.None || image.trackingState == TrackingState.Limited)
            {
                if (_trackedModel == null || image.referenceImage.name != _trackedModel.Name)
                    return;

                DeleteAllModels();
            }
        }
    }

    public void OnPlaneAdded(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (_hasPlane)
        {
            foreach (var plane in eventArgs.added)
            {
                plane.gameObject.SetActive(true);
            }

            foreach (var plane in eventArgs.updated)
            {
                if (plane.trackingState == TrackingState.Tracking || plane.trackingState == TrackingState.Limited)
                    plane.gameObject.SetActive(true);

            }

            foreach (var plane in eventArgs.removed)
            {
                plane.Value.gameObject.SetActive(false);
            }
        }

        _hasPlane = true;
    }
    #endregion
}

