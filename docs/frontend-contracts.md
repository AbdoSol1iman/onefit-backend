# OneFit — Frontend Contracts

Base URL (Azure): `https://onefit-gqcneva3b3bsfnb9.francecentral-01.azurewebsites.net`
Local: `http://localhost:5066` · API browser: `/scalar/`

## Azure app settings
| Setting | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon string, key=value form |
| `Cors__AllowedOrigins__0` (+ `__1`, …) | Frontend origin(s) — required, else browsers block all calls |
| `ApiDocs__Enabled` | `true` to expose `/scalar/` online |

## `GET /products` ✅
Paged list for shop UI + stylist top-N (replaces `POST /catalog/query`, deleted).
```
GET /products?category=shirt&max_price_egp=700&q=linen&in_stock_only=true&sort=price_asc&page=1&page_size=20
GET /products?category=shirt&max_price_egp=700&style_tags=linen,casual&style_match=rank&limit=3
```
- `q` = name search. `sort` = `price_asc` (default) `| price_desc | newest`.
- `page >= 1`, `page_size` 1–50 (default 20).
- `style_tags` = comma-separated, `style_match` = `rank` (default, soft-ranked) `| all | any`.
- `limit` 1–20: stylist mode, returns `{ items, page: 1, page_size: limit, total }`, ignores `page/page_size`.
- Response: `{ "items": [ … ], "page": 1, "page_size": 20, "total": 132 }`.
- `400` on bad `sort`/`page`/`page_size`/`style_match`/`limit`.

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
