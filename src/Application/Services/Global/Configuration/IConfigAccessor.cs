using MDIP.Core.Domain.Configs;

namespace MDIP.Application.Services.Global.Configuration;

public interface IConfigAccessor
{
    MainConfigs Main { get; }
    AdvancedConfigs Advanced { get; }
    TextFieldLowerLeftConfigs TextFieldLowerLeft { get; }
    TextFieldLowerRightConfigs TextFieldLowerRight { get; }
    TextFieldScoreBelowConfigs TextFieldScoreBelow { get; }
    TextFieldScoreRightConfigs TextFieldScoreRight { get; }
    TextFieldUpperLeftConfigs TextFieldUpperLeft { get; }
    TextFieldUpperRightConfigs TextFieldUpperRight { get; }
}