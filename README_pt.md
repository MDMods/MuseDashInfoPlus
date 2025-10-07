# Info+

[English](README.md) | [简体中文](README_zh.md) | [繁體中文](README_zh-TW.md) | [日本語](README_ja.md) | [한국어](README_ko.md) | [Français](README_fr.md) | [Deutsch](README_de.md) | [Español](README_es.md) | [Русский](README_ru.md) | Português

> **Atenção:** Este README foi gerado por IA e pode conter imprecisões ou ambiguidades. Para informações precisas, por favor consulte os README oficialmente mantidos em [chinês](README_zh.md) ou [inglês](README.md).

## Visão geral

Info+ é um mod altamente personalizável para Muse Dash que exibe informações adicionais no jogo.

Este mod foi inspirado no MuseDashCustomPlay.

## Recursos

Exibe várias informações incluindo **Informações do chart, Precisão, Contadores Miss/Great/Early/Late/Hit/Total, Diferença de pontuação/precisão do recorde pessoal, Velocidade céu/chão** e muito mais.

Todos os elementos de dados podem ser livremente personalizados em termos de visibilidade, posição, tamanho, cor, fonte, formatação e até contorno.

## Notas importantes

- As contagens Miss/Great/Early/Late dos recordes pessoais não são armazenadas no jogo base e só podem ser salvas quando o Info+ está instalado. Você precisa alcançar pelo menos um recorde pessoal com o Info+ instalado para que a diferença de estatísticas do recorde pessoal funcione
- Se você carregou o mod [SongDesc](https://github.com/mdmods/songdesc), as informações do chart serão desativadas por padrão. Use a configuração para reativá-las
- Alguns dados podem não funcionar corretamente para charts Touhou Danmaku devido a problemas de compatibilidade
- O contador de notas trata notas mantidas como duas notas separadas (contando tanto o início quanto o fim), enquanto o contador de Miss e o jogo vanilla contam cada nota mantida como uma única nota

## Prévias

![Prévia 1](Static/Preview1.webp)

![Prévia 2](Static/Preview2.webp)

![Prévia 3](Static/Preview3.webp)

## Configuração

Os arquivos de configuração são organizados por categoria e armazenados no diretório
`.\MuseDash\UserData\Info+\`. Todas as entradas de configuração incluem comentários. Por favor, entenda o propósito de cada configuração antes de fazer modificações.

Todos os arquivos de configuração são **gerados automaticamente na primeira execução**. Após modificar a configuração, **salve o arquivo para que tenha efeito** (se você estiver no jogo, terá efeito no próximo jogo).

- `MainConfigs.yml`: Arquivo de configuração principal
- `TextFieldLowerLeftConfigs.yml`: Configuração de texto para a área inferior esquerda da tela
- `TextFieldLowerRightConfigs.yml`: Configuração de texto para a área inferior direita da tela
- `TextFieldScoreBelowConfigs.yml`: Configuração de texto para a área à direita do rótulo "SCORE" (posição permanece fixa em relação ao rótulo)
- `TextFieldScoreRightConfigs.yml`: Configuração de texto para a área à direita da exibição de pontuação (posição permanece fixa em relação à pontuação)
- `TextFieldUpperLeftConfigs.yml`: Configuração de texto para a área abaixo do rótulo "SCORE" (posição permanece fixa em relação ao rótulo)
- `TextFieldUpperRightConfigs.yml`: Configuração de texto para a área superior direita da tela
- `AdvancedConfigs.yml`: Apenas para usuários avançados - não modifique a menos que entenda os parâmetros

### Marcadores de dados

Nos arquivos de configuração de texto, você encontrará entradas como `text: '{overview} / {stats}'`. Os
marcadores `{dataName}` serão substituídos de acordo com as seguintes regras. Estes podem ser combinados livremente:

- `{pbScore}`: Melhor pontuação pessoal
- `{scoreGap}`: Diferença entre a pontuação atual e a melhor pontuação pessoal
- `{pbAcc}`: Melhor precisão pessoal
- `{accGap}`: Diferença entre a precisão atual e a melhor precisão pessoal  
- `{acc}`: Precisão atual
- `{rank}`: Classificação atual
- `{total}`: Contagem total de notas
- `{hit}`: Contagem atual de acertos/coletados/pulados
- `{song}`: Nome do chart
- `{diff}`: Dificuldade do chart (numérica)
- `{level}`: Dificuldade do chart (texto)
- `{author}`: Autor do chart
- `{overview}`: Indicador TP/AP, mostra precisão atual se abaixo de 100%
- `{stats}`: Contadores Miss/Great/Early/Late
- `{pbStats}`: Contadores Miss/Great/Early/Late do recorde pessoal
- `{pbStatsGap}`: Diferença entre os contadores Miss/Great/Early/Late atuais e do recorde pessoal
- `{pbGreat}`: Contagem Great do recorde pessoal
- `{pbMissOther}`: Contagem Miss do recorde pessoal (excluindo misses colecionáveis)
- `{pbMissCollectible}`: Contagem Miss do recorde pessoal (apenas misses colecionáveis)
- `{pbEarly}`: Contagem Early do recorde pessoal
- `{pbLate}`: Contagem Late do recorde pessoal
- `{skySpeed}`: Velocidade céu atual
- `{groundSpeed}`: Velocidade chão atual

Nota: Texto rico é suportado para algumas entradas de configuração. Por exemplo:
`<size=40><color=#e1bb8a>{total}</color></size>`. Se você não está familiarizado com texto rico, por favor pesquise. Para quebras de linha, use `\n`.

## Instalação

1. Instale o MelonLoader no Muse Dash com base na dependência listada abaixo
2. Baixe a [última versão](https://github.com/KARPED1EM/MuseDashInfoPlus/releases) e coloque `Info+.dll` no diretório `.\MuseDash\Mods\`
3. Inicie o jogo e divirta-se

## Dependências

- [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) v0.6.1 ou v0.7.0
- [Muse Dash on Steam](https://store.steampowered.com/app/774171/Muse_Dash/)

## Notas para desenvolvedores

Sou relativamente novo em modding do Unity e me concentrei principalmente em fazer as coisas funcionarem. A implementação pode não ser a mais elegante. Se você tiver alguma dúvida ou quiser ajudar a melhorar este mod, fique à vontade para abrir uma [Issue](https://github.com/KARPED1EM/MuseDashInfoPlus/issues/new) ou enviar um [Pull Request](https://github.com/KARPED1EM/MuseDashInfoPlus/compare). Seu apoio é muito apreciado!
