namespace DocumentationAppApi.Responses.Applications;

public class ApplicationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}
