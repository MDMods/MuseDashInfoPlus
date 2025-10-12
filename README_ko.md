# Info+

[English](README.md) | [简体中文](README_zh-CN.md) | [繁體中文](README_zh-TW.md) | [日本語](README_ja.md) | 한국어 | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

> **주의:** 이 README는 AI에 의해 생성되었으며 부정확하거나 모호한 내용이 포함될 수 있습니다. 정확한 정보는 공식적으로 유지 관리되는 [중국어](README_zh-CN.md) 또는 [영어](README.md) README를 참조하시기 바랍니다.

## 개요

Info+는 게임 내 추가 정보를 표시하는 고도로 커스터마이징 가능한 Muse Dash 게임 모드입니다.

이 모드는 MuseDashCustomPlay에서 영감을 받았습니다.

## 기능

**차트 정보, 정확도, Miss/Great/Early/Late/Hit/총 노트 수, 개인 최고 기록과의 점수/정확도 차이, 하늘/지상 속도** 등 다양한 정보를 표시합니다.

모든 데이터 요소는 표시 여부, 위치, 크기, 색상, 글꼴, 서식 및 테두리까지 자유롭게 커스터마이징할 수 있습니다.

## 중요 사항

- ⌨️ **UI 전환 단축키**：**F10** 을 눌러 모든 게임 내 UI 표시를 전환할 수 있습니다. UI가 실수로 사라진 경우 **F10** 을 다시 눌러 복원할 수 있습니다. 이 단축키는 `MainConfigs.yml`에서 변경할 수 있습니다
- 개인 최고 기록의 Miss/Great/Early/Late 카운트는 기본 게임에 저장되지 않으며, Info+가 설치된 경우에만 저장할 수 있습니다. 개인 최고 통계 차이가 작동하려면 Info+가 설치된 상태에서 최소 한 번 개인 최고 점수를 달성해야 합니다
- [SongDesc](https://github.com/mdmods/songdesc) 모드를 로드한 경우 차트 정보가 기본적으로 비활성화됩니다. 설정에서 다시 활성화하세요
- 호환성 문제로 인해 동방 탄막 차트에서는 일부 데이터가 제대로 작동하지 않을 수 있습니다
- 노트 카운터는 홀드 노트를 두 개의 별도 노트로 취급합니다(시작과 끝을 모두 카운트). 반면 Miss 카운터와 바닐라 게임은 각 홀드 노트를 하나의 노트로 카운트합니다

## 미리보기

![미리보기 1](static/Preview1.webp)

![미리보기 2](static/Preview2.webp)

![미리보기 3](static/Preview3.webp)

## 설정

설정 파일은 카테고리별로 정리되어 `.\MuseDash\UserData\Info+\` 디렉토리에 저장됩니다. 모든 설정 항목에는 주석이 포함되어 있습니다. 수정하기 전에 각 설정의 목적을 이해하시기 바랍니다.

모든 설정 파일은 **최초 실행 시 자동 생성**됩니다. 설정을 수정한 후 **파일을 저장하면 즉시 적용**됩니다.

- `MainConfigs.yml`: 기본 설정 파일
- `TextFieldLowerLeftConfigs.yml`: 화면 좌하단 텍스트 설정
- `TextFieldLowerRightConfigs.yml`: 화면 우하단 텍스트 설정
- `TextFieldScoreBelowConfigs.yml`: "SCORE" 레이블 오른쪽 텍스트 설정 (레이블에 대해 고정 위치)
- `TextFieldScoreRightConfigs.yml`: 점수 표시 오른쪽 텍스트 설정 (점수에 대해 고정 위치)
- `TextFieldUpperLeftConfigs.yml`: "SCORE" 레이블 아래 텍스트 설정 (레이블에 대해 고정 위치)
- `TextFieldUpperRightConfigs.yml`: 화면 우상단 텍스트 설정
- `AdvancedConfigs.yml`: 고급 사용자 전용 - 매개변수를 이해하지 못하는 경우 수정하지 마세요

### 데이터 플레이스홀더

텍스트 설정 파일에는 `text: '{overview} / {stats}'`와 같은 항목이 있습니다.
`{dataName}` 플레이스홀더는 다음 규칙에 따라 대체됩니다. 이들은 자유롭게 조합할 수 있습니다:

- `{pbScore}`: 개인 최고 점수
- `{scoreGap}`: 현재 점수와 개인 최고 점수의 차이
- `{pbAcc}`: 개인 최고 정확도
- `{accGap}`: 현재 정확도와 개인 최고 정확도의 차이  
- `{acc}`: 현재 정확도
- `{rank}`: 현재 랭크
- `{total}`: 총 노트 수
- `{hit}`: 현재 히트/수집/점프 수
- `{song}`: 차트 이름
- `{diff}`: 차트 난이도 (숫자)
- `{level}`: 차트 난이도 (텍스트)
- `{author}`: 차트 제작자
- `{levelDesigner}`: 차트의 레벨 디자이너
- `{bpm}`: 차트 BPM (고정 데이터, 실시간 BPM 아님)
- `{overview}`: TP/AP 표시기, 100% 미만일 경우 현재 정확도 표시
- `{stats}`: Miss/Great/Early/Late 카운트
- `{pbStats}`: 개인 최고 Miss/Great/Early/Late 카운트
- `{pbStatsGap}`: 현재와 개인 최고의 Miss/Great/Early/Late 카운트 차이
- `{pbGreat}`: 개인 최고 Great 카운트
- `{pbMissOther}`: 개인 최고 Miss 카운트 (수집 가능 미스 제외)
- `{pbMissCollectible}`: 개인 최고 Miss 카운트 (수집 가능 미스만)
- `{pbEarly}`: 개인 최고 Early 카운트
- `{pbLate}`: 개인 최고 Late 카운트
- `{skySpeed}`: 현재 하늘 속도
- `{groundSpeed}`: 현재 지상 속도
- `{time}`: 시스템 로컬 시간

참고: 일부 설정 항목에서는 리치 텍스트가 지원됩니다. 예:
`<size=40><color=#e1bb8a>{total}</color></size>`. 리치 텍스트에 익숙하지 않은 경우 검색하시기 바랍니다. 줄 바꿈에는 `\n`을 사용하세요.

## 설치

1. 아래 나열된 종속성을 기반으로 MelonLoader를 Muse Dash에 설치합니다
2. [최신 릴리스](https://github.com/KARPED1EM/MuseDashInfoPlus/releases)를 다운로드하고 `Info+.dll`을 `.\MuseDash\Mods\` 디렉토리에 배치합니다
3. 게임을 시작하고 즐기세요

## 종속성

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 또는 v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## 개발자 참고 사항

저는 Unity 모딩에 비교적 익숙하지 않으며 주로 기능이 작동하도록 하는 데 중점을 두었습니다. 구현이 가장 우아하지 않을 수 있습니다. 질문이 있거나 이 모드를 개선하는 데 도움을 주고 싶으시다면 언제든지 [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new)를 열거나 [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare)를 제출해 주세요. 여러분의 지원에 진심으로 감사드립니다!
