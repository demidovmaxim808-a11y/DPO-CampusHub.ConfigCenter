namespace CampusHub.ConfigCenter.Models;

public class PortalOptions
{
    public string Title { get; set; } = "";
    public string Semester { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public AdminOptions Admin { get; set; } = new();
    public List<string> Modules { get; set; } = new();
}

public class AdminOptions
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}