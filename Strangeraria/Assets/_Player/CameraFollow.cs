using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // Le joueur à suivre
    public Vector3 offset = new Vector3(0, 0, 0); // Décalage de la caméra
    public float smoothSpeed = 5f;   // Vitesse de lissage

    [Header("Pixel Snap")]
    public bool snapToPixel = false;
    public float pixelsPerUnit = 128f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (snapToPixel)
        {
            float unitsPerPixel = 1f / pixelsPerUnit;
            smoothedPosition.x = Mathf.Round(smoothedPosition.x / unitsPerPixel) * unitsPerPixel;
            smoothedPosition.y = Mathf.Round(smoothedPosition.y / unitsPerPixel) * unitsPerPixel;
        }

        transform.position = smoothedPosition;
    }
}