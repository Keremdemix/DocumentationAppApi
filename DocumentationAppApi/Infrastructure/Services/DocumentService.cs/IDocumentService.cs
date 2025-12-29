using DocumentationApp.Domain.Entities;
using DocumentationAppApi.API.Models.Requests.Document;
using DocumentationAppApi.Requests.Documents;

namespace DocumentationAppApi.Application.Documents;

public interface IDocumentService
{
    Task<Document> CreateDocumentAsync(CreateDocumentRequest request);
}
