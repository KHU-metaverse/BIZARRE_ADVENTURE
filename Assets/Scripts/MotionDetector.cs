using UnityEngine;

public class MotionDetector : MonoBehaviour
{
    public int webcamWidth = 640;
    public int webcamHeight = 480;
    public int frameRate = 15;
    
    private WebCamTexture webcamTexture;
    private Color32[] currentFrame;
    private Color32[] previousFrame;
    private bool isFirstFrame = true;
    
    [HideInInspector] public Vector2 motionCenter = Vector2.zero;
    [HideInInspector] public float motionAmount = 0f;
    
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webcamTexture = new WebCamTexture(devices[0].name, webcamWidth, webcamHeight, frameRate);
            webcamTexture.Play();
            
            currentFrame = new Color32[webcamWidth * webcamHeight];
            previousFrame = new Color32[webcamWidth * webcamHeight];
        }
        else
        {
            Debug.LogError("No webcam found");
        }
    }
    
    void Update()
    {
        if (webcamTexture != null && webcamTexture.isPlaying && webcamTexture.didUpdateThisFrame)
        {
            // 현재 프레임 저장
            previousFrame = currentFrame;
            currentFrame = webcamTexture.GetPixels32();
            
            if (!isFirstFrame)
            {
                DetectMotion();
            }
            else
            {
                isFirstFrame = false;
            }
        }
    }
    
    void DetectMotion()
    {
        int diffPixels = 0;
        float totalX = 0f;
        float totalY = 0f;
        float motionThreshold = 30f; // 움직임으로 간주할 픽셀 값 차이 임계값
        
        for (int y = 0; y < webcamHeight; y++)
        {
            for (int x = 0; x < webcamWidth; x++)
            {
                int index = y * webcamWidth + x;
                
                if (index < currentFrame.Length && index < previousFrame.Length)
                {
                    // RGB 차이 계산
                    int diffR = Mathf.Abs(currentFrame[index].r - previousFrame[index].r);
                    int diffG = Mathf.Abs(currentFrame[index].g - previousFrame[index].g);
                    int diffB = Mathf.Abs(currentFrame[index].b - previousFrame[index].b);
                    
                    int totalDiff = diffR + diffG + diffB;
                    
                    if (totalDiff > motionThreshold)
                    {
                        diffPixels++;
                        totalX += x;
                        totalY += y;
                    }
                }
            }
        }
        
        if (diffPixels > 0)
        {
            // 움직임 중심점 계산 (0-1 범위로 정규화)
            motionCenter = new Vector2(
                totalX / diffPixels / webcamWidth,
                totalY / diffPixels / webcamHeight
            );
            
            // 움직임 양 계산 (0-1 범위로 정규화)
            motionAmount = Mathf.Clamp01((float)diffPixels / (webcamWidth * webcamHeight) * 10f);
        }
        else
        {
            motionAmount = 0f;
        }
    }
}