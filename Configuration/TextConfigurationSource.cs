using Microsoft.Extensions.Configuration;

namespace CampusHub.ConfigCenter.Configuration;

public class TextConfigurationSource : IConfigurationSource
{
    private readonly string _filePath;

    public TextConfigurationSource(string filePath)
    {
        _filePath = filePath;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new TextConfigurationProvider(_filePath);
    }
}