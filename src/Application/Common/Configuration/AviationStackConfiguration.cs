namespace Application.Common.Configuration;

public class AviationStackConfiguration
{
    public const string SectionName = "AviationStack";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.aviationstack.com/v1";
}
