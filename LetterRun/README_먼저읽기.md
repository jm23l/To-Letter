# 시간 배달부 — 복셀 3D Unity 프로젝트

수정한 PPT를 바탕으로 만든 3D 복셀 도시 프로토타입입니다.
Unity 6000.3.18f1을 기준으로 구성했습니다. Input System 1.17.0, Unity UI 2.0.0을 사용합니다.

## 바로 열기

1. ZIP 파일을 압축 해제합니다.
2. Unity Hub → Projects → Add → Add project from disk에서 `TimeCourier_Voxel3D` 폴더를 선택합니다.
   `Assets`, `Packages`, `ProjectSettings` 폴더가 들어 있는 폴더가 프로젝트 루트입니다.
3. Unity 6000.3.18f1로 엽니다. 처음에는 패키지를 받으므로 인터넷 연결이 필요합니다.
4. 컴파일이 끝나면 `Assets/Scenes/VoxelCity.unity`가 자동 생성되고 열립니다.
5. Game 탭의 화면 비율을 9:16 또는 480×854로 지정합니다.
6. Play를 누르고 DISTRICT 1을 선택합니다.

씬 자동 생성이 실행되지 않으면 상단 메뉴 `Time Courier → Open Game Scene`을 누르세요.
또는 `Assets/Scenes/Boot.unity`를 열어 Play를 누르면 같은 게임을 런타임에 생성합니다.
Boot는 백업 진입점이며, 평소 수정은 VoxelCity에서 합니다.

## 조작

| 기능 | 모바일 / 마우스 | 키보드 |
|---|---|---|
| 이동 | 왼쪽 아래 가상 스틱 드래그 | WASD / 방향키 |
| 편지 수집 | 오른쪽 아래 COLLECT 클릭 | E |
| 시간 능력 | HOLD SLOW를 누르고 유지 | Space 유지 |
| 일시정지 | 오른쪽 위 PAUSE | 버튼 클릭 |

가상 스틱과 홀드 버튼은 서로 다른 터치 ID를 사용하므로 동시에 조작할 수 있습니다.
PC 마우스 하나로 스틱과 버튼을 동시에 누를 수는 없습니다. PC 테스트에서는 키보드 이동을 쓰세요.

## 게임 규칙

- 한 판 60초, 편지 6개를 우체통 POST에 배달하면 성공합니다.
- 가방에 최대 3개까지 담을 수 있습니다. 두 번 이상 왕복해야 합니다.
- 가까운 편지부터 한 개씩 수집합니다. 버튼을 누르기 전에 편지에 접근하세요.
- POST에 닿으면 가진 편지를 전부 배달합니다.
- 블록 자동차 두 대가 도로를 왕복합니다. 충돌하면 3초가 감소하고 시작 위치로 돌아갑니다.
- 충돌해도 가진 편지는 유지합니다. 1초 동안 연속 피해를 막습니다.
- 시간 능력을 사용하면 장애물의 이동 속도만 30%가 됩니다.
- 플레이어 이동 속도와 남은 시간의 감소 속도는 그대로입니다.
- 능력은 최대 5초 연속 사용합니다. 사용하지 않으면 초당 0.8만큼 회복합니다.
- 게이지가 바닥나면 버튼을 놓은 뒤 다시 눌러야 합니다.
- 구역 2는 같은 맵에서 장애물 속도가 1.4배인 도전 모드입니다.
- 앱이 다른 화면으로 넘어가면 자동으로 일시정지합니다.

위 숫자와 충돌 페널티는 테스트용으로 정한 기본값입니다.

## 수정할 곳

- Hierarchy `TimeCourier`의 DeliveryGame: 제한 시간, 목표 배달 수, 가방 용량, 시간 능력과 충돌 페널티.
- `Voxel City/Player`의 CourierPlayer: 이동 속도, 수집 거리, 시작 위치.
- `Voxel City/Moving car`의 MovingObstacle: Start Point / End Point / 이동 속도.
- `Voxel City/Letter`: 위치를 옮길 수 있습니다. 편지를 추가하거나 삭제하면 DeliveryGame의 Letters 배열도 수정하세요.
- 목표 배달 수는 맵에 있는 편지 수 이하로 설정하세요. 기본 버전은 편지를 재생성하지 않습니다.
- Canvas의 메뉴와 버튼은 일반 Unity UI 오브젝트이므로 수정할 수 있습니다.
- UI 텍스트는 폰트 의존성을 줄이기 위해 영어로 넣었습니다. 한국어로 바꾸려면 배포 가능한 한글 폰트를 추가하고 Text의 Font를 교체하세요.
- `Time Courier → Create or Reset Demo Scene`은 VoxelCity을 초기 상태로 덮어쓰므로 수정 내용을 백업한 뒤 사용하세요.

## 코드 구조

- DeliveryGame: 판 상태, 제한 시간, 배달, 능력 게이지, 메뉴와 UI.
- CourierPlayer: Input System 키보드 입력, 이동, 편지 수집, 가방.
- LetterItem / Mailbox: 수집 상태와 배달 판정.
- MovingObstacle: 왕복 이동, 시간 능력 적용, 충돌.
- MobileStick / TimeHoldButton: 멀티터치 UI.
- SafeArea / PortraitCamera: 화면 대응.
- GameBootstrap: 씬 구성. Editor/ProjectSetup: 첫 실행 시 편집 가능한 씬 생성.

## Android 실행

Unity Hub에 해당 에디터의 Android Build Support, SDK/NDK, OpenJDK를 설치하세요.
Unity의 Build Profiles에서 Android로 전환한 뒤 생성된 VoxelCity을 포함해 빌드합니다.
세로 화면 고정 설정은 프로젝트에 포함되어 있습니다. APK 파일은 제공하지 않습니다.

## 확인 상태

이 프로젝트를 제작한 환경에는 Unity Editor가 없어 실제 Unity 컴파일, Play, Android 빌드는 실행하지 못했습니다.
프로젝트 파일 구조, 패키지 선언, GUID 연결, 리소스 이름 및 소스 구성을 점검했습니다.
최초 패키지 다운로드나 Unity 버전에 따른 문제는 Console의 첫 번째 오류부터 확인하세요.

확인할 동작: 시작 → 이동 → 편지 3개 수집 → 우체통 배달 → 나머지 배달 → 성공.
추가로 장애물 충돌, 능력 게이지 소진·재충전, 시간 초과, 재시작, 모바일 동시 터치를 확인하세요.

외부 이미지나 유료 에셋은 포함하지 않았습니다. 캐릭터, 건물, 자동차, 나무, 우체통은 Unity 기본 큐브로 구성했습니다. 별도의 모델 파일 없이 편집할 수 있습니다.

## 3D 버전의 변경점

- 청록색 제복, 모자, 배달 가방을 갖춘 블록 캐릭터와 다리 움직임.
- 입체 건물, 창문, 지붕, 나무, 포장도로, 횡단보도, 자동차.
- 대각선 위에서 바라보는 직교 카메라. 카메라 기준 방향으로 이동합니다.
- 충돌과 우체통 판정을 Rigidbody / Collider 기반의 3D 물리로 변경했습니다.
- `Main Camera`의 PixelCamera가 3D 화면만 낮은 해상도로 렌더링합니다. UI는 선명하게 유지합니다.
- PixelCamera의 Vertical Resolution 기본값 480. 320으로 낮추면 더 거친 픽셀, 720이면 더 부드러운 화면입니다.
- Pixel Effect 체크를 끄면 원래 3D 화질로 볼 수 있습니다.
- Built-in Render Pipeline 프로젝트입니다. 별도의 URP 프로젝트에 복사하면 머티리얼을 수정해야 합니다.
- 배달 목표는 빨간 블록 우체통입니다. 우체통에 가까이 가면 자동으로 배달합니다.
- 게임 UI는 영어입니다. 한국어 폰트는 포함하지 않았습니다.

이전 2D 프로젝트에 덮어쓰지 말고, 새 폴더를 Unity Hub에 추가해 여세요.
실제 Unity Editor가 없는 환경에서 작성했으므로 실행, 셰이더 컴파일과 Android 빌드는 검증하지 못했습니다.
