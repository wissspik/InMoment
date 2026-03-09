using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float dragSpeed = 1f;

    private Vector2 lastTouchPos;
    private bool dragging = false;

    public MenuUIController menuController;

    void Update()
    {
        if (!menuController.isMenuClosed)
            return;
        // ПК-версия (для тестов и демонстрации)
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPos;
            MoveCamera(delta);
            lastTouchPos = Input.mousePosition;
        }

        // Мобильная версия
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                dragging = true;
                lastTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.position - lastTouchPos;
                MoveCamera(delta);
                lastTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                dragging = false;
            }
        }
    }

    void MoveCamera(Vector2 screenDelta)
    {
        float scale = cam.orthographicSize * 2f / Screen.height;
        Vector3 worldDelta = new Vector3(-screenDelta.x * scale, -screenDelta.y * scale, 0);

        cam.transform.position += worldDelta * dragSpeed;
    }
}