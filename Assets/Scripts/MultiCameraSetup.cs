using UnityEngine;

[AddComponentMenu("Audio Visualizer/Multi Camera Setup")]
public class MultiCameraSetup : MonoBehaviour
{
    [System.Serializable]
    public class CameraSetup
    {
        public string name;
        public Camera camera;
        public RenderTexture targetTexture;
        public Transform lookAtTarget;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public bool useCustomProjection = false;
        public float fieldOfView = 60f;
        public float nearClipPlane = 0.3f;
        public float farClipPlane = 1000f;
    }
    
    [Header("카메라 설정")]
    public CameraSetup[] cameraSetups;
    
    [Header("렌더링 카메라")]
    public Camera mainCamera1; // 3면(정면, 좌, 우) 담당
    public Camera mainCamera2; // 하단 담당
    
    [Header("렌더 텍스처 설정")]
    public Material frontMaterial;
    public Material leftMaterial;
    public Material rightMaterial;
    public Material downMaterial;
    
    [Header("멀티 디스플레이 설정")]
    public bool useMultipleDisplays = true;
    public int mainCamera1Display = 0;
    public int mainCamera2Display = 1;
    
    void Start()
    {
        // 카메라 설정 초기화
        InitializeCameras();
        
        // 멀티 디스플레이 설정
        SetupMultipleDisplays();
    }
    
    void InitializeCameras()
    {
        foreach (CameraSetup setup in cameraSetups)
        {
            if (setup.camera == null) continue;
            
            // 카메라 위치 및 회전 설정
            if (setup.lookAtTarget != null)
            {
                setup.camera.transform.position = setup.lookAtTarget.position + setup.positionOffset;
                setup.camera.transform.LookAt(setup.lookAtTarget);
                setup.camera.transform.Rotate(setup.rotationOffset);
            }
            
            // 렌더 텍스처 할당
            if (setup.targetTexture != null)
            {
                setup.camera.targetTexture = setup.targetTexture;
            }
            
            // 커스텀 프로젝션 설정
            if (setup.useCustomProjection)
            {
                setup.camera.fieldOfView = setup.fieldOfView;
                setup.camera.nearClipPlane = setup.nearClipPlane;
                setup.camera.farClipPlane = setup.farClipPlane;
            }
        }
        
        // 메인 카메라 설정
        if (mainCamera1 != null)
        {
            mainCamera1.clearFlags = CameraClearFlags.SolidColor;
            mainCamera1.backgroundColor = Color.black;
        }
        
        if (mainCamera2 != null)
        {
            mainCamera2.clearFlags = CameraClearFlags.SolidColor;
            mainCamera2.backgroundColor = Color.black;
        }
        
        // 렌더 텍스처를 머티리얼에 할당
        AssignRenderTexturesToMaterials();
    }
    
    void AssignRenderTexturesToMaterials()
    {
        foreach (CameraSetup setup in cameraSetups)
        {
            if (setup.targetTexture == null) continue;
            
            // 카메라 이름에 따라 적절한 머티리얼에 렌더 텍스처 할당
            if (setup.name.ToLower().Contains("front") && frontMaterial != null)
            {
                frontMaterial.mainTexture = setup.targetTexture;
            }
            else if (setup.name.ToLower().Contains("left") && leftMaterial != null)
            {
                leftMaterial.mainTexture = setup.targetTexture;
            }
            else if (setup.name.ToLower().Contains("right") && rightMaterial != null)
            {
                rightMaterial.mainTexture = setup.targetTexture;
            }
            else if (setup.name.ToLower().Contains("down") && downMaterial != null)
            {
                downMaterial.mainTexture = setup.targetTexture;
            }
        }
    }
    
    void SetupMultipleDisplays()
    {
        if (!useMultipleDisplays) return;
        
        // 사용 가능한 디스플레이 수 확인
        if (Display.displays.Length > 1)
        {
            // 두 번째 디스플레이 활성화
            Display.displays[1].Activate();
            
            Debug.Log($"멀티 디스플레이 활성화: {Display.displays.Length}개 감지됨");
            
            // 메인 카메라 디스플레이 설정
            if (mainCamera1 != null)
            {
                mainCamera1.targetDisplay = mainCamera1Display;
            }
            
            if (mainCamera2 != null)
            {
                mainCamera2.targetDisplay = mainCamera2Display;
            }
        }
        else
        {
            Debug.LogWarning("멀티 디스플레이를 사용할 수 없습니다. 디스플레이가 하나만 감지되었습니다.");
            useMultipleDisplays = false;
        }
    }
    
    // 에디터에서 설정 확인
    void OnValidate()
    {
        if (mainCamera1Display == mainCamera2Display)
        {
            Debug.LogWarning("두 메인 카메라가 같은 디스플레이를 사용하도록 설정되어 있습니다.");
        }
        
        // 카메라 설정 이름 자동 설정
        if (cameraSetups != null)
        {
            for (int i = 0; i < cameraSetups.Length; i++)
            {
                if (string.IsNullOrEmpty(cameraSetups[i].name))
                {
                    cameraSetups[i].name = $"Camera {i+1}";
                }
            }
        }
    }
    
    // 에디터에서 시각화
    void OnDrawGizmosSelected()
    {
        foreach (CameraSetup setup in cameraSetups)
        {
            if (setup.camera == null || setup.lookAtTarget == null) continue;
            
            Gizmos.color = Color.yellow;
            Vector3 cameraPosition = setup.lookAtTarget.position + setup.positionOffset;
            Gizmos.DrawLine(cameraPosition, setup.lookAtTarget.position);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraPosition, 0.1f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(setup.lookAtTarget.position, 0.1f);
        }
    }
} 