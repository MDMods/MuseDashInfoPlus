using JetBrains.Annotations;
using MDIP.Domain.Configs;

namespace MDIP.Application.Services.Configuration;

public class ConfigAccessor : IConfigAccessor
{
    public MainConfigs Main => ConfigService.GetConfig<MainConfigs>(nameof(MainConfigs));
    public AdvancedConfigs Advanced => ConfigService.GetConfig<AdvancedConfigs>(nameof(AdvancedConfigs));
    public TextFieldLowerLeftConfigs TextFieldLowerLeft => ConfigService.GetConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
    public TextFieldLowerRightConfigs TextFieldLowerRight => ConfigService.GetConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
    public TextFieldScoreBelowConfigs TextFieldScoreBelow => ConfigService.GetConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
    public TextFieldScoreRightConfigs TextFieldScoreRight => ConfigService.GetConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
    public TextFieldUpperLeftConfigs TextFieldUpperLeft => ConfigService.GetConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
    public TextFieldUpperRightConfigs TextFieldUpperRight => ConfigService.GetConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));

    [UsedImplicitly] public required IConfigService ConfigService { get; init; }
}