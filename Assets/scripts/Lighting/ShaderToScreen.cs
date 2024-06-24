using UnityEngine;

public class ShaderToScreen : MonoBehaviour 
{
    public ComputeShader computeShader;
    private RenderTexture sourceTexture;
    private RenderTexture destinationTexture;
    private Camera camera;
    public Camera computeView;

    void Start() {
        camera = GetComponent<Camera>();
        if(camera == null) {
            Debug.LogError("CameraImageProcessor needs to be attached to a camera.");
            return;
        }

        InitializeRenderTextures();
        camera.targetTexture = sourceTexture;
    }

    void InitializeRenderTextures() {
        sourceTexture = new RenderTexture(Screen.width, Screen.height, 24);
        sourceTexture.enableRandomWrite = false;
        sourceTexture.Create();

        destinationTexture = new RenderTexture(Screen.width, Screen.height, 24);
        destinationTexture.enableRandomWrite = true;
        destinationTexture.Create();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        // Ensure the camera's output is ready and available in sourceTexture
        computeView.Render();

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "Source", sourceTexture);
        computeShader.SetTexture(kernelHandle, "Result", destinationTexture);
        computeShader.Dispatch(kernelHandle, destinationTexture.width / 8, destinationTexture.height / 8, 1);

        // Now blit the processed texture to the screen or the destination render texture
        Graphics.Blit(destinationTexture, dest);
    }
}