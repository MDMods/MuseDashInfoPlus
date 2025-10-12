# Info+

[English](README.md) | [简体中文](README_zh-CN.md) | 繁體中文 | [日本語](README_ja.md) | [한국어](README_ko.md) | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

## 概述

Info+ 是一個高度客製化的 MuseDash 遊戲模組，用於顯示額外的遊戲資訊。

本模組受 MuseDashCustomPlay 啟發。

## 功能特性

在遊戲內顯示各種資訊，如 **譜面資訊、準確率、Miss/Great/Early/Late/已擊打/總物量計數、最高分數/準確率差值、天空/地面速度** 等。

您可以隨意調整各項資料的開關、位置、大小、顏色、字型、格式甚至是字型描邊。

## 注意事項

- ⌨️ **UI 顯隱快捷鍵**：按 **F10** 可切換所有遊戲內 UI 的顯示。如果 UI 意外消失，再次按 **F10** 即可恢復。該快捷鍵可在 `MainConfigs.yml` 中更改
- 歷史最佳記錄下的 Miss/Great/Early/Late 資料並沒有儲存在任何地方，因此只能由 Info+ 來儲存。您必須在安裝 Info+ 的情況下拿到至少一次最佳成績才能使歷史最佳記錄的資料差值生效
- 若您載入了 [SongDesc](https://github.com/MDMods/SongDesc) 模組，譜面資訊將預設隱藏
- 東方特殊譜面由於不受支援將導致部分資料無法正常工作
- 物量計數器會將長條視為兩個 Note，首尾各算一個

## 預覽

![預覽1](static/Preview1.webp)

![預覽2](static/Preview2.webp)

![預覽3](static/Preview3.webp)

## 設定

該模組的設定檔案按照類別分開儲存於不同檔案，您可以在 `.\MuseDash\UserData\Info+\` 目錄中找到它們，檔案內的所有設定項都包含了註解，請明確設定項實際作用後更改。

所有設定檔案會在**首次啟動後自動產生**，修改設定後**儲存檔案即可立即生效**。

- `MainConfigs.yml` 為主要設定檔案
- `TextFieldLowerLeftConfigs.yml` 為螢幕左下角的文字設定檔案
- `TextFieldLowerRightConfigs.yml` 為螢幕右下角的文字設定檔案
- `TextFieldScoreBelowConfigs.yml` 為 "得分" 字樣右方的文字設定檔案，無論場景如何，該文字總會位於 "得分" 字樣的右方
- `TextFieldScoreRightConfigs.yml` 為分數右方的文字設定檔案，無論分數多少，該文字總會位於分數右方
- `TextFieldUpperLeftConfigs.yml` 為 "得分" 字樣下方的文字設定檔案，無論場景如何，該文字總會位於 "得分" 字樣的下方
- `TextFieldUpperRightConfigs.yml` 為螢幕右上角的文字設定檔案
- `AdvancedConfigs.yml` 僅供進階玩家使用，若您不知道裡面的設定有何含義，請不要更改

### 資料佔位符

您會在文字設定檔案中看到類似 `text: '{overview} / {stats}'` 這樣的設定項。`{dataName}` 將會按照下述的規則替換，您可以隨意搭配使用。

- `{pbScore}`：個人最佳分數
- `{scoreGap}`：當前得分與個人最佳分數的差值
- `{pbAcc}`：個人最佳準確率
- `{accGap}`：當前準確率與個人最佳準確率的差值
- `{acc}`：當前準確率
- `{rank}`：當前評級
- `{total}`：總物量
- `{hit}`：已經擊打/拾取/跳過齒輪的物量
- `{song}`：譜面名稱
- `{diff}`：譜面難度（數字）
- `{level}`：譜面難度（文字）
- `{author}`：音樂作者
- `{levelDesigner}`：譜面關卡設計者
- `{bpm}`：譜面 BPM（固定資料，非即時 BPM）
- `{overview}`：TP / AP 指示，若當前準確率低於 100%，則改為顯示當前準確率
- `{stats}`：Miss / Great / Early / Late 等資料
- `{pbStats}`：個人最佳記錄下的 Miss / Great / Early / Late 等資料
- `{pbStatsGap}`：當前與個人最佳記錄下的 Miss / Great / Early / Late 等資料的差值
- `{pbGreat}`：個人最佳記錄下的 Great 計數
- `{pbMissOther}`：個人最佳記錄下的 Miss 計數（不包括可收集 Note）
- `{pbMissCollectible}`：個人最佳記錄下的 Miss 計數（僅包括可收集 Note）
- `{pbEarly}`：個人最佳記錄下的 Early 計數
- `{pbLate}`：個人最佳記錄下的 Late 計數
- `{skySpeed}`：當前天空速度
- `{groundSpeed}`：當前地面速度
- `{time}`：系統本地時間

提示：若設定項提示支援富文字，代表該設定項將會依照您填入的富文字產生對應文字。例：`<size=40><color=#e1bb8a>{total}</color></size>` 。如果您不知道什麼是富文字，請自行搜尋；如果您需要換行，使用 `\n` 。

## 如何使用

- 根據下列出的依賴項安裝 MelonLoader 至 Muse Dash
- 下載 [Latest Release](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) 並將 `Info+.dll` 放置於 `.\MuseDash\Mods\` 目錄下
- 啟動遊戲即可

## 依賴項

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 或 v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## 開發者說明

我對 Unity 遊戲模組開發了解不多，目前主要致力於讓功能正常運作，實作方式可能不夠優雅。如果你有任何問題或願意幫助改進這個模組，歡迎提交 [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) 或 [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare)，非常感謝你的支持！
