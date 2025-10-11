# Info+

[English](README.md) | [简体中文](README_zh-CN.md) | [繁體中文](README_zh-TW.md) | 日本語 | [한국어](README_ko.md) | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

> **注意:** このREADMEはAIによって生成されたものであり、不正確な内容や曖昧な表現が含まれている可能性があります。正確な情報については、公式にメンテナンスされている[中国語版](README_zh-CN.md)または[英語版](README.md)のREADMEを参照してください。

## 概要

Info+は、ゲーム内の追加情報を表示する、高度にカスタマイズ可能なMuse Dashゲームモッドです。

このモッドはMuseDashCustomPlayからインスピレーションを得ています。

## 機能

**譜面情報、精度、Miss/Great/Early/Late/Hit/総ノーツ数、個人ベストとのスコア/精度差、空/地上速度**などの様々な情報を表示します。

すべてのデータ要素は、表示/非表示、位置、サイズ、色、フォント、書式設定、さらにはアウトラインまで自由にカスタマイズできます。

## 重要な注意事項

- ⌨️ **UI切替ホットキー**：**F10** を押すとすべてのゲーム内UIの表示を切り替えます。UIが誤って消えた場合は、もう一度 **F10** を押すと復元できます。このホットキーは `MainConfigs.yml` で変更できます
- 個人ベスト記録のMiss/Great/Early/Lateカウントは基本ゲームには保存されておらず、Info+がインストールされている場合にのみ保存できます。個人ベスト統計の差分を機能させるには、Info+をインストールした状態で少なくとも1回個人ベストスコアを達成する必要があります
- [SongDesc](https://github.com/mdmods/songdesc)モッドをロードした場合、譜面情報はデフォルトで無効になります。設定で再度有効にしてください
- 互換性の問題により、東方弾幕譜面では一部のデータが正常に機能しない場合があります
- ノーツカウンターはホールドノーツを2つの別々のノーツとして扱います（開始と終了の両方をカウント）が、Missカウンターとバニラゲームは各ホールドノーツを1つのノーツとしてカウントします

## プレビュー

![プレビュー1](static/Preview1.webp)

![プレビュー2](static/Preview2.webp)

![プレビュー3](static/Preview3.webp)

## 設定

設定ファイルはカテゴリごとに整理され、`.\MuseDash\UserData\Info+\`ディレクトリに保存されています。すべての設定エントリにはコメントが含まれています。変更を行う前に、各設定の目的を理解してください。

すべての設定ファイルは**初回起動時に自動生成**されます。設定を変更した後、**ファイルを保存すると即座に有効になります**。

- `MainConfigs.yml`: メイン設定ファイル
- `TextFieldLowerLeftConfigs.yml`: 画面左下のテキスト設定
- `TextFieldLowerRightConfigs.yml`: 画面右下のテキスト設定
- `TextFieldScoreBelowConfigs.yml`: "SCORE"ラベルの右側のテキスト設定（ラベルに対して固定位置）
- `TextFieldScoreRightConfigs.yml`: スコア表示の右側のテキスト設定（スコアに対して固定位置）
- `TextFieldUpperLeftConfigs.yml`: "SCORE"ラベルの下のテキスト設定（ラベルに対して固定位置）
- `TextFieldUpperRightConfigs.yml`: 画面右上のテキスト設定
- `AdvancedConfigs.yml`: 上級ユーザー専用 - パラメータを理解していない限り変更しないでください

### データプレースホルダー

テキスト設定ファイルには、`text: '{overview} / {stats}'`のようなエントリがあります。
`{dataName}`プレースホルダーは、以下のルールに従って置き換えられます。これらは自由に組み合わせることができます：

- `{pbScore}`: 個人ベストスコア
- `{scoreGap}`: 現在のスコアと個人ベストスコアの差
- `{pbAcc}`: 個人ベスト精度
- `{accGap}`: 現在の精度と個人ベスト精度の差  
- `{acc}`: 現在の精度
- `{rank}`: 現在のランク
- `{total}`: 総ノーツ数
- `{hit}`: 現在のヒット/収集/ジャンプ数
- `{song}`: 譜面名
- `{diff}`: 譜面難易度（数値）
- `{level}`: 譜面難易度（テキスト）
- `{author}`: 譜面作者
- `{bpm}`: 譜面BPM（固定データ、リアルタイムBPMではありません）
- `{overview}`: TP/APインジケーター、100%未満の場合は現在の精度を表示
- `{stats}`: Miss/Great/Early/Lateカウント
- `{pbStats}`: 個人ベストMiss/Great/Early/Lateカウント
- `{pbStatsGap}`: 現在と個人ベストのMiss/Great/Early/Lateカウントの差
- `{pbGreat}`: 個人ベストGreatカウント
- `{pbMissOther}`: 個人ベストMissカウント（収集可能ミスを除く）
- `{pbMissCollectible}`: 個人ベストMissカウント（収集可能ミスのみ）
- `{pbEarly}`: 個人ベストEarlyカウント
- `{pbLate}`: 個人ベストLateカウント
- `{skySpeed}`: 現在の空速度
- `{groundSpeed}`: 現在の地上速度
- `{time}`: システムのローカル時刻

注意: 一部の設定エントリではリッチテキストがサポートされています。例：
`<size=40><color=#e1bb8a>{total}</color></size>`。リッチテキストに不慣れな場合は、検索してください。改行には`\n`を使用します。

## インストール

1. 以下に記載されている依存関係に基づいて、MelonLoaderをMuse Dashにインストールします
2. [最新リリース](https://github.com/KARPED1EM/MuseDashInfoPlus/releases)をダウンロードし、`Info+.dll`を`.\MuseDash\Mods\`ディレクトリに配置します
3. ゲームを起動してお楽しみください

## 依存関係

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 または v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## 開発者向け注意事項

私はUnityモディングに比較的慣れておらず、主に機能を動作させることに重点を置いています。実装は最もエレガントではないかもしれません。質問がある場合、またはこのモッドの改善にご協力いただける場合は、お気軽に[Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new)を開くか、[Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare)を提出してください。ご支援に心から感謝いたします！
