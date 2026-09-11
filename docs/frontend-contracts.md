# OneFit — Frontend Contracts

Base URL (Azure): `https://onefit-gqcneva3b3bsfnb9.francecentral-01.azurewebsites.net`
Local: `http://localhost:5066` · API browser: `/scalar/`

## Azure app settings
| Setting | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon string, key=value form |
| `Cors__AllowedOrigins__0` (+ `__1`, …) | Frontend origin(s) — required, else browsers block all calls |
| `ApiDocs__Enabled` | `true` to expose `/scalar/` online |

## `POST /catalog/query` ✅
Internal slot search (Stylist). Frontend can reuse it for filtered search.
```json
// request
{ "category": "shirt", "max_price_egp": 700, "style_tags": ["linen"], "in_stock_only": true, "limit": 3 }
// response: { "results": [{ "product_id": "…", "brand": "Adidas", "name": "…", "price_egp": 271.12, "sizes_in_stock": ["S","M"], "image_url": "…" }] }
```
- `category` lowercase, `max_price_egp >= 0`, `style_tags` soft-ranked, `limit` 1–20 (default 3).
- Empty `results: []` = no match. `400` on bad `limit`.

## `GET /products` ✅
Paged list for shop UI.
```
GET /products?category=shirt&max_price_egp=700&q=linen&in_stock_only=true&sort=price_asc&page=1&page_size=20
```
- `q` = name search. `sort` = `price_asc` (default) `| price_desc | newest`.
- `page >= 1`, `page_size` 1–50 (default 20).
- Response: `{ "items": [ …same shape as above… ], "page": 1, "page_size": 20, "total": 132 }`.
- `400` on bad `sort`/`page`/`page_size`.

## `GET /products/{id}` ✅
Detail page. Adds `brand_id`, `category`, `style_tags`, and full per-size stock:
```json
{ "product_id": "…", "brand": "Adidas", "brand_id": "…", "name": "…", "category": "shirt", "price_egp": 271.12, "style_tags": ["formal"], "sizes": [{ "size": "M", "stock_qty": 10 }], "image_url": "…" }
```
- `404 { "error": "product not found" }`.

## Not live yet
| Endpoint | When |
|---|---|
| `POST /visual-search` | Day-2/3 |
| Wishlist (`POST/DELETE /wishlist/items`, `GET /wishlist`) | Day-3 |
| Alerts (`GET /alerts`, `POST /alerts/{id}/read`) | Day-4 |

Stay on mocks for these until they flip to ✅. No shape changes after Day-4 freeze.

## `GET /health`
`{"status":"healthy"}` — ping / Azure probe.
