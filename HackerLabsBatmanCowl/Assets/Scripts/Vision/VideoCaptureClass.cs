using UnityEngine;
using System.Linq;
using System.IO;

public class VideoCaptureClass : MonoBehaviour
{
    static readonly float MaxRecordingTime = 10.0f;

    UnityEngine.XR.WSA.WebCam.VideoCapture m_VideoCapture = null;
    float m_stopRecordingTimer = float.MaxValue;
    string filepath = "";
    public GameObject status = null;
    // Use this for initialization
    void Start()
    {
        status = FindObjectOfType<TextMesh>().gameObject;
        UnityEngine.XR.WSA.WebCam.VideoCapture.CreateAsync(false, OnVideoCaptureCreated);
    }

    void OnVideoCaptureCreated(UnityEngine.XR.WSA.WebCam.VideoCapture videoCapture)
    {
        if (videoCapture != null)
        {
            m_VideoCapture = videoCapture;

            Resolution cameraResolution = UnityEngine.XR.WSA.WebCam.VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            float cameraFramerate = UnityEngine.XR.WSA.WebCam.VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();

            UnityEngine.XR.WSA.WebCam.CameraParameters cameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.frameRate = cameraFramerate;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;

            m_VideoCapture.StartVideoModeAsync(cameraParameters,
                                                UnityEngine.XR.WSA.WebCam.VideoCapture.AudioState.None,
                                                OnStartedVideoCaptureMode);
        }
        else
        {
            Debug.LogError("Failed to create VideoCapture Instance!");
        }
    }

    void OnStartedVideoCaptureMode(UnityEngine.XR.WSA.WebCam.VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format("MyVideo_{0}.mp4", Time.time);
            filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            m_VideoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
        }
    }

    void OnStartedRecordingVideo(UnityEngine.XR.WSA.WebCam.VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Started Recording Video!");
        // We will stop the video from recording via other input such as a timer or a tap, etc.
    }

    void StopRecordingVideo()
    {
        m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
    }

    void OnStoppedRecordingVideo(UnityEngine.XR.WSA.WebCam.VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Stopped Recording Video!");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    void OnStoppedVideoCaptureMode(UnityEngine.XR.WSA.WebCam.VideoCapture.VideoCaptureResult result)
    {
        m_VideoCapture.Dispose();
        m_VideoCapture = null;
        byte[] video = File.ReadAllBytes(filepath);
        status.GetComponent<TextMesh>().text = video.ToString();
        Debug.Log(video.ToString());
    }
}