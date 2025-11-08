Playlist Platform — Final Project (.NET 8, Postgres, Redis, Docker)



A two-service platform for managing playlists, songs, and artists.

Supports JWT authentication, voting/likes, popularity sorting, Redis caching, pagination, RFC7807 problem details, and convenient Swagger docs.



&nbsp;Services

1\) Playlist.Api — core domain



Artists / Songs / Playlists



Add/remove songs, insert at position, reorder, bulk-add



Votes/likes for songs (+1 / -1), top by popularity



Pagination and search



Redis cache for “hot” songs and paged selections



JWT authentication and roles (Admin, User)



Swagger



2\) UserService — user playlists (private/public)



Registration / Login



Create and manage user playlists



Add songs by ID (via Playlist.Api HTTP gateway)



View public playlists of other users



Swagger



Both services apply EF Core migrations on startup.



&nbsp;	Key Features



&nbsp;Artists / Songs / Playlists CRUD (some endpoints are Admin-only)



&nbsp;Playlists: append songs or insert by position, reorder, bulk-add



&nbsp;Likes/votes: global rating and per-playlist rating



&nbsp;Redis cache: song details (hot), genre pages, top lists



&nbsp;JWT: login/registration, protected endpoints, roles



&nbsp;Swagger on both services



&nbsp;Docker Compose: Postgres + Redis + both services



&nbsp;Tech Stack



.NET 8 (Web API), C#



Entity Framework Core + migrations



PostgreSQL — primary database



Redis — cache



AutoMapper, JWT (Microsoft.IdentityModel.Tokens)



Swagger / OpenAPI



Docker, docker compose

