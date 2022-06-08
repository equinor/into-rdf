using Azure.Storage.Blobs.Models;
using Common.TieModels;

namespace Services.TieMessageService;

public interface ITieMessageService
{
    public TieData ParseXmlMessage(List<BlobDownloadResult> blob);
}