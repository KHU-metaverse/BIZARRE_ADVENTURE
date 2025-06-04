// 이 스크립트를 빈 게임 오브젝트에 추가하고 4개의 평면 오브젝트 생성
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProjectionSetup : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera frontCamera;
    public Camera leftCamera;
    public Camera rightCamera;
    public Camera downCamera;
    
    [Header("카메라 정합 설정")]
    [Range(60f, 120f)]
    public float cameraFOV = 90f; // 90도 FOV로 정확한 정합을 위한 기본값
    [Range(0f, 1f)]
    public float cameraOverlap = 0.05f; // 카메라 간 오버랩 비율 (경계 부분 블렌딩용)
    
    [Header("렌더 텍스처")]
    public RenderTexture frontTexture;
    public RenderTexture leftTexture;
    public RenderTexture rightTexture;
    public RenderTexture downTexture;
    
    [Header("메인 카메라")]
    public Camera mainCamera1; // 3면(정면, 좌, 우) 담당
    public Camera mainCamera2; // 하단 담당
    
    [Header("멀티 모니터 설정")]
    public bool useMultipleDisplays = true;
    public int mainCamera1Display = 0; // 첫 번째 모니터
    public int mainCamera2Display = 1; // 두 번째 모니터
    
    [Header("텍스처 결합")]
    public bool useCombinedTexture = true;
    public RenderTexture combinedTexture; // 3면 합친 텍스처
    public Material combinedMaterial; // 3면 합친 텍스처를 표시할 머티리얼
    public Material downMaterial; // 하단 텍스처를 표시할 머티리얼
    
    [Header("방 설정")]
    public Vector3 roomSize = new Vector3(4, 3, 4); // 방 크기 (너비, 높이, 길이)
    
    [Header("벽 오브젝트")]
    public GameObject frontWall;
    public GameObject leftWall;
    public GameObject rightWall;
    public GameObject backWall;
    public GameObject floor;
    
    private TextureCombiner textureCombiner;
    
    // 화면 크기 변경 감지를 위한 변수
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    void Start()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (useCombinedTexture)
        {
            SetupTextureCombiner();
        }

        SetupCamerasForSeamlessProjection();

        AssignRenderTextures();

        SetupMultipleDisplays();

        // 추가된 부분: PNG 파일 로드 및 할당
        // 파일 경로를 실제 Assets 폴더 내 경로로 수정해야 합니다.
        LoadAndApplyPNGTextures(
            "Assets/Textures/Attic/front.png", // 정면 이미지 경로
            "Assets/Textures/Attic/left.png",  // 좌측 이미지 경로
            "Assets/Textures/Attic/right.png", // 우측 이미지 경로
            "Assets/Textures/Attic/down.png"   // 하단 이미지 경로
        );

        AssignTexturesToWalls();
    }
    
    // 카메라 정합을 위한 설정
    void SetupCamerasForSeamlessProjection()
    {
        if (frontCamera == null || leftCamera == null || rightCamera == null || downCamera == null)
            return;

        Vector3 roomCenter = Vector3.zero;

        SetupBaseCamera(frontCamera);
        SetupBaseCamera(leftCamera);
        SetupBaseCamera(rightCamera);
        SetupBaseCamera(downCamera);

        frontCamera.transform.position = roomCenter;
        leftCamera.transform.position = roomCenter;
        rightCamera.transform.position = roomCenter;
        downCamera.transform.position = roomCenter;

        frontCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        rightCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
        leftCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
        downCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

        frontCamera.fieldOfView = cameraFOV;
        leftCamera.fieldOfView = cameraFOV;
        rightCamera.fieldOfView = cameraFOV;
        downCamera.fieldOfView = cameraFOV;

        float halfFOVRad = Mathf.Deg2Rad * cameraFOV / 2;

        float frontAspect = roomSize.x / roomSize.y;
        float sideAspect = roomSize.z / roomSize.y;
        float downAspect = roomSize.x / roomSize.z;

        frontCamera.aspect = frontAspect;
        leftCamera.aspect = sideAspect;
        rightCamera.aspect = sideAspect;
        downCamera.aspect = downAspect;

        Debug.Log($"카메라가 심라인 정합을 위해 설정되었습니다. FOV: {cameraFOV}, Front Aspect: {frontAspect}, Side Aspect: {sideAspect}, Down Aspect: {downAspect}");
    }
    
    // 기본 카메라 설정
    void SetupBaseCamera(Camera camera)
    {
        if (camera == null) return;
        
        // 클리핑 플레인 설정
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 1000f;
        
        // 투영 행렬 설정
        camera.projectionMatrix = Matrix4x4.Perspective(cameraFOV, 1.0f, 0.01f, 1000f);
        
        // 카메라 클리어 플래그 설정
        // camera.clearFlags = CameraClearFlags.SolidColor;
        // camera.backgroundColor = Color.black;
    }
    
    void Update()
    {
        // 화면 크기가 변경되었는지 확인
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            // 화면 크기가 변경되었으면 디스플레이 플레인 크기 조정
            if (textureCombiner != null && textureCombiner.combinedDisplayPlane != null && textureCombiner.downDisplayPlane != null)
            {
                OptimizeMainCameras(textureCombiner.combinedDisplayPlane, textureCombiner.downDisplayPlane);
                
                // 현재 화면 크기 저장
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                
                Debug.Log($"ProjectionSetup: 화면 크기 변경 감지: {Screen.width}x{Screen.height}, 디스플레이 플레인 크기 조정됨");
            }
        }
    }
    
    void AssignRenderTextures()
    {
        if (frontCamera && frontTexture) frontCamera.targetTexture = frontTexture;
        if (leftCamera && leftTexture) leftCamera.targetTexture = leftTexture;
        if (rightCamera && rightTexture) rightCamera.targetTexture = rightTexture;
        if (downCamera && downTexture) downCamera.targetTexture = downTexture;
    }
    
    void AssignTexturesToWalls()
    {
        AssignTextureToWall(frontWall, frontTexture);
        AssignTextureToWall(leftWall, leftTexture);
        AssignTextureToWall(rightWall, rightTexture);
        AssignTextureToWall(floor, downTexture);
    }
    
    void AssignTextureToWall(GameObject wall, RenderTexture texture)
    {
        if (wall == null || texture == null) return;
        
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Unlit/Texture"));
            }
            
            renderer.material.mainTexture = texture;
        }
    }
    
    void SetupTextureCombiner()
    {
        // 이미 있는 TextureCombiner 컴포넌트 확인
        textureCombiner = GetComponent<TextureCombiner>();
        
        // 없으면 새로 추가
        if (textureCombiner == null)
        {
            textureCombiner = gameObject.AddComponent<TextureCombiner>();
        }
        
        // TextureCombiner 설정
        textureCombiner.leftTexture = leftTexture;
        textureCombiner.frontTexture = frontTexture;
        textureCombiner.rightTexture = rightTexture;
        textureCombiner.downTexture = downTexture;
        textureCombiner.combinedTexture = combinedTexture;
        
        // 머티리얼 설정
        textureCombiner.combinedMaterial = combinedMaterial;
        textureCombiner.downMaterial = downMaterial;
        
        // 레이어 이름 확인 및 생성
        CheckAndCreateLayer("CombinedDisplay", textureCombiner.combinedDisplayLayer);
        CheckAndCreateLayer("DownDisplay", textureCombiner.downDisplayLayer);
        
        // 디스플레이 플레인 생성 - 메인 카메라 앞에 위치하도록 수정
        GameObject combinedPlane = CreateDisplayPlane("CombinedDisplayPlane", 
            mainCamera1.transform.position + mainCamera1.transform.forward * 5, 
            Quaternion.Euler(0, 180, 0), // 180도 회전하여 카메라를 향하도록 함
            new Vector3(16, 9, 1));
            
        GameObject downPlane = CreateDisplayPlane("DownDisplayPlane", 
            mainCamera2.transform.position + mainCamera2.transform.forward * 5, 
            Quaternion.Euler(0, 180, 0), // 180도 회전하여 카메라를 향하도록 함
            new Vector3(16, 9, 1));
            
        // 디스플레이 플레인 할당
        textureCombiner.combinedDisplayPlane = combinedPlane;
        textureCombiner.downDisplayPlane = downPlane;
        
        // 디스플레이 플레인 레이어 설정
        combinedPlane.layer = textureCombiner.combinedDisplayLayer;
        downPlane.layer = textureCombiner.downDisplayLayer;
        
        // 자동 업데이트 활성화
        textureCombiner.autoUpdate = true;
        
        // 메인 카메라 레이어 설정
        SetupMainCameraLayers();
        
        // 메인 카메라 설정 최적화
        OptimizeMainCameras(combinedPlane, downPlane);
    }
    
    // 메인 카메라 최적화 설정
    private void OptimizeMainCameras(GameObject combinedPlane, GameObject downPlane)
    {
        if (mainCamera1 != null && combinedPlane != null)
        {
            // 메인 카메라 1 설정 최적화
            mainCamera1.orthographic = true;
            
            // 디스플레이 플레인 크기를 화면 비율에 맞게 스트레치
            Renderer renderer = combinedPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 카메라의 orthographicSize를 고정값으로 설정
                mainCamera1.orthographicSize = 4.5f;
                
                // 타겟 디스플레이의 해상도 가져오기
                int targetDisplay = mainCamera1.targetDisplay;
                float displayAspect;
                
                if (targetDisplay < Display.displays.Length && Display.displays[targetDisplay].active)
                {
                    displayAspect = (float)Display.displays[targetDisplay].systemWidth / Display.displays[targetDisplay].systemHeight;
                    Debug.Log($"디스플레이 {targetDisplay}의 비율: {displayAspect} ({Display.displays[targetDisplay].systemWidth}x{Display.displays[targetDisplay].systemHeight})");
                }
                else
                {
                    // 활성화된 디스플레이가 없거나 인덱스가 범위를 벗어나면 메인 화면 비율 사용
                    displayAspect = (float)Screen.width / Screen.height;
                    Debug.Log($"메인 화면 비율 사용: {displayAspect} ({Screen.width}x{Screen.height})");
                }
                
                // 플레인 스케일 조정 - 디스플레이 비율에 맞게 스트레치
                Vector3 scale = combinedPlane.transform.localScale;
                scale.x = 9.0f * displayAspect; // 높이가 9일 때 디스플레이 비율에 맞는 가로 길이
                scale.y = 9.0f; // 높이 고정
                combinedPlane.transform.localScale = scale;
            }
            else
            {
                mainCamera1.orthographicSize = 4.5f; // 기본값 (16:9 비율 기준)
            }
        }
        
        if (mainCamera2 != null && downPlane != null)
        {
            // 메인 카메라 2 설정 최적화
            mainCamera2.orthographic = true;
            
            // 디스플레이 플레인 크기를 화면 비율에 맞게 스트레치
            Renderer renderer = downPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 카메라의 orthographicSize를 고정값으로 설정
                mainCamera2.orthographicSize = 4.5f;
                
                // 타겟 디스플레이의 해상도 가져오기
                int targetDisplay = mainCamera2.targetDisplay;
                float displayAspect;
                
                if (targetDisplay < Display.displays.Length && Display.displays[targetDisplay].active)
                {
                    displayAspect = (float)Display.displays[targetDisplay].systemWidth / Display.displays[targetDisplay].systemHeight;
                    Debug.Log($"디스플레이 {targetDisplay}의 비율: {displayAspect} ({Display.displays[targetDisplay].systemWidth}x{Display.displays[targetDisplay].systemHeight})");
                }
                else
                {
                    // 활성화된 디스플레이가 없거나 인덱스가 범위를 벗어나면 메인 화면 비율 사용
                    displayAspect = (float)Screen.width / Screen.height;
                    Debug.Log($"메인 화면 비율 사용: {displayAspect} ({Screen.width}x{Screen.height})");
                }
                
                // 플레인 스케일 조정 - 디스플레이 비율에 맞게 스트레치
                Vector3 scale = downPlane.transform.localScale;
                scale.x = 9.0f * displayAspect; // 높이가 9일 때 디스플레이 비율에 맞는 가로 길이
                scale.y = 9.0f; // 높이 고정
                downPlane.transform.localScale = scale;
            }
            else
            {
                mainCamera2.orthographicSize = 4.5f; // 기본값 (16:9 비율 기준)
            }
        }
    }
    
    // 레이어 확인 및 생성 (에디터에서만 작동)
    private void CheckAndCreateLayer(string layerName, int layerIndex)
    {
#if UNITY_EDITOR
        // 레이어가 이미 존재하는지 확인
        SerializedObject tagManager = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        // 레이어 인덱스가 유효한지 확인
        if (layerIndex >= 8 && layerIndex <= 31)
        {
            SerializedProperty layerProp = layers.GetArrayElementAtIndex(layerIndex);
            
            // 레이어가 비어있으면 새 레이어 이름 설정
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"레이어 생성됨: {layerName} (인덱스: {layerIndex})");
            }
            else if (layerProp.stringValue != layerName)
            {
                Debug.LogWarning($"레이어 인덱스 {layerIndex}에 이미 다른 레이어가 있습니다: {layerProp.stringValue}");
            }
        }
        else
        {
            Debug.LogError($"유효하지 않은 레이어 인덱스: {layerIndex}. 8-31 사이의 값이어야 합니다.");
        }
#endif
    }
    
    // 메인 카메라 레이어 설정
    private void SetupMainCameraLayers()
    {
        if (mainCamera1 != null && textureCombiner != null)
        {
            // 메인 카메라 1은 결합된 디스플레이 레이어와 디버그 레이어를 렌더링
            mainCamera1.cullingMask = (1 << textureCombiner.combinedDisplayLayer) | (1 << LayerMask.NameToLayer("Default"));
            mainCamera1.clearFlags = CameraClearFlags.SolidColor;
            mainCamera1.backgroundColor = Color.black;
        }
        
        if (mainCamera2 != null && textureCombiner != null)
        {
            // 메인 카메라 2는 하단 디스플레이 레이어와 디버그 레이어를 렌더링
            mainCamera2.cullingMask = (1 << textureCombiner.downDisplayLayer) | (1 << LayerMask.NameToLayer("Default"));
            mainCamera2.clearFlags = CameraClearFlags.SolidColor;
            mainCamera2.backgroundColor = Color.black;
        }
    }
    
    // 디스플레이 플레인 생성 헬퍼 메서드
    private GameObject CreateDisplayPlane(string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = name;
        plane.transform.position = position;
        plane.transform.rotation = rotation;
        plane.transform.localScale = scale;
        return plane;
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
            if (mainCamera1)
            {
                mainCamera1.targetDisplay = mainCamera1Display;
            }
            
            if (mainCamera2)
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
        if (Application.isPlaying)
        {
            SetupCamerasForSeamlessProjection();
        }
        
        if (mainCamera1Display == mainCamera2Display)
        {
            Debug.LogWarning("두 메인 카메라가 같은 디스플레이를 사용하도록 설정되어 있습니다.");
        }
    }
    
#if UNITY_EDITOR
    // 에디터에서 방 시각화
    private void OnDrawGizmos()
    {
        // 방 크기 시각화
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(Vector3.zero, roomSize);
        
        // 벽 위치 시각화
        Vector3 frontPos = new Vector3(0, 0, roomSize.z/2);
        Vector3 rightPos = new Vector3(roomSize.x/2, 0, 0);
        Vector3 backPos = new Vector3(0, 0, -roomSize.z/2);
        Vector3 leftPos = new Vector3(-roomSize.x/2, 0, 0);
        Vector3 floorPos = new Vector3(0, -roomSize.y/2, 0);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(frontPos, new Vector3(roomSize.x, roomSize.y, 0.1f));
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(rightPos, new Vector3(0.1f, roomSize.y, roomSize.z));
        
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(backPos, new Vector3(roomSize.x, roomSize.y, 0.1f));
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(leftPos, new Vector3(0.1f, roomSize.y, roomSize.z));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(floorPos, new Vector3(roomSize.x, 0.1f, roomSize.z));
        
        // 카메라 방향 시각화
        if (frontCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(frontCamera.transform.position, frontCamera.transform.forward * roomSize.z);
        }
        
        if (leftCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftCamera.transform.position, leftCamera.transform.forward * roomSize.x);
        }
        
        if (rightCamera != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rightCamera.transform.position, rightCamera.transform.forward * roomSize.x);
        }
        
        if (downCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(downCamera.transform.position, downCamera.transform.forward * roomSize.y);
        }
    }
    
    // 에디터에서 벽 생성 메뉴
    [ContextMenu("벽 오브젝트 생성")]
    public void CreateWallObjects()
    {
        // 정면 벽 생성 (Z축 양의 방향)
        if (frontWall == null)
            frontWall = CreateWall("Wall_Front", new Vector3(0, 0, roomSize.z/2), Quaternion.identity, new Vector3(roomSize.x, roomSize.y, 0.01f));
        
        // 우측 벽 생성 (X축 양의 방향)
        if (rightWall == null)
            rightWall = CreateWall("Wall_Right", new Vector3(roomSize.x/2, 0, 0), Quaternion.Euler(0, 90, 0), new Vector3(roomSize.z, roomSize.y, 0.01f));
        
        // 후면 벽 생성 (Z축 음의 방향)
        if (backWall == null)
            backWall = CreateWall("Wall_Back", new Vector3(0, 0, -roomSize.z/2), Quaternion.Euler(0, 180, 0), new Vector3(roomSize.x, roomSize.y, 0.01f));
        
        // 좌측 벽 생성 (X축 음의 방향)
        if (leftWall == null)
            leftWall = CreateWall("Wall_Left", new Vector3(-roomSize.x/2, 0, 0), Quaternion.Euler(0, -90, 0), new Vector3(roomSize.z, roomSize.y, 0.01f));
        
        // 바닥 생성 (Y축 음의 방향)
        if (floor == null)
            floor = CreateWall("Floor", new Vector3(0, -roomSize.y/2, 0), Quaternion.Euler(90, 0, 0), new Vector3(roomSize.x, roomSize.z, 0.01f));
        
        // 씬을 더티로 표시
        EditorUtility.SetDirty(this);
    }
    
    // 에디터에서 벽 생성 헬퍼 메서드
    private GameObject CreateWall(string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.rotation = rotation;
        wall.transform.localScale = scale;
        
        // 각 벽에 대해 별도의 머티리얼 생성
        Renderer renderer = wall.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Unlit/Texture"));
        
        return wall;
    }
#endif

    // PNG 텍스처 로드 및 적용을 위한 메서드
    public void LoadAndApplyPNGTexture(string pngPath, Camera targetCamera)
    {
        if (string.IsNullOrEmpty(pngPath) || targetCamera == null) return;

        // PNG 파일을 텍스처로 로드
        byte[] fileData = System.IO.File.ReadAllBytes(pngPath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        // 렌더 텍스처 생성
        RenderTexture rt = new RenderTexture(tex.width, tex.height, 24);
        rt.Create();

        // 텍스처를 렌더 텍스처에 복사
        Graphics.Blit(tex, rt);

        // 카메라에 텍스처 할당
        if (targetCamera == frontCamera)
        {
            frontTexture = rt;
            AssignTextureToWall(frontWall, rt);
        }
        else if (targetCamera == leftCamera)
        {
            leftTexture = rt;
            AssignTextureToWall(leftWall, rt);
        }
        else if (targetCamera == rightCamera)
        {
            rightTexture = rt;
            AssignTextureToWall(rightWall, rt);
        }
        else if (targetCamera == downCamera)
        {
            downTexture = rt;
            AssignTextureToWall(floor, rt);
        }

        // 원본 텍스처 해제
        Destroy(tex);

        // TextureCombiner가 있다면 업데이트
        if (textureCombiner != null)
        {
            textureCombiner.leftTexture = leftTexture;
            textureCombiner.frontTexture = frontTexture;
            textureCombiner.rightTexture = rightTexture;
            textureCombiner.downTexture = downTexture;
            // TextureCombiner의 업데이트 로직이 있다면 여기서 호출
            // 예: textureCombiner.UpdateCombinedTexture();
        }
    }

    // 모든 카메라에 PNG 텍스처 적용
    public void LoadAndApplyPNGTextures(string frontPath, string leftPath, string rightPath, string downPath)
    {
        if (!string.IsNullOrEmpty(frontPath)) LoadAndApplyPNGTexture(frontPath, frontCamera);
        if (!string.IsNullOrEmpty(leftPath)) LoadAndApplyPNGTexture(leftPath, leftCamera);
        if (!string.IsNullOrEmpty(rightPath)) LoadAndApplyPNGTexture(rightPath, rightCamera);
        if (!string.IsNullOrEmpty(downPath)) LoadAndApplyPNGTexture(downPath, downCamera);
    }
}