using System.Reflection;
using MDIP.Globals;

namespace MDIP.Ecosystem;

// Anti-corruption layer to CustomAlbums' own save, reached by reflection as a SOFT dependency (Info+
// does not reference CustomAlbums.dll). CAM keeps custom-chart PBs in its own CustomAlbumsSave (the
// native high-score store holds nothing for "999-" uids), exposed via the public
// SaveExtensions.GetChartSaveDataFromUid(save, uid).Highest[difficulty] -> ChartSave.
//
// All the reflection — type/method/field resolution against the CustomAlbums assembly, the
// generic-dictionary index, the ChartSave field reads — is confined here and resolved once. CAM
// absent or its API shape changed => every call cleanly returns false (no PB), never throws out.
internal static class CamSaveClient
{
    private static bool _resolved;
    private static FieldInfo _saveDataField;          // SaveManager.SaveData (internal static)
    private static MethodInfo _getChartSaveData;       // SaveExtensions.GetChartSaveDataFromUid(save, uid)
    private static PropertyInfo _highestProp;          // SaveData.Highest : Dictionary<int, ChartSave>
    private static MethodInfo _highestContainsKey;     // Dictionary<int,ChartSave>.ContainsKey(int)
    private static PropertyInfo _highestItem;          // Dictionary<int,ChartSave>.this[int]
    private static PropertyInfo _chartSaveScore;       // ChartSave.Score : int
    private static PropertyInfo _chartSaveAccuracy;    // ChartSave.Accuracy : float (0..100)

    // Returns the saved PB for the CAM chart + 1-based difficulty. Accuracy is 0..100 (CAM's own scale).
    public static bool TryGetPersonalBest(string uid, int difficulty, out int score, out float accuracyPercent)
    {
        score = 0;
        accuracyPercent = 0f;

        EnsureResolved();
        if (_getChartSaveData == null)
            return false;

        try
        {
            var save = _saveDataField.GetValue(null);
            if (save == null)
                return false;

            var chartSaveData = _getChartSaveData.Invoke(null, [save, uid]);
            if (chartSaveData == null)
                return false;

            var highest = _highestProp.GetValue(chartSaveData);
            if (highest == null || !(bool)_highestContainsKey.Invoke(highest, [difficulty]))
                return false;

            var chartSave = _highestItem.GetValue(highest, [difficulty]);
            if (chartSave == null)
                return false;

            score = (int)_chartSaveScore.GetValue(chartSave);
            accuracyPercent = (float)_chartSaveAccuracy.GetValue(chartSave);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warn($"CamSaveClient read failed: {ex.Message}");
            return false;
        }
    }

    private static void EnsureResolved()
    {
        if (_resolved)
            return;
        _resolved = true;

        try
        {
            var saveManager = FindType("CustomAlbums.Managers.SaveManager");
            var saveExtensions = FindType("CustomAlbums.Utilities.SaveExtensions");
            if (saveManager == null || saveExtensions == null)
            {
                Log.Info("CamSaveClient: CustomAlbums not present; CAM personal-best unavailable.");
                return;
            }

            _saveDataField = saveManager.GetField("SaveData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            _getChartSaveData = saveExtensions.GetMethod("GetChartSaveDataFromUid", BindingFlags.Public | BindingFlags.Static);
            if (_saveDataField == null || _getChartSaveData == null)
            {
                Log.Warn("CamSaveClient: CustomAlbums save API not found (version mismatch?).");
                _getChartSaveData = null;
                return;
            }

            // SaveData.Highest : Dictionary<int, ChartSave> — resolve the dict + ChartSave members from
            // the return type, no instance needed.
            _highestProp = _getChartSaveData.ReturnType.GetProperty("Highest");
            var dictType = _highestProp?.PropertyType;
            var chartSaveType = dictType?.GetGenericArguments() is { Length: 2 } args ? args[1] : null;
            _highestContainsKey = dictType?.GetMethod("ContainsKey", [typeof(int)]);
            _highestItem = dictType?.GetProperty("Item");
            _chartSaveScore = chartSaveType?.GetProperty("Score");
            _chartSaveAccuracy = chartSaveType?.GetProperty("Accuracy");

            if (_highestProp == null || _highestContainsKey == null || _highestItem == null ||
                _chartSaveScore == null || _chartSaveAccuracy == null)
            {
                Log.Warn("CamSaveClient: CustomAlbums save shape not as expected (version mismatch?).");
                _getChartSaveData = null;
                return;
            }

            Log.Info("CamSaveClient: CustomAlbums save API resolved.");
        }
        catch (Exception ex)
        {
            Log.Warn($"CamSaveClient: resolution failed: {ex.Message}");
            _getChartSaveData = null;
        }
    }

    private static Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName, false);
            if (type != null)
                return type;
        }
        return null;
    }
}
