<p align="center">
  <img src="https://img.icons8.com/emoji/64/package-emoji.png" alt="icon" width="60"/>
</p>

<h1 align="center">InventoryManager</h1>

<p align="center">
  Multi-User Collaborative Inventory Management Platform
</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-LIVE--PRODUCTION-00c853?style=for-the-badge&logo=checkmarx&logoColor=white"/>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/PostgreSQL-18-4169E1?style=for-the-badge&logo=postgresql&logoColor=white"/>
  <img src="https://img.shields.io/badge/SignalR-REAL--TIME-FF6F00?style=for-the-badge&logoColor=white"/>
  <img src="https://img.shields.io/badge/LICENSE-MIT-555555?style=for-the-badge"/>
</p>

---

## Live Deployment

The application is live and public. You can create inventories and start managing items immediately after registration.

> **ℹ️ Note**
>
> **Performance Tip:** This project is hosted on a **Render Free Tier**. If the link takes about 30 seconds to load initially, the server is simply "waking up" from its sleep cycle. Once active, the SignalR connection provides sub-millisecond real-time responses. ⚡

**Live Link:** https://inventory-manager-reta.onrender.com

---

## InventoryManager: Multi-User Collaborative Inventory Platform

A full-featured, multi-user inventory management web application built with **ASP.NET Core MVC**, **PostgreSQL**, **SignalR**, and **Bootstrap**. Users can create personal or shared inventories, define custom fields per inventory, manage items collaboratively, discuss in real-time, and control access granularly.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture Overview](#architecture-overview)
- [Domain Models](#domain-models)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Setup](#local-setup)
  - [Database Setup](#database-setup)
  - [Running the App](#running-the-app)
- [Configuration](#configuration)
  - [Environment Variables](#environment-variables)
  - [Social Login (Google & Facebook)](#social-login-google--facebook)
  - [Email (SMTP)](#email-smtp)
- [Authentication & Authorization](#authentication--authorization)
- [Inventory System](#inventory-system)
  - [Custom Fields](#custom-fields)
  - [Custom ID Generation](#custom-id-generation)
  - [Access Control](#access-control)
- [Items](#items)
  - [Likes](#likes)
  - [Tags](#tags)
  - [Image & Document Previews](#image--document-previews)
- [Real-Time Discussion (SignalR)](#real-time-discussion-signalr)
- [Search](#search)
- [Admin Panel](#admin-panel)
- [Themes & Localization](#themes--localization)
- [Auto-Save](#auto-save)
- [Export (CSV & Excel)](#export-csv--excel)
- [Deployment (Docker / Render)](#deployment-docker--render)
- [Known Issues & Troubleshooting](#known-issues--troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## Features

### Core Features
- **Multi-user authentication** — register/login with form-based auth or social login (Google, Facebook)
- **Email confirmation** — optional SMTP-based email verification on registration
- **Inventory management** — create, edit, delete inventories with title, description, category, cover image, and public/private visibility
- **Custom fields** — define an arbitrary number of typed fields per inventory (string, integer, boolean, date, select-list)
- **Field validation tuning** — set min/max length, regex patterns for strings; min/max value ranges for numeric fields
- **Custom ID generation** — define human-readable auto-incrementing IDs per inventory (e.g., `BOOK-001`, `ITEM-042`)
- **Item management** — add, edit, delete items within an inventory using the custom schema defined by the owner
- **Like system** — authenticated users can like/unlike individual items with real-time like counts
- **Tagging** — tag inventories with freeform tags; autocomplete on input
- **Full-text search** — search across all public inventories and items by name, description, or tag
- **Real-time discussion** — per-inventory live chat powered by SignalR with Markdown rendering
- **Access control** — share inventories with specific users (read-only or read-write)
- **Inventory export** — export all items to CSV or Excel (.xlsx)
- **Statistics** — per-inventory statistics tab (item count, like count, top items)
- **Document/image previews** — render image thumbnails and PDF iframes for URL fields
- **Drag-and-drop reordering** — reorder custom fields and custom ID elements
- **Auto-save** — inventory edit page auto-saves every 8 seconds with optimistic locking
- **Markdown rendering** — inventory descriptions and discussion posts support full Markdown

### User Experience
- **Light/Dark theme** — per-user theme preference persisted in the database
- **Multi-language (i18n)** — language preference persisted per user via `RequestCultureMiddleware`
- **Blocked user handling** — admins can block users; blocked users are intercepted by middleware
- **Profile page** — view your own inventories, liked items, and access grants; all tables are sortable and filterable

### Admin Features
- **User management** — view all users, block/unblock, promote/demote admin role
- **Full inventory access** — admins act as owner of every inventory in the system

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| Language | C# 12 |
| ORM | Entity Framework Core 8 (Npgsql provider) |
| Database | PostgreSQL |
| Real-time | SignalR (ASP.NET Core) |
| Authentication | ASP.NET Core Identity + Google OAuth + Facebook OAuth |
| Frontend | Bootstrap 5, Vanilla JS |
| Markdown | Markdig (server-side), Marked.js (client-side SignalR) |
| CSV Export | CsvHelper |
| Excel Export | ClosedXML |
| Containerization | Docker |
| Hosting | Render.com |

---

## Architecture Overview

The project follows a clean **MVC + Service Layer** pattern:

```
Browser
  │
  ▼
Controllers  ──────►  Services (business logic)
  │                       │
  │                       ▼
  │                   ApplicationDbContext (EF Core)
  │                       │
  ▼                       ▼
Views (Razor)         PostgreSQL
  │
  ▼
Hubs (SignalR)
```

**Middleware pipeline order:**

```
UseRequestLocalization
  → UseStaticFiles
  → UseRouting
  → UseAuthentication
  → BlockedUserMiddleware
  → RequestCultureMiddleware
  → UseAuthorization
  → MapControllers / MapHub
```

---

## Domain Models

### ApplicationUser
Extends `IdentityUser` with:
- `IsBlocked` (bool) — blocked users are redirected by middleware
- `Theme` (string) — `"light"` or `"dark"`, resolved by `ThemeHelper`
- `Language` (string) — culture code, e.g. `"en"`, `"de"`, resolved by `LocalizationHelper`

### Inventory
- `Id` (Guid), `Title`, `Description`, `Category`, `ImageUrl`
- `IsPublic` (bool) — public inventories are writable by all authenticated users
- `OwnerId` (FK → ApplicationUser, `DeleteBehavior.Restrict`)
- Navigation: `Fields` (List\<InventoryField\>), `Items`, `Tags` (via InventoryTag), `AccessGrants`

### InventoryField
- `Id` (Guid), `InventoryId` (FK), `Name`, `FieldType` (enum), `Order` (int)
- `FieldType` values: `String`, `Integer`, `Boolean`, `Date`, `SelectList`
- Optional `FieldValidation` child: `MinLength`, `MaxLength`, `RegexPattern`, `MinValue`, `MaxValue`
- Optional `FieldListOption` children (for SelectList type): ordered list of allowed values

### Item
- `Id` (Guid), `InventoryId` (FK), `Name`, `ImageUrl`
- Dynamic field values stored as `string?` properties mapped to `InventoryField` rows
- `[Timestamp] Version` (byte[]) — rowversion for optimistic concurrency
- `CustomId` (string?) — generated by `CustomIdService`
- Navigation: `Likes` (List\<ItemLike\>), `Tags`

### ItemLike
- `Id` (Guid), `ItemId` (FK), `UserId` (FK)
- Unique constraint on (ItemId, UserId)

### InventoryAccess
- `Id` (Guid), `InventoryId` (FK), `UserId` (FK), `CanEdit` (bool)

### DiscussionPost
- `Id` (Guid), `InventoryId` (FK), `UserId` (FK), `Content` (Markdown string), `CreatedAt`

### Tag / InventoryTag
- `Tag`: `Id` (Guid), `Name` (unique)
- `InventoryTag`: junction table (InventoryId, TagId)

### InventorySequence / IdElement
- `InventorySequence`: tracks the current counter value per inventory for custom ID generation
- `IdElement`: defines the parts of the custom ID template (prefix, counter, padding)

---

## Project Structure

```
InventoryManager/
├── Controllers/
│   ├── AccountController.cs       # Register, Login, Logout, Social login, Email confirm
│   ├── AdminController.cs         # User list, Block/Unblock, Role management
│   ├── DiscussionController.cs    # Post message (REST fallback)
│   ├── HomeController.cs          # Home page, top inventories
│   ├── InventoryController.cs     # CRUD, Details, AutoSave, Export CSV/Excel
│   ├── ItemController.cs          # CRUD, ToggleLike, DeleteMultiple
│   ├── ProfileController.cs       # Profile view, SetTheme, SetLanguage
│   └── SearchController.cs        # Full-text search
│
├── Services/
│   ├── AccessService.cs           # CanEditInventory, CanEditItems, admin/public checks
│   ├── CustomIdService.cs         # ID template rendering + sequence increment
│   ├── DiscussionService.cs       # GetPostsAsync, AddPostAsync
│   ├── InventoryService.cs        # GetAll, GetById, Create, Update, Delete
│   ├── ItemService.cs             # CRUD with field validation
│   ├── SearchService.cs           # Full-text search across inventories and items
│   ├── StatisticsService.cs       # Item count, like count, top items
│   ├── TagService.cs              # Tag CRUD, tag cloud, autocomplete
│   └── EmailSender.cs             # IEmailSender implementation (SMTP)
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Inventory.cs
│   ├── InventoryField.cs
│   ├── FieldValidation.cs
│   ├── FieldListOption.cs
│   ├── Item.cs
│   ├── ItemLike.cs
│   ├── InventoryAccess.cs
│   ├── DiscussionPost.cs
│   ├── Tag.cs / InventoryTag.cs
│   ├── InventorySequence.cs
│   ├── IdElement.cs
│   ├── Enums/FieldType.cs
│   ├── DTOs/
│   │   ├── DiscussionPostDto.cs
│   │   ├── IdPreviewDto.cs
│   │   └── TagAutocompleteDto.cs
│   └── ViewModels/
│       ├── InventoryDetailsViewModel.cs
│       ├── InventoryCreateVM.cs / InventoryEditVM.cs
│       ├── ItemCreateViewModel.cs / ItemEditVM.cs
│       ├── ProfileVM.cs
│       ├── AccessManageVM.cs
│       ├── AdminUserVM.cs
│       ├── SearchResultVM.cs
│       └── DiscussionTabViewModel.cs
│
├── Hubs/
│   └── DiscussionHub.cs           # SignalR hub — JoinInventory, SendMessage
│
├── Middleware/
│   ├── BlockedUserMiddleware.cs   # Redirects blocked users to /Account/Blocked
│   └── RequestCultureMiddleware.cs # Sets culture from user.Language preference
│
├── Helpers/
│   ├── IdGeneratorHelper.cs       # Renders custom ID from template + sequence
│   ├── LocalizationHelper.cs      # Maps language code to CultureInfo
│   ├── MarkdownHelper.cs          # Wraps Markdig: ToHtml(string markdown)
│   ├── PermissionHelper.cs        # IsOwner(inventory, userId) with null guard
│   └── ThemeHelper.cs             # Resolve(string theme) → Bootstrap CSS path
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
│       ├── InventoryConfig.cs     # OwnerId: DeleteBehavior.Restrict
│       ├── InventoryFieldConfig.cs
│       ├── ItemConfig.cs
│       ├── ItemLikeConfig.cs
│       ├── InventoryAccessConfig.cs
│       ├── TagConfig.cs
│       ├── IdElementConfig.cs
│       └── InventorySequenceConfig.cs
│
├── Views/
│   ├── Account/                   # Login, Register, RegisterConfirmation, ConfirmEmail
│   ├── Admin/                     # User management table
│   ├── Home/                      # Index (top inventories), Privacy
│   ├── Inventory/
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml            # With auto-save
│   │   ├── Details.cshtml         # Tabbed layout, SignalR scripts
│   │   ├── Index.cshtml
│   │   ├── _ItemsTab.cshtml       # Checkbox multi-select + Delete Selected toolbar
│   │   ├── _FieldsTab.cshtml      # Dynamic field editor with validation tuning
│   │   ├── _DiscussionTab.cshtml  # Markdown-rendered posts, profile links
│   │   ├── _AccessTab.cshtml      # Share inventory with users
│   │   ├── _CustomIdTab.cshtml    # Custom ID template builder
│   │   ├── _SettingsTab.cshtml    # Public/Private toggle, danger zone
│   │   └── _StatisticsTab.cshtml  # Charts and counts
│   ├── Item/                      # Create, Edit, Details, Index
│   ├── Profile/                   # Index (sortable/filterable tables)
│   ├── Search/                    # Results
│   └── Shared/                    # _Layout.cshtml (theme injection), _LoginPartial
│
├── wwwroot/
│   ├── js/
│   │   ├── discussion-signalr.js  # SignalR client, Markdown rendering, profile links
│   │   ├── like.js                # Fetch POST to ToggleLike, update count
│   │   ├── inventory-autosave.js  # Debounced auto-save with optimistic lock version
│   │   ├── field-validation.js    # Client-side field tuning validation
│   │   └── preview.js             # Image lightbox modal for item field previews
│   └── css/
│
├── Migrations/
├── Dockerfile
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Node.js](https://nodejs.org/) (optional, only if you modify frontend assets)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Local Setup

```bash
# 1. Clone the repository
git clone https://github.com/shahedul-islam-joshi/InventoryManager.git
cd InventoryManager

# 2. Restore NuGet packages
dotnet restore

# 3. Set up user secrets for sensitive config
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=inventorymanager;Username=postgres;Password=yourpassword"
```

### Database Setup

```bash
# Apply all migrations (creates tables and seeds the admin user)
dotnet ef database update
```

The seed creates a default admin account:
- **Email:** `admin@admin.com`
- **Password:** `admin123`

> **Important:** Change the admin password immediately after first login in a production environment.

### Running the App

```bash
dotnet run
```

Navigate to `https://localhost:7016` (or the port shown in your terminal).

---

## Configuration

### Environment Variables

All sensitive configuration should be provided via **user secrets** (local) or **environment variables** (production). Never commit secrets to source control.

`appsettings.json` contains safe defaults with empty values for all secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Authentication": {
    "Google": { "ClientId": "", "ClientSecret": "" },
    "Facebook": { "AppId": "", "AppSecret": "" }
  },
  "Email": {
    "Host": "",
    "Port": "587",
    "Username": "",
    "Password": "",
    "From": ""
  }
}
```

All optional integrations (Google, Facebook, SMTP email) are **conditionally registered** — if the values are empty the app starts normally without those features, so you can run the app locally with just a database connection string.

### Social Login (Google & Facebook)

**Google:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a project → APIs & Services → Credentials → Create OAuth 2.0 Client ID
3. Set Authorized redirect URI to: `https://yourdomain.com/signin-google`
4. Copy the Client ID and Client Secret

**Facebook:**
1. Go to [Meta for Developers](https://developers.facebook.com/)
2. Create App → Add Facebook Login product
3. Set Valid OAuth Redirect URI to: `https://yourdomain.com/signin-facebook`
4. Copy App ID and App Secret

**Store credentials:**
```bash
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
dotnet user-secrets set "Authentication:Facebook:AppId" "your-app-id"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "your-app-secret"
```

### Email (SMTP)

Email confirmation is sent on registration when SMTP credentials are configured. If not configured, users can log in without confirming their email.

```bash
dotnet user-secrets set "Email:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:Username" "you@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
dotnet user-secrets set "Email:From" "you@gmail.com"
```

For Gmail, use an [App Password](https://myaccount.google.com/apppasswords) rather than your main password.

---

## Authentication & Authorization

The app uses **ASP.NET Core Identity** for user management with the following roles:

| Role | Permissions |
|---|---|
| `Admin` | Full access to every inventory, user management panel, block/unblock users |
| Authenticated User | Create inventories, access inventories they own or have been granted access to |
| Anonymous | Browse public inventories and items (read-only) |

**Middleware chain for authorization:**
1. `BlockedUserMiddleware` — intercepts all requests from blocked users and redirects to `/Account/Blocked`
2. `RequestCultureMiddleware` — reads `user.Language` from the database and sets `Thread.CurrentCulture`
3. ASP.NET Core `[Authorize]` attributes on controllers and actions

**Access rules for inventory actions:**
- Owner → full access (edit, delete, manage fields, manage access list)
- Admin → acts as owner of every inventory
- Explicit access grant with `CanEdit = true` → can add/edit/delete items
- `IsPublic = true` + authenticated → can add/edit/delete items
- Explicit access grant with `CanEdit = false` → read-only
- Anonymous + public → read-only

---

## Inventory System

### Custom Fields

Each inventory can define an unlimited number of typed fields. The inventory owner configures fields in the **Fields** tab of the inventory detail page.

**Supported field types:**

| Type | Description | Stored As |
|---|---|---|
| `String` | Free-text input | varchar |
| `Integer` | Whole number input | string (parsed on display) |
| `Boolean` | Checkbox | "true"/"false" |
| `Date` | Date picker | ISO 8601 string |
| `SelectList` | Dropdown from predefined options | selected option value |

**Field validation tuning** (optional per field):
- **String fields:** MinLength, MaxLength, RegexPattern
- **Numeric fields:** MinValue, MaxValue
- Validated both server-side (in `ItemService`) and client-side (`field-validation.js`)

**SelectList options:** When a field type is `SelectList`, the owner can define an ordered list of allowed values (e.g., `Desktop`, `Laptop`, `Tablet`). Items then show a `<select>` dropdown restricted to those values.

### Custom ID Generation

Inventory owners can define a **custom ID template** for items. This is configured in the **Custom ID** tab.

Example template: `BOOK-{counter:4}` → generates `BOOK-0001`, `BOOK-0002`, etc.

The `InventorySequence` table tracks the current counter per inventory. The `CustomIdService` increments the sequence and renders the template. The `IdElement` table stores the parts of the template.

### Access Control

Managed in the **Access** tab of the inventory detail page.

- The owner can search for users by email and grant/revoke access
- Access grants are stored in `InventoryAccess` with a `CanEdit` boolean
- The access list is sortable by name and email

---

## Items

Items are the rows of an inventory. Each item has:
- A **Name** (required)
- An optional **Image URL**
- One value per custom field defined by the inventory
- An auto-generated **Custom ID** (if configured)
- A **Like count**
- **Tags** (shared tag pool with autocomplete)

### Likes

The like button is rendered in the item detail view and the items table. Clicking it sends a JSON `POST` to `/Item/ToggleLike` and updates the count without page reload.

Implementation notes:
- `ToggleLike` never loads the `Item` entity (avoids rowversion concurrency conflicts)
- Uses `AnyAsync` to verify item existence, then works directly on `ItemLikes` table
- No `[ValidateAntiForgeryToken]` (incompatible with `[FromBody]` JSON)

### Tags

Tags are shared across the entire application (not per-inventory). When adding a tag to an item or inventory, the input shows autocomplete suggestions from existing tags via `/Tag/Autocomplete?q=...`.

### Image & Document Previews

When an item field value is a URL, the display logic detects the file extension:
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` → renders as `<img>` thumbnail with click-to-enlarge lightbox (Bootstrap modal, no external library)
- `.pdf` → renders as `<iframe>` with fallback link
- Other URLs → plain hyperlink text

---

## Real-Time Discussion (SignalR)

Each inventory has a live discussion thread powered by **ASP.NET Core SignalR**.

**How it works:**
1. When the user opens the Details page, `discussion-signalr.js` connects to `/hubs/discussion`
2. The client calls `JoinInventory(inventoryId)` to subscribe to that inventory's group
3. When a message is submitted, it is sent via `SendMessage(inventoryId, content)`
4. The hub calls `AddPostAsync` on `DiscussionService` to persist the message
5. The hub broadcasts `ReceiveMessage(postDto)` to all clients in the group
6. The client renders the message with **Marked.js** for Markdown and wraps the username in a profile link

**Hub:** `Hubs/DiscussionHub.cs`
**Client:** `wwwroot/js/discussion-signalr.js`
**CDN script (no integrity hash):**
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js"
        crossorigin="anonymous" referrerpolicy="no-referrer"></script>
```

> **Note:** Do not add an `integrity` attribute to this script tag. The SHA-512 hash provided by cdnjs does not match the file served and causes the browser to block the script.

---

## Search

Full-text search is available at `/Search/Results?q=...` and via the navbar search box.

The `SearchService` queries across:
- Inventory titles and descriptions (public inventories only, or those the user has access to)
- Item names
- Tags

Results are grouped by type and rendered in `Views/Search/Results.cshtml`.

---

## Admin Panel

Accessible at `/Admin` — only visible and accessible to users in the `Admin` role.

**Features:**
- View all registered users with registration date, email, role, and blocked status
- Block / Unblock users (blocked users are immediately redirected on next request)
- Promote users to Admin / Demote from Admin role
- Admins see and can edit every inventory in the system as if they were the owner

---

## Themes & Localization

### Themes

Users can switch between **Light** and **Dark** themes from their profile page. The selection is persisted in `ApplicationUser.Theme`.

`ThemeHelper.Resolve(theme)` returns the correct Bootstrap CSS path. `_Layout.cshtml` injects the resolved CSS for the logged-in user on every page.

POST endpoint: `POST /Profile/SetTheme` with body `{ theme: "dark" }`

### Localization

Users can set their preferred language from their profile page. The selection is persisted in `ApplicationUser.Language`.

`RequestCultureMiddleware` reads this value on each request and calls `CultureInfo.CurrentCulture` and `CultureInfo.CurrentUICulture` accordingly. `Program.cs` configures `UseRequestLocalization` with supported cultures.

POST endpoint: `POST /Profile/SetLanguage` with body `{ language: "de" }`

---

## Auto-Save

The inventory **Edit** page auto-saves changes every **8 seconds** after any field modification (debounced).

**Implementation (`wwwroot/js/inventory-autosave.js`):**
- Watches Title, Description, Category, ImageUrl, IsPublic fields for changes
- Debounces — resets the 8-second timer on each keystroke
- POSTs to `/Inventory/AutoSave` with the form data including the current `Version` (rowversion byte array) for optimistic concurrency
- Status indicator shows `Saving...`, `Saved ✓`, or `Conflict — please reload`
- On success, updates the hidden `Version` field with the new version returned from the server
- On 409 Conflict, stops auto-saving and displays the conflict message so the user can manually resolve

---

## Export (CSV & Excel)

From the inventory detail page, users with at least read access can export all items:

- **Export CSV** → `GET /Inventory/ExportCsv/{id}` → downloads `inventory-name.csv`
- **Export Excel** → `GET /Inventory/ExportExcel/{id}` → downloads `inventory-name.xlsx`

Both formats include: Item Name, Custom ID, Tags, all custom field values, Like count, Created date.

Libraries used:
- CSV: [CsvHelper](https://joshclose.github.io/CsvHelper/)
- Excel: [ClosedXML](https://github.com/ClosedXML/ClosedXML)

---

## Deployment (Docker / Render)

### Docker

A `Dockerfile` is included in the repository root.

```bash
# Build the image
docker build -t inventorymanager .

# Run with environment variables
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=db;Database=inventorymanager;Username=postgres;Password=secret" \
  inventorymanager
```

### Render.com

The live demo is hosted on [Render](https://render.com) using:
- **Web Service** running the Docker image
- **PostgreSQL** managed database (Render-hosted)

Environment variables are set in the Render dashboard under the service's Environment tab. Use the same key names as `appsettings.json` with `__` as the hierarchy separator (e.g., `ConnectionStrings__DefaultConnection`).

**First-deploy checklist:**
1. Set `ConnectionStrings__DefaultConnection` to your Render PostgreSQL internal URL
2. Run `dotnet ef database update` against the production database (or set `AutoMigrate = true` in `Program.cs`)
3. Set `Authentication__Google__ClientId` etc. if social login is needed
4. Change the seeded admin password immediately

---

## Known Issues & Troubleshooting

### App crashes on startup: `ArgumentException: The value cannot be an empty string (Parameter 'ClientId')`

**Cause:** Google or Facebook authentication credentials are empty strings in `appsettings.json`.

**Fix:** Social login registration is conditional — it only registers if both ClientId and ClientSecret are non-empty. If you see this error, ensure your `Program.cs` uses the conditional registration pattern:

```csharp
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
    authBuilder.AddGoogle(options => { ... });
```

### SignalR discussion not loading / JavaScript error in console

**Cause:** An `integrity` hash attribute on the SignalR CDN script tag is mismatched.

**Fix:** Remove the `integrity` attribute entirely from the SignalR `<script>` tag in `Details.cshtml`. It should look exactly like:

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js"
        crossorigin="anonymous" referrerpolicy="no-referrer"></script>
```

### Like button throws `DbUpdateConcurrencyException`

**Cause:** Loading the `Item` entity in `ToggleLike` causes EF Core to track its `[Timestamp] Version` rowversion. When only `ItemLikes` rows are modified, EF sees 0 rows affected for `Item` and throws.

**Fix:** Never load the `Item` entity in `ToggleLike`. Use `AnyAsync` to check existence, then only operate on the `ItemLikes` DbSet.

### OwnerId is NULL after admin recreation

**Cause:** `OnDelete(DeleteBehavior.SetNull)` in `InventoryConfig` nullifies `OwnerId` when the owner user is deleted.

**Fix:** Migration `RestrictInventoryOwnerDelete` changes this to `DeleteBehavior.Restrict`. If you have existing NULL values, run:

```sql
UPDATE "Inventories"
SET "OwnerId" = (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'admin@admin.com')
WHERE "OwnerId" IS NULL;
```

### Theme or language not applying

**Cause:** `RequestCultureMiddleware` is not registered in the pipeline, or `UseRequestLocalization` is called without options.

**Fix:** Ensure `Program.cs` calls `app.UseRequestLocalization(locOptions)` with a configured `RequestLocalizationOptions` object, and that `app.UseMiddleware<RequestCultureMiddleware>()` appears after `UseAuthentication` and `BlockedUserMiddleware`.

---

## Contributing

Pull requests are welcome. For major changes please open an issue first to discuss what you would like to change.

**Code style:**
- Follow existing C# naming conventions (PascalCase for public members, camelCase for locals)
- Keep controllers thin — business logic belongs in services
- Add XML doc comments on public service interfaces
- Do not commit secrets or connection strings

**Before submitting a PR:**
```bash
dotnet build
dotnet test   # if tests are added
```

---

## License

This project is licensed under the MIT License. See `LICENSE` for details.