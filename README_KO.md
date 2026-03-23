# FriendlyFire-StS2 — Slay the Spire 2 아군 피해 Mod

**🌐 Language / 언어：** [English](README_EN.md) | [日本語](README_JA.md) | 한국어 | [中文](README.md)

> `Alt` 키를 누른 채 공격 카드로 팀원을 "우호적으로" 베어보세요.

![Slay the Spire 2](https://img.shields.io/badge/Slay%20the%20Spire%202-Mod-red?style=flat-square)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)
![Harmony 2.4.2](https://img.shields.io/badge/Harmony-2.4.2-green?style=flat-square)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)
![AI Assisted](https://img.shields.io/badge/AI%20Assisted-Claude-blueviolet?style=flat-square)

---

## 기능 소개

| 기능 | 설명 |
|------|------|
| **단일 대상 아군 피해** | `Alt` 키를 누른 채 `AnyEnemy` 타입 공격 카드로 팀원을 대상으로 선택 가능 |
| **AOE 확장 공격** | `Alt` 키를 누른 채 `AllEnemies` 타입 AOE 카드가 **다른 플레이어의 캐릭터**에도 적중 (자신과 소환물 제외) |
| **특수 효과 적용** | 카드의 디버프(취약, 약화 등)가 팀원에게도 정상 적용 |
| **JSON 화이트리스트** | 설정 파일로 아군 피해를 허용할 카드를 세밀하게 제어 |
| **위험 카드 보호** | `Monster` 속성에 접근하는 카드를 자동 차단하여 크래시 방지 |
| **화면 표시** | 토글 키를 누르고 있는 동안 화면 상단에 빨간색 "아군 피해 활성화" 배너 표시 |
| **멀티플레이 동기화 안전** | TargetId 신호 메커니즘으로 모든 클라이언트의 상태 일치 보장 |

---

## 설치 가이드

> **중요: Mod를 설치하기 전에 반드시 세이브 데이터를 백업하세요!**
>
> 세이브 위치:
> - **Windows:** `%APPDATA%\..\Roaming\SlayTheSpire2\`
> - **macOS:** `~/Library/Application Support/SlayTheSpire2/`

### 1단계: 게임 디렉토리 확인

| 플랫폼 | 경로 |
|--------|------|
| **Windows** | `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\` |
| **macOS** | `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/` |

> **팁:** Steam에서 게임 우클릭 → 관리 → 로컬 파일 탐색

### 2단계: mods 폴더 생성

게임 루트 디렉토리에 `mods` 폴더를 생성 (이미 있으면 건너뛰기).

### 3단계: BaseLib (필수 의존성) 설치

이 Mod는 [Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2)가 필요합니다. **먼저 설치하세요.**

1. [BaseLib-StS2 Releases](https://github.com/Alchyr/BaseLib-StS2/releases)에서 최신 버전 다운로드
2. 압축 해제 후 `BaseLib` 폴더를 `mods/`에 배치

### 4단계: FriendlyFire 설치

1. [Releases](../../releases)에서 최신 `FriendlyFire.zip` 다운로드
2. 압축 해제 후 `FriendlyFire` 폴더를 `mods/`에 배치

```
mods/
  +-- BaseLib/                      <-- 필수 의존성 (3단계)
  +-- FriendlyFire/                 <-- 이 Mod
        +-- FriendlyFire.dll
        +-- FriendlyFire.pck
        +-- mod_manifest.json
        +-- friendly_fire_config.cfg
```

### 5단계: 게임 실행

1. Slay the Spire 2 실행
2. 메인 메뉴 → **Mod 관리자**
3. **BaseLib**과 **Friendly Fire** 활성화
4. 협동 전투 시작

### 사용 방법

| 동작 | 효과 |
|------|------|
| **Alt 없이** 공격 카드 사용 | 일반 동작 (바닐라와 동일) |
| **Alt를 누른 채** 단일 대상 카드 사용 | 팀원을 대상으로 선택 가능, 빨간색 표시 나타남 |
| **Alt를 누른 채** AOE 카드 사용 | AOE가 모든 적 + 다른 플레이어의 캐릭터에 적중 (자신과 소환물 제외) |

> **멀티플레이 주의:** 모든 플레이어가 **같은 버전**의 Mod와 **동일한** 화이트리스트 설정을 사용해야 합니다. 호스트가 설정 파일을 배포하는 것을 권장합니다.

### 제거 방법

1. `mods/FriendlyFire/` 폴더 삭제
2. 게임 재시작 — 세이브 데이터에 영향 없음

---

## 설정

`friendly_fire_config.cfg`를 편집하여 Mod 동작을 커스터마이즈. 변경 후 **게임 재시작** 필요.

```jsonc
{
  // 이 키를 눌러 아군 피해 활성화. 선택지: Alt, Shift, Ctrl, Tab, Space, F1~F4
  "toggle_key": "Alt",

  // 단일 대상 공격 카드 화이트리스트 (카드 클래스명)
  // 빈 배열 [] = 모든 단일 대상 공격 카드 허용
  "single_target_whitelist": [],

  // AOE 아군 피해 확장 활성화
  "aoe_enabled": true,

  // AOE 카드 화이트리스트 (위와 같은 규칙)
  "aoe_whitelist": [],

  // 위험 카드 블랙리스트 (Target.Monster에 접근하여 크래시를 일으키는 카드)
  "dangerous_cards_blacklist": []
}
```

---

## FAQ

**Q: 아군 피해가 자신에게도 적용되나요?**
> 아닙니다. AOE는 공격자 본인과 모든 소환물/펫을 제외합니다.

**Q: 싱글플레이에서 작동하나요?**
> 단일 대상 아군 피해에는 대상이 될 팀원이 없습니다. 이 Mod는 **협동 멀티플레이** 용으로 설계되었습니다.

**Q: 멀티플레이에서 연결이 끊기나요?**
> 아닙니다. Mod는 TargetId 신호 메커니즘을 사용하여 모든 클라이언트가 동일한 대상 계산을 수행합니다. 모든 플레이어가 같은 Mod 버전과 설정을 사용해야 합니다.

---

## AI 개발 노트

이 프로젝트는 AI(Claude)를 활용하여 개발되었습니다. 자세한 기술 아키텍처는 [중국어 README](README.md)를 참조하세요.

---

## 라이선스

[MIT License](LICENSE) — 자유롭게 사용, 수정, 배포 가능.
