# Info+

English | [简体中文](README_zh-CN.md) | [繁體中文](README_zh-TW.md) | [日本語](README_ja.md) | [한국어](README_ko.md) | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

## Overview

Info+ is a highly customizable Muse Dash game mod that displays additional in-game information.

This mod is inspired by MuseDashCustomPlay.

## Features

Displays various information including **Chart Info, Accuracy, Miss/Great/Early/Late/Hit/Total Counts, Score/Accuracy Gap from Personal Best, Sky/Ground Speed** and so on.

All data elements can be freely customized in terms of visibility, position, size, color, font, formatting and even outline.

## Important Notes

- ⌨️ **UI Toggle Hotkey**: Press **F10** to toggle visibility of all in-game UI. If the UI disappears accidentally, press **F10** again to restore it. This hotkey can be changed in `MainConfigs.yml`
- The Miss/Great/Early/Late counts of personal best records are not stored in the base game, and can only be saved when Info+ is installed. You need to achieve at least one personal best score with Info+ installed for the personal best stats gap to work
- If you loaded [SongDesc](https://github.com/mdmods/songdesc) mod, Chart Info will be disabled by default, use configuration to re-enable
- Some data may not function properly for Touhou Danmaku charts due to compatibility issues
- The Note Counter treats hold notes as two separate notes (counting both the start and end), while the Miss Counter and vanilla game count each hold note as a single note

## Previews

![Preview 1](static/Preview1.webp)

![Preview 2](static/Preview2.webp)

![Preview 3](static/Preview3.webp)

## Configuration

Configuration files are organized by category and stored in the
`.\MuseDash\UserData\Info+\` directory. All configuration entries include comments. Please understand the purpose of each setting before making modifications.

All configuration files are **automatically generated upon first launch**. After modifying the configuration, **save the file and changes will take effect immediately**.

- `MainConfigs.yml`: Primary configuration file
- `TextFieldLowerLeftConfigs.yml`: Text configuration for lower-left screen area
- `TextFieldLowerRightConfigs.yml`: Text configuration for lower-right screen area
- `TextFieldScoreBelowConfigs.yml`: Text configuration for area right of the "SCORE" label (position remains fixed relative to label)
- `TextFieldScoreRightConfigs.yml`: Text configuration for area right of the score display (position remains fixed relative to score)
- `TextFieldUpperLeftConfigs.yml`: Text configuration for area below the "SCORE" label (position remains fixed relative to label)
- `TextFieldUpperRightConfigs.yml`: Text configuration for upper-right screen area
- `AdvancedConfigs.yml`: For advanced users only - do not modify unless you understand the parameters

### Data Placeholders

In text configuration files, you'll find entries like `text: '{overview} / {stats}'`. The
`{dataName}` placeholders will be replaced according to the following rules. These can be combined freely:

- `{pbScore}`: Personal best score
- `{scoreGap}`: Difference between current score and personal best score
- `{pbAcc}`: Personal best accuracy
- `{accGap}`: Difference between current accuracy and personal best accuracy  
- `{acc}`: Current accuracy
- `{rank}`: Current rank
- `{total}`: Total note count
- `{hit}`: Current hit/collected/jumped count
- `{song}`: Chart name
- `{diff}`: Chart difficulty (numeric)
- `{level}`: Chart difficulty (text)
- `{author}`: Chart author
- `{bpm}`: Chart BPM (fixed data, not real-time BPM)
- `{overview}`: TP/AP indicator, shows current accuracy if below 100%
- `{stats}`: Miss/Great/Early/Late counts
- `{pbStats}`: Personal best Miss/Great/Early/Late counts
- `{pbStatsGap}`: Difference between current and personal best Miss/Great/Early/Late counts
- `{pbGreat}`: Personal best Great count
- `{pbMissOther}`: Personal best Miss count (excluding collectible misses)
- `{pbMissCollectible}`: Personal best Miss count (collectible misses only)
- `{pbEarly}`: Personal best Early count
- `{pbLate}`: Personal best Late count
- `{skySpeed}`: Current sky speed
- `{groundSpeed}`: Current ground speed
- `{time}`: System local time

Note: Rich text is supported for some configuration entries. For example:
`<size=40><color=#e1bb8a>{total}</color></size>`. If you're unfamiliar with rich text, please google it. For line breaks, use `\n`.

## Installation

1. Install MelonLoader into Muse Dash based on the dependency listed below
2. Download the [Latest Release](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) and place `Info+.dll` in the `.\MuseDash\Mods\` directory
3. Launch the game and enjoy

## Dependencies

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 or v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## Developer Notes

I'm relatively new to Unity modding and have focused primarily on making the things work. The implementation might not be the most elegant. If you have any questions or would like to help improve this mod, please feel free to open an [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) or submit a [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare). Your support is greatly appreciated!
