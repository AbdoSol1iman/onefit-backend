using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Npgsql.BackendMessages;
using OneFit.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace OneFit.Infrastructure.FileStorage
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileStorageService(
            IOptions<CloudinarySettings> settings)
        {
            var account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(
                    fileName,
                    fileStream),

                PublicId =
                    $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",

                Type = "authenticated"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception(
                    $"File upload failed: {result.Error.Message}");
            }

            return result.PublicId;
        }

        public Task<string> GetUrlAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            var url = _cloudinary.Api.Url
                .ResourceType("raw")
                .Type("authenticated")
                .Secure(true)
                .BuildUrl(storageKey);

            url = url.Replace("http://", "https://");

            return Task.FromResult(url);
        }

        public async Task<Stream> DownloadAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            var url = _cloudinary.Api.Url
                .ResourceType("raw")
                .Type("authenticated")
                .Secure(true)
                .Signed(true)
                .BuildUrl(storageKey);

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"File download failed. Status code: {response.StatusCode}");
            }

            var memoryStream = new MemoryStream();

            await response.Content.CopyToAsync(
                memoryStream,
                cancellationToken);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
