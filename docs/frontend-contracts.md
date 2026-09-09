# OneFit — Frontend Contracts (Dev A)

> Source of truth for what the frontend can call. Updated as endpoints go live.
> Day-4 contract freeze: no shape changes after Day-4 EOD.
>
> Interactive API reference (dev only): `http://localhost:5066/scalar/` (raw spec: `/openapi/v1.json`).

## How to connect (local)
The API reads `ConnectionStrings:DefaultConnection`. The real Neon password is
**never committed** — supply the full connection string via env var (overrides
`appsettings.json`):

```bash
export ConnectionStrings__DefaultConnection='Host=ep-mute-silence-b25y0d7o-pooler.c-6.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=<password>;SSL Mode=Require;Channel Binding=Require'
dotnet run --project src/OneFit.Api
```
> Key=value form is required — Npgsql rejects URL-form strings.

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

## ✅ LIVE — `GET /products`
Public paginated list for shop/browse UI. All filters in SQL, stable
price+id ordering so pages don't overlap or skip.

```
GET /products?category=shirt&max_price_egp=700&q=linen&in_stock_only=true&sort=price_asc&page=1&page_size=20
```
- `category` optional, lowercase (`shirt|pants|shoes|accessory|...`).
- `max_price_egp` optional, `>= 0`.
- `q` optional, case-insensitive name search.
- `in_stock_only` default `true`.
- `sort` ∈ `price_asc` (default) `| price_desc | newest`.
- `page` >= 1, `page_size` 1–50, default 20.

**Response `200`**
```json
{
  "items": [
    {
      "product_id": "00c306f0-714e-4f84-9e5b-6c29029ce677",
      "brand": "Adidas",
      "name": "John Players Men Navy Blue Shirt",
      "price_egp": 271.12,
      "sizes_in_stock": ["S", "M", "L", "XL"],
      "image_url": "https://..."
    }
  ],
  "page": 1, "page_size": 20, "total": 132
}
```

**Errors**
- `400 { "error": "sort must be price_asc, price_desc or newest" }` (same shape for bad `page`/`page_size`/`max_price_egp`).

## ✅ LIVE — `GET /products/{id}`
Product detail page: full per-size stock, tags, brand.

**Response `200`**
```json
{
  "product_id": "00c306f0-714e-4f84-9e5b-6c29029ce677",
  "brand": "Adidas",
  "brand_id": "24198e14-2198-45de-9e6c-3288781d2f3b",
  "name": "John Players Men Navy Blue Shirt",
  "category": "shirt",
  "price_egp": 271.12,
  "style_tags": ["formal", "summer", "navy blue", "topwear"],
  "sizes": [
    { "size": "M", "stock_qty": 10 },
    { "size": "L", "stock_qty": 0 }
  ],
  "image_url": "https://..."
}
```
- `404 { "error": "product not found" }` on unknown id.

## 🔜 NOT LIVE YET (Dev A, Days 3–4)
| Endpoint | Status |
|---|---|
| `POST /visual-search` | Day-2/3, embedding deferred — contract TBD |
| `POST /wishlist/items`, `DELETE /wishlist/items/{id}`, `GET /wishlist` | Day-3 |
| `GET /alerts`, `POST /alerts/{id}/read` | Day-4 |

Frontend stays on mocks for the above until each row flips to ✅.
