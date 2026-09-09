# OneFit — Frontend Contracts (Dev A)

> Source of truth for what the frontend can call. Updated as endpoints go live.
> Day-4 contract freeze: no shape changes after Day-4 EOD.

## How to connect (local)
The API reads `ConnectionStrings:DefaultConnection`. The real Neon password is
**never committed** — supply the full connection string via env var (overrides
`appsettings.json`):

```bash
export ConnectionStrings__DefaultConnection="postgresql://neondb_owner:<password>@ep-mute-silence-b25y0d7o-pooler.c-6.eu-central-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require"
dotnet run --project src/OneFit.Api
```

## ✅ LIVE (Day-1) — `POST /catalog/query`
Internal catalog query (Dev B Stylist calls this per slot). Frontend may also
call it for direct search/filter UI.

**Request**
```json
{
  "category": "shirt",
  "max_price_egp": 700,
  "style_tags": ["linen", "casual"],
  "in_stock_only": true,
  "limit": 3
}
```
- `category` optional, lowercase (`shirt|pants|shoes|accessory|...`).
- `max_price_egp` optional, `>= 0`.
- `style_tags` optional, soft-ranked (best tag overlap first), not hard-filtered.
- `limit` 1–20, default 3.

**Response `200`**
```json
{
  "results": [
    {
      "product_id": "00c306f0-714e-4f84-9e5b-6c29029ce677",
      "brand": "Adidas",
      "name": "John Players Men Navy Blue Shirt",
      "price_egp": 271.12,
      "sizes_in_stock": ["S", "M", "L", "XL"],
      "image_url": "https://..."
    }
  ]
}
```
Empty `results: []` means no match — do **not** fabricate items (NFR-08).

**Errors**
- `400 { "error": "limit must be 1..20" }` on bad limit.
- `500` on DB failure (logged server-side per NFR-10).

## 🔜 NOT LIVE YET (Dev A, Days 3–4)
| Endpoint | Status |
|---|---|
| `POST /visual-search` | Day-2/3, embedding deferred — contract TBD |
| `POST /wishlist/items`, `DELETE /wishlist/items/{id}`, `GET /wishlist` | Day-3 |
| `GET /alerts`, `POST /alerts/{id}/read` | Day-4 |

Frontend stays on mocks for the above until each row flips to ✅.
