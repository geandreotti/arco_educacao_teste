using System.Collections.Generic;
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


    private List<ARModel> _intantiatedModels = new List<ARModel>();

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


    public ARModel[] Models => _models;
    public ARModel Selecting => _selecting;

    public List<ARModel> InstantiatedModels => _intantiatedModels;

    private void Awake()
    {
        _camera = Camera.main;
        _planeManager = FindAnyObjectByType<ARPlaneManager>();
        _raycastManager = FindAnyObjectByType<ARRaycastManager>();
        _trackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();

        _trackedImageManager.enabled = false;

        _indicatorScale = _indicator.localScale;
        _indicator.DOScale(0, 0);

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

        _trackedImageManager.enabled = _scanMode;

        DOTween.Kill(_indicator);
        _spawnIndicatorActive = false;
        _indicator.DOScale(0, .25f).SetEase(Ease.InOutSine);
    }

    private ARModel _trackedModel;
    public void UpdateImageTracking(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!_scanMode)
            return;

        foreach (ARTrackedImage image in eventArgs.added)
        {
            if (image.trackingState == TrackingState.Tracking)
            {
                if (_trackedModel == null)
                {
                    foreach (ARModel model in _models)
                    {
                        if (model.Name == image.referenceImage.name)
                        {
                            _trackedModel = Instantiate(model, image.transform.position, image.transform.rotation);
                            _trackedModel.transform.SetPositionAndRotation(image.transform.position, image.transform.rotation);
                            break;
                        }
                    }
                }
                else
                {
                    if (image.referenceImage.name != _trackedModel.Name)
                    {
                        _trackedModel.Show(false);
                        _trackedModel = null;
                        return;
                    }

                    _trackedModel.transform.SetPositionAndRotation(image.transform.position, image.transform.rotation);

                }

                return;
            }

            if (image.trackingState == TrackingState.Limited || image.trackingState == TrackingState.None)
            {
                _trackedModel.Show(false);
                _trackedModel = null;
            }
        }


    }

    private void UpdateIndicator()
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
                _indicator.DOScale(_indicatorScale, .25f).SetEase(Ease.InOutSine);
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

    private void CheckSpawn()
    {
        if (_selecting == null)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (ARUIManager.Instance.IsPointerOverUIObject(Input.GetTouch(0).fingerId))
                return;
            else
                SpawnModel();
        }

    }

    private void CheckInteraction()
    {
        if (_intantiatedModels.Count == 0)
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
                            _intantiatedModels.Remove(model);
                            Destroy(model.gameObject);
                            ARUIManager.Instance.ShowDeleteButton(_intantiatedModels.Count > 0);
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

        Debug.Log("Interacting with " + model.name);

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

                Vector3 newScale = _interacting.transform.localScale - (Vector3.one * deltaMagnitudeDiff * 0.01f) / 5;
                newScale = Vector3.Max(newScale, Vector3.one * 0.1f); // Prevent scaling too small
                newScale = Vector3.Min(newScale, Vector3.one * 10f); // Prevent scaling too large

                _interacting.transform.localScale = newScale;
            }
        }

        if (Input.touchCount == 0 && _interacting != null)
            _interacting = null;
    }

    private void SpawnModel()
    {
        DOTween.Kill(_indicator);
        _spawnIndicatorActive = false;
        _indicator.DOScale(0, .25f).SetEase(Ease.InOutSine);

        _intantiatedModels.Add(Instantiate(_selecting, _indicator.position, Quaternion.identity));
        _selecting = null;

        ARUIManager.Instance.ShowMode(false, "");
        ARUIManager.Instance.ShowHint(false, "");
        ARUIManager.Instance.ShowDeleteButton(_intantiatedModels.Count > 0);
    }
}

