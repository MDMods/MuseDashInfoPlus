# MuseDashInfo+

[English](README.md) | [中文](README_zh.md)

### 概述

MuseDashInfo+ 是一个轻量级的 MuseDash 游戏模组，用于显示额外的游戏信息。

本模组受 MuseDashCustomPlay 启发。

### 功能特性

- **谱面信息**：在游戏界面右上角显示当前歌曲名称与铺面难度等级
- **物量计数器**：在游戏界面左下角实时显示谱面总物量和已击打数量
- **漏击计数器**：在游戏界面左上角实时显示当前的 Great/Miss 和音符/红心遗漏数
- **当前准确率**：在游戏界面左上角实时显示当前准确率（因技术限制略有偏差）
- **最高分/差值**：在游戏界面分数右侧实时显示当前谱面最高分/与最高分的差距
- **替换 Miss 数**：游戏原版的 Miss 并不包括幽灵以及受伤硬直状态中的 Miss，开启该选项将结算页面的 Miss 数替换为模组计算出的实际 Miss 数（该选项也会修复原版端双押只击打下面的 Note 造成多出来一个 Miss 的 Bug）

以上功能均可单独启用或关闭，部分支持自定义文本格式。

### 注意事项

- 若您加载了 [SongDesc](https://github.com/MDMods/SongDesc) 模组，谱面信息将默认隐藏
- 东方特殊谱面不支持物量计数器/最高分/最高分差值
- 物量计数器会将长条视为两个 Note，首尾各算一个，而 Miss 计数器与游戏原版都将长条视为一个 Note

### 预览

![预览1](Static/Preview1.webp)

![预览2](Static/Preview2.webp)

![预览3](Static/Preview3.webp)

### 配置

您可以在 `..\MuseDash\UserData\Info+.cfg` 里更改该模组的配置。

该文件会在**首次启动后自动生成**，修改配置后**重启游戏生效**。以下是配置示例以及默认值：

```
[Info+]

# 显示歌曲名
DisplayChartName = true

# 显示谱面难度
DisplayChartDifficulty = true

# 自定义歌曲难度文本
# {diff} 将被替换为 EASY/HARD/MASTER
# {level} 将被替换为谱面等级
CustomChartDifficultyFormat = "{diff} - Level {level}"

# 显示物量计数器
DisplayNoteCounts = true

# 自定义物量计数器文本
# {total} 将被替换为谱面总物量
# {hit} 将被替换为当前已击打数量
CustomNoteCountsFormat = "{hit} of {total} notes"

# 显示漏击计数器
DisplayMissCounts = true

# 显示当前准确率
DisplayAccuracy = true

# 显示当前与最高分的分数差距
DisplayScoreGap = true

# 显示当前谱面历史最高分数
DisplayHighestScore = false

# 以模组计算的 Miss 数替代结算页面的 Miss 数
ReplaceResultsScreenMissCount = true

# 自定义各个数据之间的分隔符
CustomSeparator = " / "
```

提示：上述自定义字符串均支持富文本，例：`<size=40><color=#e1bb8a>{total}</color></size>`。

### 如何使用

- 根据下列出的依赖项安装 MelonLoader 至 Muse Dash
- 下载 [Latest Release](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) 并将 `Info+.dll` 放置于 `..\MuseDash\Mods\` 目录下
- 启动游戏即可

### 依赖项

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

### 开发者说明

我对 Unity 游戏模组开发了解不多，目前主要致力于让功能正常运作，实现方式可能不够优雅。如果你有任何问题或愿意帮助改进这个模组，欢迎提交 [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) 或 [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare)，非常感谢你的支持！
