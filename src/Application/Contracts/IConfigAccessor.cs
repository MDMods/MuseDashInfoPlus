using MDIP.Domain.Configs;

namespace MDIP.Application.Contracts;

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