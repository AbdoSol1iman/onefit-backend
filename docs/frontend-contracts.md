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
Paged list for shop UI + stylist top-N (replaces `POST /catalog/query`, deleted).
```
GET /products?category=shirt&max_price_egp=700&q=linen&in_stock_only=true&sort=price_asc&page=1&page_size=20
GET /products?category=shirt&max_price_egp=700&style_tags=linen,casual&style_match=rank&limit=3
```
- `q` = name search. `sort` = `price_asc` (default) `| price_desc | newest`.
- `page >= 1`, `page_size` 1–50 (default 20).
- `style_tags` = comma-separated, `style_match` = `rank` (default, soft-ranked) `| all | any`.
- `limit` 1–20: stylist mode, returns `{ items, page: 1, page_size: limit, total }`, ignores `page/page_size`.
- Response: `{ "items": [{ "product_id": "…", "brand": "…", "name": "…", "category": "shirt", "price_egp": 271.12, "sizes_in_stock": ["S","M"], "image_url": "…" }], "page": 1, "page_size": 20, "total": 132 }`.
- `400` on bad `sort`/`page`/`page_size`/`style_match`/`limit`.

## `GET /products/{id}` ✅
Detail page. Adds `brand_id`, `category`, `style_tags`, full per-size stock:
```json
{ "product_id": "…", "brand": "…", "brand_id": "…", "name": "…", "category": "shirt", "price_egp": 271.12, "style_tags": ["formal"], "sizes": [{ "size": "M", "stock_qty": 10 }], "image_url": "…" }
```

## `POST /stylist/message` ✅
Stylist chat: `{ "shopper_id": "s1", "message": "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه" }` → `{ "status": "ready|need_budget|off_topic|llm_fallback|stylist_busy", "reply": "…", "intent": { "occasion": "wedding", "setting": "beach", "style": "casual", "budget_egp": 2500 }, "outfits": […], "plan": { "item_slots": […] } | null }`.
- Missing budget → `need_budget` + follow-up question, no catalog call. `skip` skips budget.
- Off-topic → `off_topic` redirect, no catalog call.
- LLM invalid twice → `llm_fallback`; Gemini quota hit → `stylist_busy`.
- Uses `GET /products` semantics internally (`limit: 3`, style-ranked).

## Catalog query endpoints — removed
`POST /catalog/query` and `GET /api/v1/catalog/query` are deleted. Use `GET /products` with `style_tags` + `limit` instead (same replacement as above).

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
