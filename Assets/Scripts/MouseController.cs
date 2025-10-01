using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }
                
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit2D = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit2D.collider != null)
            {
                CheckClickCollider(hit2D.collider.gameObject);
            }
        }
    }

    void CheckHover(GameObject gameObject)
    {
        if (gameObject.GetComponent<NPCBehaviour>())
        {
            return;
        }
    }

    void CheckClickCollider(GameObject gameObject)
    {
        if (gameObject.GetComponent<NPCBehaviour>())
        {
            gameObject.GetComponent<NPCBehaviour>().SetAsTarget(!gameObject.GetComponent<NPCBehaviour>().IsTarget);
            return;
        }
    }
}