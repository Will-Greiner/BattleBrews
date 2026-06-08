using UnityEngine;

public class HandLogic : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float distanceFromCamera = 5f;

    public Camera camera;

    void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(-camera.transform.forward,camera.transform.position + camera.transform.forward * distanceFromCamera);

        float enter;

        if (plane.Raycast(ray, out enter))
        {
            Vector3 targetPosition = ray.GetPoint(enter);

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }
}