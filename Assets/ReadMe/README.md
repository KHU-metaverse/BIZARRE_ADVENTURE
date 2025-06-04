# 프로젝션 매핑 시스템 개선 가이드

이 문서는 프로젝션 매핑 시스템의 개선 사항과 설정 방법에 대해 설명합니다.

## 1. 멀티 카메라 설정

### 새로운 기능
- 3면 카메라(정면, 좌, 우)와 하단 카메라 설정
- 각 카메라가 렌더 텍스처에 화면을 캡처
- 메인 카메라 2대가 각각 3면과 하단을 담당
- 멀티 모니터 지원 (1번 모니터: 3면, 2번 모니터: 하단)

### 설정 방법
1. 씬에 빈 게임 오브젝트를 생성하고 `MultiCameraSetup` 컴포넌트를 추가합니다.
2. 각 면을 담당할 카메라를 생성하고 `CameraSetup` 배열에 추가합니다.
3. 각 카메라에 대한 렌더 텍스처를 할당합니다.
4. 메인 카메라 2대를 설정하고 각각 `mainCamera1`과 `mainCamera2`에 할당합니다.
5. 멀티 디스플레이 설정을 필요에 따라 조정합니다.

```csharp
// 예시 설정
MultiCameraSetup cameraSetup = gameObject.AddComponent<MultiCameraSetup>();

// 카메라 설정
cameraSetup.cameraSetups = new MultiCameraSetup.CameraSetup[4];
cameraSetup.cameraSetups[0] = new MultiCameraSetup.CameraSetup { name = "Front Camera", ... };
cameraSetup.cameraSetups[1] = new MultiCameraSetup.CameraSetup { name = "Left Camera", ... };
cameraSetup.cameraSetups[2] = new MultiCameraSetup.CameraSetup { name = "Right Camera", ... };
cameraSetup.cameraSetups[3] = new MultiCameraSetup.CameraSetup { name = "Down Camera", ... };

// 메인 카메라 설정
cameraSetup.mainCamera1 = mainCamera1;
cameraSetup.mainCamera2 = mainCamera2;
```

## 2. 음악 반응형 파티클 시스템

### 새로운 기능
- 향상된 오디오 분석 (8개 주파수 대역)
- 고급 비트 감지 알고리즘
- 주파수 대역별 파티클 반응
- 비트에 따른 특수 효과
- 다양한 파티클 모듈레이션 (크기, 속도, 색상, 모양)

### 설정 방법
1. 씬에 빈 게임 오브젝트를 생성하고 `MusicReactiveParticleSystem` 컴포넌트를 추가합니다.
2. 오디오 소스와 `AudioAnalyzer` 컴포넌트를 설정합니다.
3. 메인 파티클 시스템과 버스트 파티클 시스템을 할당합니다.
4. 반응성 설정과 색상 그라디언트를 조정합니다.

```csharp
// 예시 설정
MusicReactiveParticleSystem particleSystem = gameObject.AddComponent<MusicReactiveParticleSystem>();
particleSystem.audioAnalyzer = GetComponent<AudioAnalyzer>();
particleSystem.mainParticleSystem = GetComponent<ParticleSystem>();
```

## 3. 오디오 분석기 개선

### 새로운 기능
- 8개 주파수 대역 분석
- 에너지 히스토리 기반 비트 감지
- 주파수 대역별 값 계산 (저주파, 저중주파, 중주파, 고중주파, 고주파)
- 디버그 시각화 도구

### 설정 방법
1. 씬에 빈 게임 오브젝트를 생성하고 `AudioAnalyzer` 컴포넌트를 추가합니다.
2. 오디오 소스를 할당합니다.
3. 비트 감지 설정을 조정합니다.
4. 필요에 따라 디버그 옵션을 활성화합니다.

```csharp
// 예시 설정
AudioAnalyzer analyzer = gameObject.AddComponent<AudioAnalyzer>();
analyzer.audioSource = GetComponent<AudioSource>();
analyzer.beatThreshold = 0.6f;
analyzer.useEnergyHistory = true;
```

## 4. 프로젝션 설정 개선

### 새로운 기능
- 멀티 디스플레이 지원
- 렌더 텍스처 자동 할당
- 카메라 디스플레이 설정

### 설정 방법
1. 씬에 빈 게임 오브젝트를 생성하고 `ProjectionSetup` 컴포넌트를 추가합니다.
2. 각 면에 대한 카메라와 렌더 텍스처를 할당합니다.
3. 메인 카메라를 설정합니다.
4. 멀티 디스플레이 설정을 조정합니다.

```csharp
// 예시 설정
ProjectionSetup setup = gameObject.AddComponent<ProjectionSetup>();
setup.frontCamera = frontCamera;
setup.leftCamera = leftCamera;
setup.rightCamera = rightCamera;
setup.downCamera = downCamera;
setup.mainCamera1 = mainCamera1;
setup.mainCamera2 = mainCamera2;
```

## 5. 씬 설정 예시

1. 빈 게임 오브젝트 "AudioManager"를 생성하고 다음 컴포넌트를 추가합니다:
   - AudioSource (음악 파일 할당)
   - AudioAnalyzer
   - AudioReactiveParticles

2. 빈 게임 오브젝트 "CameraManager"를 생성하고 다음 컴포넌트를 추가합니다:
   - MultiCameraSetup
   - ProjectionSetup

3. 각 면에 대한 카메라 오브젝트를 생성합니다:
   - FrontCamera
   - LeftCamera
   - RightCamera
   - DownCamera

4. 메인 카메라 2대를 생성합니다:
   - MainCamera1 (3면 담당)
   - MainCamera2 (하단 담당)

5. 파티클 시스템 오브젝트를 생성하고 다음 컴포넌트를 추가합니다:
   - ParticleSystem
   - MusicReactiveParticleSystem

## 6. 텍스처 결합 시스템

### 새로운 기능
- 3면 카메라(좌, 정면, 우)의 렌더 텍스처를 하나로 결합
- 하단 카메라의 렌더 텍스처 별도 처리
- 커스텀 셰이더를 통한 텍스처 결합
- 밝기 및 대비 조정 기능
- 에디터 도구를 통한 쉬운 설정
- 텍스처 비율에 맞는 자동 디스플레이 크기 조정
- 레이어 기반 카메라 렌더링 최적화

### 에디터에서 설정하기
1. **메뉴를 통한 설정**:
   - `AudioVisualizer/프로젝션 매핑/새 프로젝션 설정 생성` 메뉴를 선택하여 새 프로젝션 설정을 생성합니다.
   - 새 씬을 생성할지 또는 현재 씬에 추가할지 선택할 수 있습니다.
   - 자동으로 카메라와 메인 카메라가 생성됩니다.

2. **ProjectionSetup 컴포넌트 설정**:
   - 인스펙터에서 카메라, 렌더 텍스처, 메인 카메라 등을 설정합니다.
   - "렌더 텍스처 생성" 버튼을 클릭하여 필요한 렌더 텍스처를 자동으로 생성합니다.
   - "머티리얼 생성" 버튼을 클릭하여 필요한 머티리얼을 자동으로 생성합니다.
   - "전체 설정 적용" 버튼을 클릭하여 모든 설정을 한 번에 적용합니다.
   - 컨텍스트 메뉴(오른쪽 클릭)에서 "벽 오브젝트 생성"을 선택하여 방 벽을 생성합니다.

3. **TextureCombiner 컴포넌트 설정**:
   - `AudioVisualizer/프로젝션 매핑/텍스처 결합기 추가` 메뉴를 선택하여 텍스처 결합기를 추가합니다.
   - 인스펙터에서 입력 텍스처, 출력 텍스처, 머티리얼 등을 설정합니다.
   - "머티리얼 생성" 버튼을 클릭하여 필요한 머티리얼을 자동으로 생성합니다.
   - "렌더 텍스처 생성" 버튼을 클릭하여 필요한 렌더 텍스처를 자동으로 생성합니다.
   - "지금 텍스처 결합" 버튼을 클릭하여 텍스처 결합을 테스트합니다.

4. **레이어 설정**:
   - `AudioVisualizer/프로젝션 매핑/레이어 설정` 메뉴를 선택하여 필요한 레이어를 자동으로 생성합니다.
   - "CombinedDisplay"와 "DownDisplay" 레이어가 자동으로 생성됩니다.

### 설정 방법 (수동)
1. 씬에 빈 게임 오브젝트를 생성하고 `ProjectionSetup` 컴포넌트를 추가합니다.
2. 4개의 카메라(정면, 좌, 우, 하단)를 생성하고 `ProjectionSetup` 컴포넌트에 할당합니다.
3. 2개의 메인 카메라를 생성하고 `ProjectionSetup` 컴포넌트에 할당합니다.
4. 인스펙터에서 "렌더 텍스처 생성" 버튼을 클릭하여 렌더 텍스처를 생성합니다.
5. 인스펙터에서 "머티리얼 생성" 버튼을 클릭하여 머티리얼을 생성합니다.
6. 인스펙터에서 "전체 설정 적용" 버튼을 클릭하여 모든 설정을 적용합니다.
7. 컨텍스트 메뉴에서 "벽 오브젝트 생성"을 선택하여 방 벽을 생성합니다.

```csharp
// 예시 설정
// 이 코드는 직접 입력할 필요가 없습니다. 에디터 도구를 통해 자동으로 설정됩니다.
TextureCombiner combiner = gameObject.AddComponent<TextureCombiner>();

// 렌더 텍스처 할당
combiner.leftTexture = leftCamera.targetTexture;
combiner.frontTexture = frontCamera.targetTexture;
combiner.rightTexture = rightCamera.targetTexture;
combiner.downTexture = downCamera.targetTexture;

// 디스플레이 플레인 할당
combiner.combinedDisplayPlane = combinedQuad;
combiner.downDisplayPlane = downQuad;

// 밝기 및 대비 조정
combiner.brightness = 1.2f;
combiner.contrast = 1.1f;

// 레이어 설정
combiner.combinedDisplayLayer = 8; // "CombinedDisplay" 레이어
combiner.downDisplayLayer = 9;     // "DownDisplay" 레이어

// 메인 카메라 설정
mainCamera1.cullingMask = 1 << combiner.combinedDisplayLayer;
mainCamera2.cullingMask = 1 << combiner.downDisplayLayer;
```

### 셰이더 정보
- `CombinedTexture.shader`: 3개의 텍스처를 하나로 결합하는 커스텀 셰이더
- UV 좌표를 기반으로 각 텍스처 영역 결정 (좌측 1/3, 중앙 1/3, 우측 1/3)
- 밝기 및 대비 조정 기능 내장
- 텍스처 경계에서의 부드러운 블렌딩

### 레이어 설정
- "CombinedDisplay" 레이어(기본값: 8): 결합된 텍스처를 표시하는 디스플레이 플레인에 사용
- "DownDisplay" 레이어(기본값: 9): 하단 텍스처를 표시하는 디스플레이 플레인에 사용
- 메인 카메라 1은 "CombinedDisplay" 레이어만 렌더링
- 메인 카메라 2는 "DownDisplay" 레이어만 렌더링
- 이를 통해 렌더링 성능 최적화 및 멀티 디스플레이 환경에서 정확한 출력 보장

## 7. 에디터 도구 사용 가이드

### 프로젝션 매핑 메뉴
Unity 에디터의 상단 메뉴에서 `AudioVisualizer/프로젝션 매핑` 메뉴를 통해 다양한 기능에 접근할 수 있습니다:

1. **새 프로젝션 설정 생성**:
   - 새 씬을 생성하거나 현재 씬에 프로젝션 설정을 추가합니다.
   - 자동으로 카메라와 메인 카메라가 생성됩니다.
   - 생성된 `ProjectionSetup` 컴포넌트를 통해 추가 설정을 진행할 수 있습니다.

2. **텍스처 결합기 추가**:
   - 선택된 게임 오브젝트에 `TextureCombiner` 컴포넌트를 추가합니다.
   - 선택된 오브젝트가 없으면 새 게임 오브젝트를 생성합니다.

3. **레이어 설정**:
   - 프로젝션 매핑에 필요한 레이어를 자동으로 생성합니다.
   - "CombinedDisplay"와 "DownDisplay" 레이어가 생성됩니다.

### 카메라 설정 중요 사항
프로젝션 매핑 시스템의 핵심은 카메라 설정입니다. 다음 사항에 주의하세요:

1. **카메라 위치**:
   - 모든 카메라는 방 중앙(Vector3.zero)에 위치해야 합니다.
   - 이는 실제 프로젝션 매핑에서 관찰자가 방 중앙에서 주변을 바라보는 것과 같은 효과를 줍니다.

2. **카메라 방향**:
   - 각 카메라는 해당하는 벽을 향해 바깥쪽을 바라봐야 합니다:
     - 정면 카메라: Z축 양의 방향(0, 0, 0)을 바라봄
     - 좌측 카메라: X축 음의 방향(0, -90, 0)을 바라봄
     - 우측 카메라: X축 양의 방향(0, 90, 0)을 바라봄
     - 하단 카메라: Y축 음의 방향(90, 0, 0)을 바라봄

3. **벽 방향과 카메라 방향의 일치**:
   - 벽의 방향과 카메라의 방향이 일치해야 올바른 프로젝션이 이루어집니다.
   - 씬 뷰에서 카메라 방향이 시각화되어 쉽게 확인할 수 있습니다.

4. **카메라 설정 자동화**:
   - `AudioVisualizer/프로젝션 매핑/새 프로젝션 설정 생성` 메뉴를 사용하면 카메라가 올바른 위치와 방향으로 자동 설정됩니다.
   - 수동으로 카메라를 조정할 경우, 방 중앙에 위치시키고 올바른 방향을 설정해야 합니다.

5. **메인 카메라 설정**:
   - 메인 카메라 1과 2는 직교 투영(Orthographic) 모드로 설정됩니다.
   - 이는 텍스처를 왜곡 없이 화면에 표시하기 위한 최적의 설정입니다.
   - 카메라는 디스플레이 플레인을 정면에서 바라보도록 위치합니다(Z축 음의 방향).
   - 카메라의 orthographicSize는 디스플레이 플레인의 크기에 맞게 자동으로 조정됩니다.

6. **디스플레이 플레인 설정**:
   - 디스플레이 플레인은 메인 카메라 앞에 위치하며, 카메라와 정면으로 마주보도록 설정됩니다.
   - 플레인은 Y축을 기준으로 180도 회전되어 있어 카메라에서 텍스처가 올바르게 보입니다.
   - 플레인의 크기는 각 타겟 디스플레이의 비율에 맞게 스트레치되어 해당 화면을 꽉 채웁니다.
   - 각 카메라의 targetDisplay 설정에 따라 해당 디스플레이의 비율을 자동으로 감지하여 적용합니다.
   - 멀티 디스플레이 환경에서 각 화면마다 다른 비율을 가질 수 있으므로, 각 디스플레이에 맞게 개별적으로 조정됩니다.
   - 화면 크기가 변경되면 자동으로 감지하여 플레인 크기를 조정합니다. 이는 성능 최적화를 위해 변경이 있을 때만 실행됩니다.
   - 화면 크기 변경 감지는 `TextureCombiner`와 `ProjectionSetup` 클래스 모두에 구현되어 있어 어느 쪽에서든 변경을 처리할 수 있습니다.
   - 이전에는 텍스처 비율(결합된 텍스처 3:1, 하단 텍스처 16:9)에 맞게 조정했지만, 현재는 각 디스플레이의 비율에 맞게 조정하여 항상 화면을 꽉 채우도록 변경되었습니다.
   - 각 플레인은 해당하는 레이어("CombinedDisplay" 또는 "DownDisplay")에 할당됩니다.

### ProjectionSetup 컴포넌트
`ProjectionSetup` 컴포넌트는 프로젝션 매핑의 핵심 설정을 담당합니다:

1. **카메라 설정**:
   - 4개의 카메라(정면, 좌, 우, 하단)를 설정합니다.
   - 각 카메라의 위치와 회전을 조정할 수 있습니다.

2. **렌더 텍스처 설정**:
   - 각 카메라에 대한 렌더 텍스처를 설정합니다.
   - "렌더 텍스처 생성" 버튼을 통해 자동으로 생성할 수 있습니다.

3. **메인 카메라 설정**:
   - 2개의 메인 카메라를 설정합니다.
   - 메인 카메라 1은 3면(정면, 좌, 우)을 담당합니다.
   - 메인 카메라 2는 하단을 담당합니다.

4. **멀티 디스플레이 설정**:
   - 멀티 모니터 환경에서의 설정을 조정합니다.
   - 각 메인 카메라가 어떤 디스플레이에 출력될지 설정합니다.

5. **텍스처 결합 설정**:
   - 텍스처 결합 사용 여부를 설정합니다.
   - 결합된 텍스처와 머티리얼을 설정합니다.

6. **방 설정**:
   - 방의 크기를 설정합니다.
   - 컨텍스트 메뉴에서 "벽 오브젝트 생성"을 선택하여 방 벽을 생성합니다.

### TextureCombiner 컴포넌트
`TextureCombiner` 컴포넌트는 텍스처 결합을 담당합니다:

1. **입력 텍스처**:
   - 4개의 입력 텍스처(좌, 정면, 우, 하단)를 설정합니다.

2. **출력 텍스처**:
   - 결합된 텍스처를 설정합니다.

3. **머티리얼**:
   - 결합된 텍스처와 하단 텍스처를 표시할 머티리얼을 설정합니다.

4. **디스플레이**:
   - 결합된 텍스처와 하단 텍스처를 표시할 디스플레이 플레인을 설정합니다.

5. **레이어 설정**:
   - 디스플레이 플레인의 레이어를 설정합니다.

6. **조정**:
   - 밝기와 대비를 조정합니다.

7. **자동 업데이트**:
   - 텍스처 결합의 자동 업데이트 여부를 설정합니다.

## 8. 추가 팁

- 렌더 텍스처의 해상도를 프로젝터 해상도에 맞게 설정하세요.
- 파티클 시스템의 반응성을 음악 스타일에 맞게 조정하세요.
- 멀티 디스플레이 설정은 빌드 설정에서 "Player Settings > Resolution and Presentation"에서 "Display Resolution Dialog"를 "Enabled"로 설정해야 합니다.
- 비트 감지 임계값은 음악의 볼륨과 특성에 따라 조정해야 합니다.
- 에디터에서 설정한 후 런타임에서 자동으로 적용되므로, 별도의 코드 작성 없이도 프로젝션 매핑을 구현할 수 있습니다.
- 씬 뷰에서 방 크기와 벽 위치가 시각화되므로, 쉽게 공간을 파악하고 조정할 수 있습니다. 