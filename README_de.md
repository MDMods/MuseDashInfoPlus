# Info+

[English](README.md) | [简体中文](README_zh-CN.md) | [繁體中文](README_zh-TW.md) | [日本語](README_ja.md) | [한국어](README_ko.md) | [Français](README_fr.md) | Deutsch | [Español](README_es.md) | [Русский](README_ru.md) | [Português](README_pt.md)

> **Hinweis:** Diese README wurde von KI generiert und kann Ungenauigkeiten oder mehrdeutige Aussagen enthalten. Für genaue Informationen beziehen Sie sich bitte auf die offiziell gepflegten [chinesischen](README_zh-CN.md) oder [englischen](README.md) README-Dateien.

## Übersicht

Info+ ist ein hochgradig anpassbarer Muse Dash Game-Mod, der zusätzliche In-Game-Informationen anzeigt.

Dieser Mod wurde von MuseDashCustomPlay inspiriert.

## Funktionen

Zeigt verschiedene Informationen an, einschließlich **Chart-Info, Genauigkeit, Miss/Great/Early/Late/Hit/Gesamt-Anzahl, Score/Genauigkeits-Abstand von persönlichem Rekord, Himmel/Boden-Geschwindigkeit** und vieles mehr.

Alle Datenelemente können in Bezug auf Sichtbarkeit, Position, Größe, Farbe, Schriftart, Formatierung und sogar Umriss frei angepasst werden.

## Wichtige Hinweise

- ⌨️ **UI-Umschalt-Hotkey**: Drücken Sie **F10**, um die Anzeige aller In-Game-UI umzuschalten. Wenn die UI versehentlich verschwindet, drücken Sie erneut **F10**, um sie wiederherzustellen. Dieser Hotkey kann in `MainConfigs.yml` geändert werden
- Die Miss/Great/Early/Late-Zählungen persönlicher Rekorde werden nicht im Basisspiel gespeichert und können nur gespeichert werden, wenn Info+ installiert ist. Sie müssen mit installiertem Info+ mindestens einen persönlichen Rekord erzielen, damit die persönliche Rekord-Statistikdifferenz funktioniert
- Wenn Sie den [SongDesc](https://github.com/mdmods/songdesc) Mod geladen haben, werden Chart-Informationen standardmäßig deaktiviert. Verwenden Sie die Konfiguration, um sie wieder zu aktivieren
- Einige Daten funktionieren möglicherweise nicht richtig für Touhou Danmaku-Charts aufgrund von Kompatibilitätsproblemen
- Der Notenzähler behandelt gehaltene Noten als zwei separate Noten (sowohl Anfang als auch Ende werden gezählt), während der Miss-Zähler und das Vanilla-Spiel jede gehaltene Note als einzelne Note zählen

## Vorschauen

![Vorschau 1](static/Preview1.webp)

![Vorschau 2](static/Preview2.webp)

![Vorschau 3](static/Preview3.webp)

## Konfiguration

Konfigurationsdateien sind nach Kategorien organisiert und im Verzeichnis
`.\MuseDash\UserData\Info+\` gespeichert. Alle Konfigurationseinträge enthalten Kommentare. Bitte verstehen Sie den Zweck jeder Einstellung, bevor Sie Änderungen vornehmen.

Alle Konfigurationsdateien werden **beim ersten Start automatisch generiert**. Nach dem Ändern der Konfiguration **speichern Sie die Datei und die Änderungen werden sofort wirksam**.

- `MainConfigs.yml`: Primäre Konfigurationsdatei
- `TextFieldLowerLeftConfigs.yml`: Textkonfiguration für den unteren linken Bildschirmbereich
- `TextFieldLowerRightConfigs.yml`: Textkonfiguration für den unteren rechten Bildschirmbereich
- `TextFieldScoreBelowConfigs.yml`: Textkonfiguration für den Bereich rechts vom "SCORE"-Label (Position bleibt relativ zum Label fixiert)
- `TextFieldScoreRightConfigs.yml`: Textkonfiguration für den Bereich rechts von der Score-Anzeige (Position bleibt relativ zum Score fixiert)
- `TextFieldUpperLeftConfigs.yml`: Textkonfiguration für den Bereich unter dem "SCORE"-Label (Position bleibt relativ zum Label fixiert)
- `TextFieldUpperRightConfigs.yml`: Textkonfiguration für den oberen rechten Bildschirmbereich
- `AdvancedConfigs.yml`: Nur für fortgeschrittene Benutzer - nicht ändern, es sei denn, Sie verstehen die Parameter

### Daten-Platzhalter

In Textkonfigurationsdateien finden Sie Einträge wie `text: '{overview} / {stats}'`. Die
`{dataName}`-Platzhalter werden gemäß den folgenden Regeln ersetzt. Diese können frei kombiniert werden:

- `{pbScore}`: Persönlicher Rekord-Score
- `{scoreGap}`: Differenz zwischen aktuellem Score und persönlichem Rekord-Score
- `{pbAcc}`: Persönliche Rekord-Genauigkeit
- `{accGap}`: Differenz zwischen aktueller Genauigkeit und persönlicher Rekord-Genauigkeit  
- `{acc}`: Aktuelle Genauigkeit
- `{rank}`: Aktueller Rang
- `{total}`: Gesamtanzahl der Noten
- `{hit}`: Aktuelle Hit/Gesammelte/Gesprungene-Anzahl
- `{song}`: Chart-Name
- `{diff}`: Chart-Schwierigkeit (numerisch)
- `{level}`: Chart-Schwierigkeit (Text)
- `{author}`: Chart-Autor
- `{levelDesigner}`: Level-Designer des Charts
- `{bpm}`: Chart-BPM (feste Daten, nicht Echtzeit-BPM)
- `{overview}`: TP/AP-Indikator, zeigt aktuelle Genauigkeit, wenn unter 100%
- `{stats}`: Miss/Great/Early/Late-Zählungen
- `{pbStats}`: Persönliche Rekord Miss/Great/Early/Late-Zählungen
- `{pbStatsGap}`: Differenz zwischen aktuellen und persönlichen Rekord Miss/Great/Early/Late-Zählungen
- `{pbGreat}`: Persönliche Rekord Great-Zählung
- `{pbMissOther}`: Persönliche Rekord Miss-Zählung (ohne sammelbare Misses)
- `{pbMissCollectible}`: Persönliche Rekord Miss-Zählung (nur sammelbare Misses)
- `{pbEarly}`: Persönliche Rekord Early-Zählung
- `{pbLate}`: Persönliche Rekord Late-Zählung
- `{skySpeed}`: Aktuelle Himmel-Geschwindigkeit
- `{groundSpeed}`: Aktuelle Boden-Geschwindigkeit
- `{time}`: Systemzeit

Hinweis: Rich Text wird für einige Konfigurationseinträge unterstützt. Zum Beispiel:
`<size=40><color=#e1bb8a>{total}</color></size>`. Wenn Sie mit Rich Text nicht vertraut sind, suchen Sie bitte danach. Für Zeilenumbrüche verwenden Sie `\n`.

## Installation

1. Installieren Sie MelonLoader in Muse Dash basierend auf der unten aufgeführten Abhängigkeit
2. Laden Sie das [Latest Release](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) herunter und legen Sie `Info+.dll` im Verzeichnis `.\MuseDash\Mods\` ab
3. Starten Sie das Spiel und genießen Sie es

## Abhängigkeiten

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 oder v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## Entwicklerhinweise

Ich bin relativ neu im Unity-Modding und habe mich hauptsächlich darauf konzentriert, die Dinge zum Laufen zu bringen. Die Implementierung ist möglicherweise nicht die eleganteste. Wenn Sie Fragen haben oder helfen möchten, diesen Mod zu verbessern, können Sie gerne ein [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) öffnen oder einen [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare) einreichen. Ihre Unterstützung wird sehr geschätzt!
