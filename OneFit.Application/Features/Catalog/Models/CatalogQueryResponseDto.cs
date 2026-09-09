using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OneFit.Application.Features.Catalog.Models
{
    public record CatalogQueryResponseDto(
    IReadOnlyList<CatalogProductDto> Results,
    bool Relaxed = false);
}
