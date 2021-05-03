using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CamPicker : MonoBehaviour
{
    public RawImage image;
    public RectTransform imageParent;
    public AspectRatioFitter imageFitter;
    public int capturedPhotoIndex = 0;
    public GameObject GuideCube;
    public GameObject loadingPanel;
    public List<GameObject> guideCubeAllCells;

    // Device cameras
    WebCamDevice frontCameraDevice;
    WebCamDevice backCameraDevice;
    WebCamDevice activeCameraDevice;

    WebCamTexture frontCameraTexture;
    WebCamTexture backCameraTexture;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    private int squareImageSize;
    private float gridToPhotoScale;
    private int gridCellSize = 150;
    private int realCellSpaceFromBorder = 75;
    private int cellSpaceFromBorder;
    private int gridImageSize = 900;

    void Start()
    {
        // Check for device cameras
        if (WebCamTexture.devices.Length == 0)
        {
            return;
        }

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices[0];

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
        backCameraTexture = new WebCamTexture(backCameraDevice.name);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;

        // Set the camera to use by default
        SetActiveCamera(backCameraTexture);
    }

    // Set the device camera to use and start it
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
            device.name == cameraToUse.deviceName);

        image.texture = activeCameraTexture;
        image.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();
    }

    public void CapturePhoto()
    {
        StartCoroutine(nameof(TakePhoto));
    }

    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {

        // NOTE - you almost certainly have to do this here:

        yield return new WaitForEndOfFrame();

        // it's a rare case where the Unity doco is pretty clear,
        // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
        // be sure to scroll down to the SECOND long example on that doco page 

        Texture2D photo = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
        photo.SetPixels(activeCameraTexture.GetPixels());
        photo.Apply();

        Texture2D camTexture = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
        camTexture.SetPixels(activeCameraTexture.GetPixels());
        camTexture.Apply();

        if (activeCameraTexture.videoRotationAngle == 90)
        {
            camTexture = RotateTexture(activeCameraTexture, true);
        }

        squareImageSize = Math.Min(camTexture.width, camTexture.height);

        gridToPhotoScale = (squareImageSize / (float)gridImageSize);
        int smallPhotoSize = (int)(gridCellSize * gridToPhotoScale);
        cellSpaceFromBorder = (int)(realCellSpaceFromBorder * gridToPhotoScale);
        int firstXPosition = (int)(camTexture.width / 2f - squareImageSize / 2f + cellSpaceFromBorder);
        int firstYPosition = (int)(camTexture.height / 2f - squareImageSize / 2f + cellSpaceFromBorder);

        Color[][] colors = new Color[9][];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xPosition = firstXPosition + j * ((int)(gridCellSize * gridToPhotoScale) + (cellSpaceFromBorder * 2));
                int yPosition = firstYPosition + i * ((int)(gridCellSize * gridToPhotoScale) + (cellSpaceFromBorder * 2));
                colors[3 * i + j] = camTexture.GetPixels(xPosition, yPosition, smallPhotoSize, smallPhotoSize);

            }
        }


        Vector3 guideCubeRotation = GuideCube.transform.rotation.eulerAngles;

        switch (capturedPhotoIndex)
        {
            case 0:
            case 1:
            case 2:
                guideCubeRotation += new Vector3(0, 90, 0);
                break;
            case 3:
                guideCubeRotation += new Vector3(90, 0, 0);
                break;
            case 4:
                guideCubeRotation += new Vector3(-180, 0, 0);
                break;

        }
        Quaternion rotation = Quaternion.Euler(guideCubeRotation);
        StartCoroutine(rotateObject(GuideCube, rotation, 1f));
        SetGuideCubeFaceColor(capturedPhotoIndex, colors);
        if (capturedPhotoIndex < 5)
        {
            capturedPhotoIndex++;
            StartCoroutine(ExecuteAfterTime(0, colors));
        }
        else
        {
            loadingPanel.SetActive(true);
            capturedPhotoIndex = 0;
            StartCoroutine(ExecuteAfterTime(0.3f, colors));
        }

    }

    IEnumerator ExecuteAfterTime(float time, Color[][] colors)
    {
        yield return new WaitForSeconds(time);

        ColorDetector.AddFaceColor(colors);
    }

    bool rotating = false;
    IEnumerator rotateObject(GameObject gameObjectToMove, Quaternion newRot, float duration)
    {
        if (rotating)
        {
            yield break;
        }
        rotating = true;

        Quaternion currentRot = gameObjectToMove.transform.rotation;

        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            gameObjectToMove.transform.rotation = Quaternion.Lerp(currentRot, newRot, counter / duration);
            yield return null;
        }

        gameObjectToMove.transform.rotation = newRot;
        rotating = false;
    }

    Texture2D RotateTexture(WebCamTexture originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    // Switch between the device's front and back camera
    public void SwitchCamera()
    {
        SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ?
            backCameraTexture : frontCameraTexture);
    }

    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started
    void Update()
    {
        // Skip making adjustment for incorrect camera data
        if (activeCameraTexture.width < 100)
        {
            return;
        }

        // Rotate image to show correct orientation 
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        image.rectTransform.localEulerAngles = rotationVector;

        // Set AspectRatioFitter's ratio
        float videoRatio =
            (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        imageFitter.aspectRatio = videoRatio;

        // Unflip if vertically flipped
        image.uvRect =
            activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        // Mirror front-facing camera's image horizontally to look more natural
        imageParent.localScale =
            activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;


        // GuideCube.transform.rotation = Quaternion.Lerp(guideCubeRotation, guideCubeNewRotation, 10f * Time.deltaTime);
        // if (Quaternion.Angle(guideCubeRotation, guideCubeNewRotation) <= 1)
        // {
        //     GuideCube.transform.rotation = guideCubeNewRotation;
        // }

    }

    public void SetGuideCubeFaceColor(int face, Color[][] colors)
    {
        for (int i = 0; i < 9; i++)
        {
            SetGuideCubeTileColor(face, i, ColorDetector.AverageColor(colors[i]));
        }
    }

    public void SetGuideCubeTileColor(int face, int tileNumber, Color color)
    {
        guideCubeAllCells[face * 9 + tileNumber].GetComponent<Renderer>().material.color = color;
    }
}
