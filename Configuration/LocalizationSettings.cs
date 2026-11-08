namespace MyFirstApi.Configuration;
public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "en";
    public string[] SupportedCultures { get; set; } = Array.Empty<string>();
}