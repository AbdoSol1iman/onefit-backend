# OneFit — Frontend Contracts

Base URL (Azure): `https://onefit-gqcneva3b3bsfnb9.francecentral-01.azurewebsites.net`
Local: `http://localhost:5066` · API browser: `/scalar/`

All JSON is snake_case. `400` = bad input, `401` = login needed, `404` = not found.

## Azure app settings
| Setting | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon string, key=value form |
| `Cors__AllowedOrigins__0` (+ `__1`, …) | Frontend origin(s) — required, else browsers block all calls |
| `ApiDocs__Enabled` | `true` to expose `/scalar/` online |
| `Jwt__SecretKey`, `Stripe__*`, `CloudinarySettings__*` | Secrets — never commit, app settings only |

## Auth (no token needed)

**`POST /api/auth/register`** — `{ "email": "…", "password": "…", "confirm_password": "…", "first_name": "…", "last_name": "…" }` → `200 { "message": "Registration successful…" }` · duplicate/bad input → `400 { "message": "…" }` (never 500).

**`POST /api/auth/register/brand`** — brand signup with documents (multipart form).

**`POST /api/auth/login`** — `{ "email": "…", "password": "…" }` → `200 { "access_token": "…", "refresh_token": "…", "user": {…} }` · wrong password → `401`.

> Send `Authorization: Bearer <access_token>` on everything below marked 🔒.

## `GET /products` ✅
Paged list for shop UI.
```
GET /products?category=shirt&max_price_egp=700&q=linen&in_stock_only=true&sort=price_asc&page=1&page_size=20
```
- `q` = name search. `sort` = `price_asc` (default) `| price_desc | newest`.
- `page >= 1`, `page_size` 1–50 (default 20).
- Response: `{ "items": [{ "product_id": "…", "brand": "…", "name": "…", "category": "shirt", "price_egp": 271.12, "sizes_in_stock": ["S","M"], "image_url": "…" }], "page": 1, "page_size": 20, "total": 132 }`.

## `GET /products/{id}` ✅
Detail page. Adds `brand_id`, `category`, `style_tags`, full per-size stock:
```json
{ "product_id": "…", "brand": "…", "brand_id": "…", "name": "…", "category": "shirt", "price_egp": 271.12, "style_tags": ["formal"], "sizes": [{ "size": "M", "stock_qty": 10 }], "image_url": "…" }
```

## `POST /catalog/query` ✅
Internal slot search (Stylist). `{ "category": "shirt", "max_price_egp": 700, "style_tags": ["linen"], "in_stock_only": true, "limit": 3 }` → `{ "results": […] }` (same item shape as above, `limit` 1–20).

## `GET /api/v1/catalog/query` ✅
Same search as above via query params: `?category=shirt&max_price_egp=700&in_stock_only=true&limit=10`.

## Cart

**`POST /api/v1/cart/items`** — `{ "shopper_id": "…", "product_id": "…", "size": "M", "qty": 1 }` → `200` (increments if already in cart). Bad size/stock → `400`/`404`.

**`DELETE /api/v1/cart/items/{product_id}/{size}?shopper_id=…`** — remove one line item → `200`.

**`GET /api/v1/cart`** 🔒 — the logged-in shopper's cart (shopper taken from token, no params).

## Checkout & orders 🔒

**`POST /api/v1/checkout`** — checks out the token owner's cart, splits one sub-order per brand → `200` order summary.

**`GET /api/v1/orders`** — order history of the token owner.

## Wishlist

**`POST /api/v1/wishlist/items`** — `{ "shopper_id": "…", "product_id": "…", "size": "M" }` → `200 { "item": {…} }`.

**`DELETE /api/v1/wishlist/items/{wishlist_item_id}`** — remove → `200`.

**`GET /api/v1/wishlist?shopper_id=…`** — list a shopper's items → `200`.

## Payments

**`POST /api/v1/payments/webhook`** — Stripe server-to-server callback (raw body + `Stripe-Signature` header). Not called by the frontend.

## Not live yet
| Endpoint | When |
|---|---|
| `POST /visual-search` | Embedding deferred — contract TBD |
| Alerts (`GET /alerts`, `POST /alerts/{id}/read`) | Day-4 |

Stay on mocks for these until they flip to ✅. No shape changes after Day-4 freeze.

## `GET /health`
`{"status":"healthy"}` — ping / Azure probe.
