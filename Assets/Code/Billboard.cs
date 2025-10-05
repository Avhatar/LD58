using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Core.instance.cameraController.cameraMain;
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);
        }
    }
}