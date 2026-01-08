namespace DocumentationAppApi.API.Models.Requests.Document
{
    public class CreateDocumentRequest
    {
        public int ApplicationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int CreatedBy{ get; set; }
        public string Format { get; set; }
    }

}
