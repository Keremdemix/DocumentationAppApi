namespace DocumentationApp.Application.Documents.Responses
{
    public class DocumentResponse
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
