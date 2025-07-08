namespace PocztexApi.Shared.Seeding;

public static class SeederHelper
{
    public static UniqueId GetUniqueId(string id)
    {
        if (id.ToLower() == "{new}")
            return UniqueId.CreateNew();

        return UniqueId.Parse(id);
    }
}