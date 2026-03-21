using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("ѕеремещение камеры")]
    public float panSpeed = 15f;
    public float panBorderThickness = 10f;

    private float limitMinX => - PathfindingManager.Instance.Grid.gridWidth / 2;
    private float limitMinY => - PathfindingManager.Instance.Grid.gridHeight / 2;
    private float limitMaxX => PathfindingManager.Instance.Grid.gridWidth / 2;
    private float limitMaxY => PathfindingManager.Instance.Grid.gridHeight / 2;

    [Header("«ум камеры")]
    public float scrollSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    private Camera cam;
    private Vector3 dragOrigin;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
    }
    void HandleCameraMovement()
    {
        Vector3 pos = transform.position;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.mousePosition.y >= Screen.height - panBorderThickness) pos.y += panSpeed * Time.deltaTime;
            if (Input.mousePosition.y <= panBorderThickness) pos.y -= panSpeed * Time.deltaTime;
            if (Input.mousePosition.x >= Screen.width - panBorderThickness) pos.x += panSpeed * Time.deltaTime;
            if (Input.mousePosition.x <= panBorderThickness) pos.x -= panSpeed * Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(2)) dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            pos += difference;
        }
        pos.y = Mathf.Clamp(pos.y, limitMinY, limitMaxY);
        pos.x = Mathf.Clamp(pos.x, limitMinX, limitMaxX);
        transform.position = pos;
    }
    void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f && !EventSystem.current.IsPointerOverGameObject())
        {
            float targetSite = cam.orthographicSize - (scroll * scrollSpeed);
            cam.orthographicSize = Mathf.Clamp(targetSite, minZoom, maxZoom);
        }
    }
}
