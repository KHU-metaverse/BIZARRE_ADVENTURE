using UnityEngine;

[ExecuteInEditMode]
public class TextureCombiner : MonoBehaviour
{
    [Header("입력 텍스처")]
    public RenderTexture leftTexture;
    public RenderTexture frontTexture;
    public RenderTexture rightTexture;
    public RenderTexture downTexture;
    
    [Header("출력 텍스처")]
    public RenderTexture combinedTexture;
    
    [Header("머티리얼")]
    public Material combinedMaterial;
    public Material downMaterial;
    
    [Header("디스플레이")]
    public GameObject combinedDisplayPlane;
    public GameObject downDisplayPlane;
    
    [Header("레이어 설정")]
    public int combinedDisplayLayer = 8; // "CombinedDisplay" 레이어
    public int downDisplayLayer = 9;     // "DownDisplay" 레이어
    
    [Header("조정")]
    [Range(0f, 2f)]
    public float brightness = 1.0f;
    [Range(0f, 2f)]
    public float contrast = 1.0f;
    
    [Header("심라인 정합 설정")]
    [Range(0f, 0.2f)]
    public float seamBlendWidth = 0.05f; // 심라인 블렌딩 폭
    [Range(0f, 1f)]
    public float seamBlendStrength = 0.5f; // 블렌딩 강도
    public bool enableSeamCorrection = true; // 심라인 보정 활성화
    
    [Header("자동 업데이트")]
    public bool autoUpdate = true;
    
    // 화면 크기 변경 감지를 위한 변수
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    // 셰이더 속성 ID 캐싱
    private int brightnessID;
    private int contrastID;
    private int seamBlendWidthID;
    private int seamBlendStrengthID;
    private int enableSeamCorrectionID;
    
    private void OnEnable()
    {
        // 초기 화면 크기 저장
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        // 초기 디스플레이 플레인 크기 설정
        UpdateDisplayPlaneToScreenRatio();
        
        // 셰이더 속성 ID 캐싱
        brightnessID = Shader.PropertyToID("_Brightness");
        contrastID = Shader.PropertyToID("_Contrast");
        seamBlendWidthID = Shader.PropertyToID("_SeamBlendWidth");
        seamBlendStrengthID = Shader.PropertyToID("_SeamBlendStrength");
        enableSeamCorrectionID = Shader.PropertyToID("_EnableSeamCorrection");
        
        if (autoUpdate)
        {
            InvokeRepeating("CombineTextures", 0.1f, 0.033f); // 약 30fps로 업데이트
        }
    }
    
    private void OnDisable()
    {
        CancelInvoke("CombineTextures");
    }
    
    private void Update()
    {
        // 머티리얼이 있으면 속성 업데이트
        if (combinedMaterial != null)
        {
            // 기본 속성 업데이트
            combinedMaterial.SetFloat(brightnessID, brightness);
            combinedMaterial.SetFloat(contrastID, contrast);
            
            // 심라인 정합 속성 업데이트
            combinedMaterial.SetFloat(seamBlendWidthID, seamBlendWidth);
            combinedMaterial.SetFloat(seamBlendStrengthID, seamBlendStrength);
            combinedMaterial.SetFloat(enableSeamCorrectionID, enableSeamCorrection ? 1.0f : 0.0f);
        }
        
        // 디스플레이 플레인에 머티리얼 적용
        ApplyMaterialsToDisplays();
        
        // 화면 크기가 변경되었는지 확인
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            // 화면 크기가 변경되었으면 디스플레이 플레인 크기 조정
            UpdateDisplayPlaneToScreenRatio();
            
            // 현재 화면 크기 저장
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            Debug.Log($"화면 크기 변경 감지: {Screen.width}x{Screen.height}, 디스플레이 플레인 크기 조정됨");
        }
    }
    
    public void CombineTextures()
    {
        if (combinedMaterial == null || combinedTexture == null)
        {
            Debug.LogWarning("텍스처 결합을 위한 머티리얼 또는 결합 텍스처가 설정되지 않았습니다.");
            return;
        }
        
        // 입력 텍스처 설정
        if (leftTexture != null)
            combinedMaterial.SetTexture("_LeftTex", leftTexture);
        
        if (frontTexture != null)
            combinedMaterial.SetTexture("_FrontTex", frontTexture);
        
        if (rightTexture != null)
            combinedMaterial.SetTexture("_RightTex", rightTexture);
        
        // 다운 텍스처 설정
        if (downTexture != null && downMaterial != null)
        {
            downMaterial.mainTexture = downTexture;
        }
        
        // 결합된 텍스처 렌더링
        RenderCombinedTexture();
    }
    
    private void RenderCombinedTexture()
    {
        // 현재 렌더 타겟 저장
        RenderTexture currentRT = RenderTexture.active;
        
        // 결합된 텍스처를 렌더 타겟으로 설정
        RenderTexture.active = combinedTexture;
        
        // 화면 지우기
        GL.Clear(true, true, Color.black);
        
        // 전체 화면 쿼드 렌더링
        combinedMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        
        // 쿼드 그리기
        GL.Begin(GL.QUADS);
        
        GL.TexCoord2(0, 0);
        GL.Vertex3(0, 0, 0);
        
        GL.TexCoord2(0, 1);
        GL.Vertex3(0, 1, 0);
        
        GL.TexCoord2(1, 1);
        GL.Vertex3(1, 1, 0);
        
        GL.TexCoord2(1, 0);
        GL.Vertex3(1, 0, 0);
        
        GL.End();
        GL.PopMatrix();
        
        // 원래 렌더 타겟 복원
        RenderTexture.active = currentRT;
    }
    
    private void ApplyMaterialsToDisplays()
    {
        // 결합된 디스플레이에 머티리얼 적용
        if (combinedDisplayPlane != null && combinedMaterial != null)
        {
            Renderer renderer = combinedDisplayPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = combinedMaterial;
                
                // 레이어 설정
                combinedDisplayPlane.layer = combinedDisplayLayer;
            }
        }
        
        // 하단 디스플레이에 머티리얼 적용
        if (downDisplayPlane != null && downMaterial != null)
        {
            Renderer renderer = downDisplayPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = downMaterial;
                
                // 레이어 설정
                downDisplayPlane.layer = downDisplayLayer;
            }
        }
    }
    
    // 화면 비율에 맞게 디스플레이 플레인 크기 조정
    private void UpdateDisplayPlaneToScreenRatio()
    {
        // 결합된 디스플레이 플레인 크기 조정
        if (combinedDisplayPlane != null)
        {
            // 타겟 디스플레이 인덱스 가져오기
            int targetDisplay = 0; // 기본값
            
            // 메인 카메라 찾기
            Camera mainCamera = null;
            Camera[] cameras = Camera.allCameras;
            foreach (Camera cam in cameras)
            {
                // 컬링 마스크로 이 카메라가 combinedDisplayLayer를 렌더링하는지 확인
                if ((cam.cullingMask & (1 << combinedDisplayLayer)) != 0)
                {
                    mainCamera = cam;
                    targetDisplay = cam.targetDisplay;
                    break;
                }
            }
            
            // 해당 디스플레이의 해상도 가져오기
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
            
            // 플레인 스케일 조정
            Vector3 scale = combinedDisplayPlane.transform.localScale;
            scale.x = 9.0f * displayAspect; // 높이가 9일 때 디스플레이 비율에 맞는 가로 길이
            scale.y = 9.0f; // 높이 고정
            combinedDisplayPlane.transform.localScale = scale;
        }
        
        // 하단 디스플레이 플레인 크기 조정
        if (downDisplayPlane != null)
        {
            // 타겟 디스플레이 인덱스 가져오기
            int targetDisplay = 0; // 기본값
            
            // 메인 카메라 찾기
            Camera mainCamera = null;
            Camera[] cameras = Camera.allCameras;
            foreach (Camera cam in cameras)
            {
                // 컬링 마스크로 이 카메라가 downDisplayLayer를 렌더링하는지 확인
                if ((cam.cullingMask & (1 << downDisplayLayer)) != 0)
                {
                    mainCamera = cam;
                    targetDisplay = cam.targetDisplay;
                    break;
                }
            }
            
            // 해당 디스플레이의 해상도 가져오기
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
            
            // 플레인 스케일 조정
            Vector3 scale = downDisplayPlane.transform.localScale;
            scale.x = 9.0f * displayAspect; // 높이가 9일 때 디스플레이 비율에 맞는 가로 길이
            scale.y = 9.0f; // 높이 고정
            downDisplayPlane.transform.localScale = scale;
        }
    }
} 