using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    private GameMode currentMode => GameModeManager.Instance.CurrentMode;

    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private float minBounds = 0.2f;
    [SerializeField] private float maxBounds = 0.2f;

    private Vector3 startMousePos;
    private Vector3 endMousePos;
    private Vector3 startMouseScreenPos;

    private Camera mainCamera;
    public Canvas mainCanvas;

    public event Action<Vector3Int> OnCellClicked;
    public event Action<Vector3, Vector3> OnAreaSelected;
    public event Action<Vector3, Vector3> OnAreaDragging;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        mainCamera = Camera.main;
        GameModeManager.Instance.OnGameModeChanged += HandleGameModeChange;
    }

    private void Update()
    {
        MouseInput();
    }
    private void OnDestroy()
    {
        GameModeManager.Instance.OnGameModeChanged -= HandleGameModeChange;
    }
    private void HandleGameModeChange(GameMode gameMode)
    {
        selectionBox.gameObject.SetActive(false);
    }

    void MouseInput()
    {
        if (Input.GetMouseButtonDown(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelOrders ||  currentMode == GameMode.DeconstructBuilding))
        {
            selectionBox.gameObject.SetActive(true);

            startMouseScreenPos = Input.mousePosition;
            selectionBox.position = startMouseScreenPos;
            selectionBox.sizeDelta = Vector2.zero;

            startMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            startMousePos.z = 0;
        }
        else if (Input.GetMouseButton(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelOrders || currentMode == GameMode.DeconstructBuilding))
        {
            Vector3 currentMouseScreenPos = Input.mousePosition;
            endMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            selectionBox.position = (startMouseScreenPos + currentMouseScreenPos) / 2;

            OnAreaDragging?.Invoke(startMousePos, endMousePos);

            float width = Mathf.Abs(currentMouseScreenPos.x - startMouseScreenPos.x) / mainCanvas.scaleFactor;
            float height = Mathf.Abs(currentMouseScreenPos.y - startMouseScreenPos.y) / mainCanvas.scaleFactor;
            selectionBox.sizeDelta = new Vector2(width, height);
        }
        else if (Input.GetMouseButtonUp(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelOrders || currentMode == GameMode.DeconstructBuilding))
        {
            if (selectionBox.gameObject.activeSelf)
            {
                selectionBox.gameObject.SetActive(false);
                OnAreaSelected?.Invoke(startMousePos, endMousePos);
            }
        }
        if(Input.GetMouseButtonUp(0)) OnCellClicked?.Invoke(GetMouseCellPosition());


    }
    public Vector3Int GetMouseCellPosition()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = Vector3Int.FloorToInt(mouseWorldPosition);
        pos.z = 0;
        return pos;
    }
    public (int startX, int endX, int startY, int endY) CalculateBoundsSelectedArea(Vector3 start, Vector3 end)
    {
        float minX = Mathf.Min(start.x, end.x) + minBounds;
        float maxX = Mathf.Max(start.x, end.x) - maxBounds;
        float minY = Mathf.Min(start.y, end.y) + minBounds;
        float maxY = Mathf.Max(start.y, end.y) - maxBounds;

        int startX = Mathf.FloorToInt(minX);
        int endX = Mathf.FloorToInt(maxX);
        int startY = Mathf.FloorToInt(minY);
        int endY = Mathf.FloorToInt(maxY);

        return (startX, endX, startY, endY);
    }
}


