using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default);

        Task<string> GetUrlAsync(
            string storageKey,
            CancellationToken cancellationToken = default);

        Task<Stream> DownloadAsync(
            string storageKey,
            CancellationToken cancellationToken = default);
    }
}
