# MuseDashInfo+

[English](README.md) | [中文](README_zh.md)

## Overview

MuseDashInfo+ is a highly customizable Muse Dash game mod that displays additional in-game information.

This mod is inspired by MuseDashCustomPlay.

## Features

Displays various information including **Chart Info (Song Name/Difficulty/Author/Level), Accuracy, Miss Count, Great Count, Early Count, Late Count, Note Counter (Hit/Total), Personal Best Score, and Score Gap**.

All data elements can be freely customized in terms of visibility, position, size, color, font, and even custom text formatting.

## Important Notes

- If you loaded [SongDesc](https://github.com/mdmods/songdesc) mod, Chart Info will be disabled by default, use configuration to re-enable
- Text outline feature is currently not supported and will be available in future updates
- Some data may not function properly for Touhou Danmaku charts due to compatibility issues
- The Note Counter treats hold notes as two separate notes (counting both the start and end), while the Miss Counter and vanilla game count each hold note as a single note

## Previews

![Preview 1](Static/Preview1.webp)

![Preview 2](Static/Preview2.webp)

![Preview 3](Static/Preview3.webp)

## Configuration

Configuration files are organized by category and stored in the `..\MuseDash\UserData\Info+\` directory. All configuration entries include comments. Please understand the purpose of each setting before making modifications.

All configuration files are **automatically generated upon first launch**. Changes take effect after **restarting the game**.

`MainConfigs.yml`: Primary configuration file
`TextFieldLowerLeftConfigs.yml`: Text configuration for lower-left screen area
`TextFieldLowerRightConfigs.yml`: Text configuration for lower-right screen area
`TextFieldScoreBelowConfigs.yml`: Text configuration for area right of the "SCORE" label (position remains fixed relative to label)
`TextFieldScoreRightConfigs.yml`: Text configuration for area right of the score display (position remains fixed relative to score)
`TextFieldUpperLeftConfigs.yml`: Text configuration for area below the "SCORE" label (position remains fixed relative to label)
`TextFieldUpperRightConfigs.yml`: Text configuration for upper-right screen area
`AdvancedConfigs.yml`: For advanced users only - do not modify unless you understand the parameters

Note: Rich text are supported for some configuration entries. For example: `<size=40><color=#e1bb8a>{total}</color></size>`. If you're unfamiliar with rich text, please google it. For line breaks, use `\n`.

## Installation

1. Install MelonLoader into Muse Dash based on the dependency listed below
2. Download the [Latest Release ](https://github.com/KARPED1EM/MuseDashInfoPlus/releases)and place `Info+.dll` in the `..\MuseDash\Mods\` directory
3. Launch the game and enjoy

## Dependencies

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## Developer Notes

I'm relatively new to Unity modding and have focused primarily on making the things work. The implementation might not be the most elegant. If you have any questions or would like to help improve this mod, please feel free to open an [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) or submit a [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare). Your support is greatly appreciated!
