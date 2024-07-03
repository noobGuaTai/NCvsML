using UnityEngine;

public class CameraAspectRatio : MonoBehaviour
{
    public float targetAspect = 16.0f / 9.0f;

    void Update()
    {
        SetAspectRatio();
    }

    void SetAspectRatio()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
    void OnPreRender()
    {
        float currentAspect = System.Convert.ToSingle((double)Screen.width / Screen.height);
        if (currentAspect > Camera.main.aspect)
        {
            Camera.main.pixelRect = new Rect(0, 0, Screen.height * 16 / 9.0f, Screen.height);
            Camera.main.pixelRect = new Rect(Screen.width / 2 - Camera.main.pixelWidth / 2, Screen.height / 2 - Camera.main.pixelHeight / 2, Camera.main.pixelWidth, Camera.main.pixelHeight);
        }
        else if (currentAspect < Camera.main.aspect)
        {
            Camera.main.pixelRect = new Rect(0, 0, Screen.width, Screen.width / 16.0f * 9);
            Camera.main.pixelRect = new Rect(Screen.width / 2 - Camera.main.pixelWidth / 2, Screen.height / 2 - Camera.main.pixelHeight / 2, Camera.main.pixelWidth, Camera.main.pixelHeight);
        }
    }
}
