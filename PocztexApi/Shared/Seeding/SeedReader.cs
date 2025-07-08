using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PocztexApi.Shared.Seeding;

public class SeedReader
{
    List<SeedDataDto>? _data = null;

    public async Task<TDataModel[]> ReadSeedFromArray<TDataModel>(string schema)
    {
        if (_data is null)
            await LoadSeedData();

        return [.. GetSeedDataBySchema<TDataModel[]>(schema).SelectMany(x => x)];
    }

    public async Task<TDataModel[]> ReadSeedFromObject<TDataModel>(string schema)
    {
        if (_data is null)
            await LoadSeedData();

        return [.. GetSeedDataBySchema<TDataModel>(schema)];
    }

    async Task LoadSeedData()
    {
        _data = [];

        // Load from external files
        {
            var directory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "seeders"));

            if (directory.Exists)
            {
                foreach (var file in directory.GetFiles("*.seed.json", SearchOption.AllDirectories))
                {
                    using var fileStream = new StreamReader(file.OpenRead());

                    AddData(await fileStream.ReadToEndAsync());
                }
            }
        }

        void AddData(string json)
        {
            var data = JsonConvert.DeserializeObject<SeedDataDto>(json);

            if (data is not null)
                _data!.Add(data);
        }
    }

    IEnumerable<TDataModel> GetSeedDataBySchema<TDataModel>(string schema)
    {
        if (_data is not null)
        {
            foreach (var data in _data.Where(d => string.Compare(d.Schema, schema, ignoreCase: true) == 0))
            {
                if (data.Data is not null)
                    yield return data.Data.ToObject<TDataModel>()!;
            }
        }
    }

    record SeedDataDto(string Schema, JToken? Data);
}