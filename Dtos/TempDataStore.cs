using Career_Guidance_Platform.Dtos.Career;

namespace Career_Guidance_Platform.Dtos;

public class TempDataStore
{
    private static readonly Dictionary<int, List<TopCareerDto>> _store = new();

    public static void Set(int resultId, List<TopCareerDto> data)
    {
        _store[resultId] = data;
    }

    public static List<TopCareerDto>? Get(int resultId)
    {
        return _store.ContainsKey(resultId)
            ? _store[resultId]
            : null;
    }
}