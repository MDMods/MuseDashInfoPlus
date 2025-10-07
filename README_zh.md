# Info+

[English](README.md) | 简体中文 | [繁體中文](README_zh-TW.md) | [日本語](README_ja.md) | [한국어](README_ko.md) | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

## 概述

Info+ 是一个高度客制化的 MuseDash 游戏模组，用于显示额外的游戏信息。

本模组受 MuseDashCustomPlay 启发。

## 功能特性

在游戏内显示各种信息，如 **谱面信息、准确率、Miss/Great/Early/Late/已击打/总物量计数、最高分数/准确率差值、天空/地面速度** 等。

您可以随意调整各项数据的开关、位置、大小、颜色、字体、格式甚至是字体描边。

## 注意事项

- 历史最佳记录下的 Miss/Great/Early/Late 数据并没有保存在任何地方，因此只能由 Info+ 来保存。您必须在安装 Info+ 的情况下拿到至少一次最佳成绩才能使历史最佳记录的数据差值生效
- 若您加载了 [SongDesc](https://github.com/MDMods/SongDesc) 模组，谱面信息将默认隐藏
- 东方特殊谱面由于不受支持将导致部分数据无法正常工作
- 物量计数器会将长条视为两个 Note，首尾各算一个

## 预览

![预览1](Static/Preview1.webp)

![预览2](Static/Preview2.webp)

![预览3](Static/Preview3.webp)

## 配置

该模组的配置文件按照类别分开储存于不同文件，您可以在 `.\MuseDash\UserData\Info+\` 目录中找到它们，文件内的所有配置项都包含了注释，请明确配置项实际作用后更改。

所有配置文件会在**首次启动后自动生成**，修改配置后**保存文件即可立即生效**（如果您在游戏中，那么下一局游戏生效）。

- `MainConfigs.yml` 为主要配置文件
- `TextFieldLowerLeftConfigs.yml` 为屏幕左下角的文本配置文件
- `TextFieldLowerRightConfigs.yml` 为屏幕右下角的文本配置文件
- `TextFieldScoreBelowConfigs.yml` 为 “得分” 字样右方的文本配置文件，无论场景如何，该文本总会位于 “得分” 字样的右方
- `TextFieldScoreRightConfigs.yml` 为分数右方的文本配置文件，无论分数多少，该文本总会位于分数右方
- `TextFieldUpperLeftConfigs.yml` 为 ”得分“ 字样下方的文本配置文件，无论场景如何，该文本总会位于 ”得分“ 字样的下方
- `TextFieldUpperRightConfigs.yml` 为屏幕右上角的文本配置文件
- `AdvancedConfigs.yml` 仅供高级玩家使用，若您不知道里面的配置有何含义，请不要更改

### 数据占位符

您会在文本配置文件中看到类似 `text: '{overview} / {stats}'` 这样的配置项。`{dataName}` 将会按照下述的规则替换，您可以随意搭配使用。

- `{pbScore}`：个人最佳分数
- `{scoreGap}`：当前得分与个人最佳分数的差值
- `{pbAcc}`：个人最佳准确率
- `{accGap}`：当前准确率与个人最佳准确率的差值
- `{acc}`：当前准确率
- `{rank}`：当前评级
- `{total}`：总物量
- `{hit}`：已经击打/拾取/跳过齿轮的物量
- `{song}`：谱面名称
- `{diff}`：谱面难度（数字）
- `{level}`：谱面难度（文本）
- `{author}`：谱面作者
- `{overview}`：TP / AP 指示，若当前准确率低于 100%，则改为显示当前准确率
- `{stats}`：Miss / Great / Early / Late 等数据
- `{pbStats}`：个人最佳记录下的 Miss / Great / Early / Late 等数据
- `{pbStatsGap}`：当前与个人最佳记录下的 Miss / Great / Early / Late 等数据的差值
- `{pbGreat}`：个人最佳记录下的 Great 计数
- `{pbMissOther}`：个人最佳记录下的 Miss 计数（不包括可收集 Note）
- `{pbMissCollectible}`：个人最佳记录下的 Miss 计数（仅包括可收集 Note）
- `{pbEarly}`：个人最佳记录下的 Early 计数
- `{pbLate}`：个人最佳记录下的 Late 计数
- `{skySpeed}`：当前天空速度
- `{groundSpeed}`：当前地面速度

提示：若配置项提示支持富文本，代表该配置项将会依照您填入的富文本生成对应文本。例：`<size=40><color=#e1bb8a>{total}</color></size>` 。如果您不知道什么是富文本，请自行搜索；如果您需要换行，使用 `\n` 。

## 如何使用

- 根据下列出的依赖项安装 MelonLoader 至 Muse Dash
- 下载 [Latest Release](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) 并将 `Info+.dll` 放置于 `.\MuseDash\Mods\` 目录下
- 启动游戏即可

## 依赖项

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 或 v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## 开发者说明

我对 Unity 游戏模组开发了解不多，目前主要致力于让功能正常运作，实现方式可能不够优雅。如果你有任何问题或愿意帮助改进这个模组，欢迎提交 [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) 或 [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare)，非常感谢你的支持！
