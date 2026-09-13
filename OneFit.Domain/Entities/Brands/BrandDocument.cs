using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Domain.Entities.Brands
{
    public class BrandDocument
    {
        public Guid DocumentId { get; set; }

        public string BrandId { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string StorageKey { get; set; } = null!;

        public DateTime UploadedAt { get; set; }

        public virtual Brand Brand { get; set; } = null!;
    }
}
