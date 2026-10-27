# 레터런 2D 유니티 프로젝트

기존 레터런의 배달 규칙과 모바일 조작을 유지하고, 화면과 물리를 XY 평면의 2D 방식으로 전환한 프로젝트입니다.
보관된 3D 기본 프로젝트와 한글 UI·능력 편지 업데이트를 합쳐 전환했습니다. 사용자 PC에서 이후 따로 수정한 파일은 포함하지 않습니다.

## 여는 순서

1. ZIP을 새 폴더에 압축 해제합니다. 기존 프로젝트의 Scripts 안에 넣는 패치가 아닙니다.
2. Unity Hub에서 프로젝트 추가를 눌러 **Assets, Packages, ProjectSettings가 들어 있는 LetterRun2D 폴더**를 선택합니다.
3. 기존에 사용하던 **Unity 6000.3.18f1**로 엽니다. 첫 실행에는 패키지 다운로드를 위한 인터넷 연결이 필요합니다.
4. 컴파일과 임포트가 끝나면 `Assets/Scenes/LetterTown2D.unity` 장면을 자동으로 만들고 엽니다.
5. 자동으로 열리지 않으면 상단 메뉴 `Letter Run 2D > 게임 장면 열기`를 누릅니다.
6. Game 창을 세로 비율로 놓고 Play를 누른 뒤 **게임 시작**을 누릅니다.

장면은 편집 모드에서 실제 게임 오브젝트로 생성하고 저장합니다. Play 중에만 보이는 임시 장면이 아닙니다.
`Boot.unity`는 기본 장면 생성에 문제가 생겼을 때 런타임 생성으로 확인할 수 있는 보조 장면입니다.
`기본 장면 다시 만들기`는 사용자가 수정한 장면 배치를 초기화하므로, 필요할 때만 사용하세요.

## 바뀐 내용

- 블록 메시 대신 2D 픽셀 스프라이트 캐릭터, 편지, 자동차, 우체통, 나무, 건물을 사용합니다.
- Rigidbody2D, CircleCollider2D, BoxCollider2D로 이동·충돌·배달 감지를 처리합니다. 3D 물리 컴포넌트는 사용하지 않습니다.
- 카메라는 XY 평면을 정면으로 봅니다. 상단 HUD와 하단 조작 영역 사이에 맵 전체가 들어오도록 조정합니다.
- 캐릭터의 이동 방향에 따라 앞/뒤/옆 스프라이트가 바뀌고 이동 중 살짝 움직입니다.
- 벽은 이동을 막고, 자동차 및 위험 상태의 바닥은 피격을 처리합니다.
- 픽셀 이미지는 Point 필터, 무압축, 밉맵 없음으로 설정했습니다.

## 유지한 게임 규칙

- 제한 시간 60초, 목표 6개 배달, 가방 최대 3개.
- 일반 편지 6개와 가속·보호 편지 각 1개, 총 8개.
- 왼쪽 가상 스틱 이동 / 편지 줍기 버튼 탭 / 시간 감속 버튼 홀드.
- 우체통 근처에 들어가면 보유한 편지를 자동 배달합니다.
- 능력 편지도 가방 및 배달 수량에 포함합니다. 가방이 가득 차면 먼저 배달해야 합니다.
- 가속 기본 5초, 이동 속도 1.5배 / 보호 기본 6초.
- 능력 지속 시간과 막대는 캐릭터 머리 위에 표시합니다.
- 자동차 2대와 시간차 바닥 위험 구역 2곳. 바닥은 회색 안전, 노랑 예고, 빨강 위험입니다.
- 충돌 시 현재 위치에서 0.8초 스턴과 제한 시간 3초 감소. 보유 편지는 유지하며 회복 보호 시간을 제공합니다.
- 시간 감속은 장애물 이동과 바닥 작동 주기를 늦춥니다. 제한 시간은 계속 흐릅니다.
- 한글 시작·결과·일시정지 화면. 중앙 아래 안내 문구는 기본 비활성화 상태입니다.

## 수정할 곳

플레이를 끈 상태에서 Hierarchy에서 선택해 Inspector에서 변경한 뒤 저장하세요.

| 수정 내용 | 선택할 오브젝트 / 컴포넌트 |
|---|---|
| 제한 시간, 목표 수량, 가방 용량, 충돌 패널티 | LetterRun2D / DeliveryGame |
| 이동 속도, 수집 범위, 가속·보호 지속 시간 | 2D Town > Player / CourierPlayer |
| 자동차 경로와 속도 | Moving car / MovingObstacle의 Start Point, End Point, Move Speed |
| 바닥의 안전·예고·위험 시간 | Timed hazard / TimedHazard |
| 편지의 위치와 종류 | Letter / Transform 및 LetterItem의 Kind |
| 시작 화면 글자 | Canvas > Safe Area > Start menu 안의 Text |
| 카메라에 담는 범위 | Main Camera / CameraFrame2D의 Map Width, Map Height |

맵 자체를 확대할 때는 바닥과 경계 벽도 함께 옮기세요. CameraFrame2D 값만 바꾸면 보는 범위만 달라집니다.
편지를 더 배치하면 DeliveryGame의 Letters 배열에도 등록하고, 자동차를 더 배치하면 Obstacles 배열에 등록하세요.
새로운 배달 목표나 총 편지 수를 정했다면 시작 화면 Rules 문구도 수정하세요.

## 휴대폰용 웹 배포

이 ZIP은 소스 프로젝트이며 기존 공개 URL의 게임을 자동으로 바꾸지 않습니다.
Unity의 Web 대상 빌드에 LetterTown2D 장면을 포함하고 다시 빌드한 뒤, 기존 GitHub Pages 배포 폴더의 index.html, Build, TemplateData를 새 빌드 결과로 교체해야 합니다.
기본 장면 생성 시 WebGL 압축은 Disabled로 설정합니다. 기존 URL에 배포하면 동일한 QR을 사용할 수 있습니다.

## 확인 범위와 실행 후 점검

이 제작 환경에는 Unity Editor가 없어 Unity 컴파일·Play·Web 빌드를 실행하지 못했습니다.
C# 구문 분석, 2D 물리 전환 여부, 리소스 경로와 중복 GUID, 편지 8개의 맵 내부 배치를 정적으로 점검했습니다.

Unity에서 다음을 확인하세요.

- 시작 후 스틱과 버튼을 동시에 눌러 이동·수집·감속이 동작하는지.
- 3개 수집 후 추가 수집이 막히고, 우체통에서 가방이 비며 배달 수가 증가하는지.
- 능력 편지를 주우면 수량이 증가하고 머리 위 타이머가 끝난 뒤 효과가 해제되는지.
- 자동차와 빨간 바닥에 닿으면 제자리 스턴 및 시간 감소가 발생하는지.
- 보호 중에는 충돌 패널티가 발생하지 않는지.
- 일시정지에서 타이머와 능력 시간이 멈추고, 재시작하면 편지·장애물·수량이 초기화되는지.
- 6개 배달 성공과 시간 종료 실패가 표시되는지.
- 실제 휴대폰에서 HUD와 조작 버튼이 화면 밖으로 잘리지 않는지.

## 사용 자료

2D 픽셀 스프라이트: 이 프로젝트용으로 새로 제작한 도형 기반 픽셀 이미지.
한글 폰트: 기존 프로젝트에 포함된 CourierKorean.ttf. 라이선스는 Assets/Resources/Fonts/OFL.txt 참조.
입력: Unity Input System 1.17.0 / UI: UGUI 2.0.0.
API 참고: https://docs.unity.cn/6000.1/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html
입력 UI 참고: https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/ui-input-module-reference.md
