# Store Module

[![CI status](https://github.com/VirtoCommerce/vc-module-store/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-store/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store)

## Overview

The Store module provides multi-store management capabilities for the Virto Commerce platform. It allows creating and configuring multiple storefronts, each with individual settings for languages, currencies, fulfillment centers, SEO, tax calculation, authentication schemes, and email notifications. The module supports dynamic properties, scoped security permissions, export/import operations, and integrates with the platform's notification, SEO, and currency subsystems.

## Key Features

* **Multi-store management** — Create, update, delete, and search stores with configurable properties including name, URL, catalog, time zone, country, and region.
* **Per-store asset public URL** — Serve each store's asset (image) URLs from its own domain (e.g. a store-specific CDN) instead of the shared platform/CDN domain via the `Store.AssetPublicUrl` property and the `IStoreAssetPublicUrlResolver` service.
* **Per-store currency and language support** — Configure multiple currencies and languages per store with a designated default for each.
* **Fulfillment center assignment** — Assign primary and alternate fulfillment centers for orders and returns per store.
* **Store authentication schemes** — Manage per-store authentication providers (password login, external SSO) with ordering and activation controls.
* **SEO support** — Store-level SEO info management with configurable SEO link types (None, Short, Collapsed, Long) and best-match SEO resolution.
* **Email verification** — Configurable email verification flow that sends confirmation emails to store users via the Notifications module.
* **Dynamic email notifications** — Send dynamic notifications with custom fields to store or administrator email addresses.
* **Trusted groups** — Define groups of stores that share user logins across storefronts.
* **Login on behalf** — Check whether a contact has permission to log in on behalf of another user within a store context.
* **Public store settings** — Expose publicly accessible (anonymous) store settings grouped by module.
* **Scoped security** — Restrict permissions (read, update, delete) to specific stores via `StoreSelectedScope`.
* **Export/Import** — Full store data export and import with batched JSON serialization.
* **Change logging** — Automatic change log entries for store modifications via background jobs.
* **Store setup for AI agents** — With the MCP module installed, an AI assistant sets up, checks and opens stores through tools, every change after approval; other modules plug their part of a store's setup into a contract in Store.Core.

## Configuration

### Application Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Stores.States` | ShortText (dictionary) | `Open` | Store state. Allowed values: `Open`, `Closed`, `RestrictedAccess`. Hidden setting. |
| `Stores.TaxCalculationEnabled` | Boolean | `true` | Enables tax calculation for the store. |
| `Stores.AllowAnonymousUsers` | Boolean | `true` | Allows anonymous (unauthenticated) users to access the store. |
| `Stores.IsSpa` | Boolean | `false` | Indicates whether the storefront is a Single Page Application. |
| `Stores.EmailVerificationEnabled` | Boolean | `false` | Enables sending email verification notifications to store users. |
| `Stores.EmailVerificationRequired` | Boolean | `false` | Requires email verification before users can access the store. |
| `Stores.EnablePriceRoundingForTotalsCalculation` | Boolean | `true` | Enables price rounding when calculating order totals. |
| `Stores.SeoLinksType` | ShortText | `Collapsed` | SEO URL format type. Allowed values: `None`, `Short`, `Collapsed`, `Long`. |

### Store Asset Public URL

Each store can define its own base URL for public asset (image) links via the **Store asset URL** field in the store details blade (the `Store.AssetPublicUrl` property, also exposed through the Experience API `store` query). When set, the `IStoreAssetPublicUrlResolver` service combines relative asset paths with this base URL and rebases absolute platform asset URLs onto it, preserving the path, query string (e.g. SAS tokens), and fragment. When the field is empty, asset URLs are returned unchanged (global CDN / platform default). Serving the assets from the custom domain (DNS/CDN routing) is handled at the infrastructure level.

The rewrite of absolute URLs can be restricted to specific source hosts with the `VirtoCommerce:StoreAssets` configuration section in `appsettings.json`:

```json
{
  "VirtoCommerce": {
    "StoreAssets": {
      "KnownAssetHosts": [
        "cdn.example.com",
        "https://mycompany.blob.core.windows.net"
      ]
    }
  }
}
```

| Key | Default | Description |
|-----|---------|-------------|
| `VirtoCommerce:StoreAssets:KnownAssetHosts` | `[]` | Hosts (bare domains or full URLs) of platform/CDN asset URLs that are allowed to be replaced with the store's asset public URL. When empty, any absolute asset URL is rebased. When set, only URLs whose host is listed (compared case-insensitively) are rebased; URLs on other hosts are treated as external and returned as-is. |

### Permissions

| Permission | Description |
|------------|-------------|
| `store:access` | Access store module functionality. |
| `store:create` | Create new stores. |
| `store:read` | Read store data. Supports `StoreSelectedScope` to restrict to specific stores. |
| `store:update` | Update existing stores. Supports `StoreSelectedScope` to restrict to specific stores. |
| `store:delete` | Delete stores. Supports `StoreSelectedScope` to restrict to specific stores. |

## Store setup with an AI agent (MCP)

With the [MCP module](https://github.com/VirtoCommerce/vc-module-mcp) installed (an optional dependency), an AI assistant connected to the platform's `/admin/mcp` endpoint sets up, checks and opens stores. A person sees only the stores their permissions allow, including permissions limited to stores with `StoreSelectedScope`. Secrets, such as payment keys, are never listed: a person enters them in the back office.

| Tool | What it does |
|------|--------------|
| `get_store_overview` | Start here: the stores the user manages, how many are open, closed and restricted, and what each one is missing. |
| `list_stores` | The stores the user manages, with their state, languages, currencies, catalog and storefront URL. |
| `get_store` | One store's whole setup: its basics, the part of every installed module and a readiness summary. |
| `get_store_settings` | The store-level settings of every installed module, with their meaning, default and value. |
| `compare_store_settings` | The settings that differ between two stores, or between a store and the defaults. |
| `get_store_readiness_report` | What a store is missing before it goes live: why each item matters, how to fix it and whether it blocks opening the store. |
| `get_store_setup_options` | What a store's setup consists of on this installation, and what can be chosen. |
| `create_configured_store` | Creates a store with its whole setup in one call. The store is created closed. |
| `apply_store_setup` | Changes one part or several of a store's setup in one call. |
| `open_store` | Opens a store; refused while something blocks it from going live. |
| `close_store` | Closes a store. |

Every action waits in Agent approvals until a person with `mcp:approve` approves it. A dry run returns the same preview and changes nothing. A change that spans several modules is not a transaction: when a part fails, the parts before it stay applied, the answer names them, and the same call repeated finishes the setup.

### The role

A store administrator who works through an AI agent needs these permissions (Security > Roles):

```json
["store:access", "store:read", "store:create", "store:update", "shipping:read", "shipping:update", "tax:read", "tax:update", "platform:setting:read", "mcp:audit:read"]
```

`store:read`, `store:create` and `store:update` open the tools above; the `shipping` and `tax` ones open those parts of a store's setup; `platform:setting:read` opens the MCP module's `search_settings` and `explain_setting`, which explain the settings of the whole platform; `store:access` and `mcp:audit:read` open the **Stores** screen the answers link to and the **Agent audit log**.

Limit `store:read` and `store:update` to some stores to let the agent work with those stores only. The approver needs `mcp:approve` and the same permissions, because an approved change runs as the approver.

### An example routine: launching a new store

The module ships no routines: each person keeps their own in their assistant, for example as a skill. One to copy and adapt:

```text
Launch a new store with the Virto Commerce store admin tools.
1. Read get_store_setup_options. Ask me for the store's id and name, its languages and currencies, and the payment
   methods, shipping methods and tax provider it uses. Offer only what the options list.
2. Call create_configured_store with dryRun. Show me the changes and what the store would still miss. If it would
   not be ready, ask whether to create it anyway and finish it in the back office, or to complete the setup first.
3. When I confirm, call it again without dryRun. It waits in Agent approvals: tell me so.
4. After the approval, read get_store_readiness_report. Fix what the tools can fix with apply_store_setup, again
   with a dry run first. For the rest, give me the back-office links from the report.
5. When nothing blocks going live, call open_store with dryRun, show me the result, and submit it when I confirm.
Never ask me for a secret, such as a payment key: I enter it in the back office.
```

### A module's part of a store's setup

A store's setup is made of parts. Store owns two: `basics` (its own fields, including the optional links to a catalog and fulfillment centers) and `settings` (the store-level settings of every module). Another module adds its part in its `Initialize`, and Store never references it:

```csharp
serviceCollection.AddStoreSetupPart<PaymentStoreSetupPart>();
```

A part implements `IStoreSetupPart` and the duties it covers, any of them:

| Interface | Duty |
|-----------|------|
| `IStoreSetupChoices` | What can be chosen on the installation, such as method codes. A choice names the field it fills with its section, such as `basics.catalog_id`. |
| `IStoreSetupState` | The store's current state for the part, in the shape of its section, without secrets. |
| `IStoreSetupCheck` | What the store is missing: critical findings block going live, then warnings and notes. |
| `IStoreSetupSection` | Applies the part's section of a setup: plans it without side effects, then applies it so that a repeat changes nothing. |

The shapes a part exchanges come from `VirtoCommerce.MCP.Core`, which Store.Core references: `McpFinding`, `McpSetupChoice`, `McpSetupSetting`, `McpSetupPartPlan` and the others ([Setups that modules plug into](https://github.com/VirtoCommerce/vc-module-mcp/blob/dev/docs/toolset-helpers.md#setups-that-modules-plug-into)). A part's module does not need the MCP module installed: Store ships `VirtoCommerce.MCP.Core`.

A part names the existing permissions that guard it, and the permission scope that limits them to stores (`McpSetupPermissions`; `StoreSetupPermissions.ForStore()` gives Store's own); the tools check them for every part. Secrets follow the MCP module's rule (`McpSecretSettings`): a setting typed `SecureString`, or with the word Key, Secret, Password, Token or Credential(s) in its name, is a secret. A module adds its other secrets with `serviceCollection.AddMcpSecretSettings("MyModule.SigningCode")`.

A part whose objects have settings, such as payment methods, follows Store's rules through `StoreSetupSettingsExtensions`: `DescribeAll` for its choices and `GetValues` for its state, both without secrets, and `PlanSettings` for a settings map in its section. `PlanSettings` refuses an unknown name, a secret and a wrong value, returns a setting to its default for a null, and changes nothing for the value already in effect. `StoreSetupFindings.ForStore` starts a finding about a store, with its back-office screen.

## Architecture

### Key Flow

1. An API request arrives at `StoreModuleController` or `StoreAuthenticationSchemeController`.
2. The `StoreAuthorizationHandler` evaluates the user's permissions, including any `StoreSelectedScope` restrictions.
3. The controller delegates to `StoreService` (CRUD) or `StoreSearchService` (search), which validate input via `StoreValidator`.
4. `StoreService` uses `StoreRepository` to load/persist `StoreEntity` objects through `StoreDbContext`.
5. The appropriate database provider (SqlServer, MySql, or PostgreSql) handles migrations and EF Core configuration.
6. After save, `StoreService` deep-saves store settings via `ISettingsManager` and publishes a `StoreChangedEvent`.
7. `LogChangesChangedEventHandler` enqueues a background job (`LogEntityChangesJobHandler`) to record changes in the platform change log.
8. `SendStoreUserVerificationEmailHandler` listens for `UserVerificationEmailEvent` and sends email verification via `StoreNotificationSender` when the user belongs to a store.
9. Store and SEO data are cached via `StoreCacheRegion` and `StoreSeoInfoCacheRegion`, invalidated on changes.

## Components

### Projects

| Project | Layer | Purpose |
|---------|-------|---------|
| VirtoCommerce.StoreModule.Core | Core | Domain models, service interfaces, events, settings, permissions, and notifications. |
| VirtoCommerce.StoreModule.Data | Data | Service implementations, repository, DbContext, caching, validation, event handlers, and export/import. |
| VirtoCommerce.StoreModule.Data.SqlServer | DB Provider | EF Core migrations and configuration for SQL Server. |
| VirtoCommerce.StoreModule.Data.MySql | DB Provider | EF Core migrations and configuration for MySQL. |
| VirtoCommerce.StoreModule.Data.PostgreSql | DB Provider | EF Core migrations and configuration for PostgreSQL. |
| VirtoCommerce.StoreModule.Web | Web | REST API controllers, authorization handler, module initialization, and export/import orchestration. |
| VirtoCommerce.StoreModule.MCP | MCP | The store admin toolset for AI agents, served by the MCP module (an optional dependency) in its admin scope. |
| VirtoCommerce.StoreModule.Tests | Tests | Unit tests for the module. |

### Key Services

| Service | Interface | Responsibility |
|---------|-----------|----------------|
| `StoreService` | `IStoreService` | CRUD operations for stores, including settings deep-load/save, validation, deletion with change log, and resolving user-allowed store IDs. |
| `StoreSearchService` | `IStoreSearchService` | Search stores by keyword, object IDs, store states, fulfillment center IDs, and domain. |
| `StoreCurrencyResolver` | `IStoreCurrencyResolver` | Resolves a specific currency or all currencies for a store with culture-aware formatting and caching. |
| `StoreAssetPublicUrlResolver` | `IStoreAssetPublicUrlResolver` | Resolves store-aware public asset (image) URLs: combines relative paths with, or rebases absolute URLs onto, the store's `AssetPublicUrl`, optionally restricted by `VirtoCommerce:StoreAssets:KnownAssetHosts`. |
| `StoreNotificationSender` | `IStoreNotificationSender` | Generates email verification links and sends confirmation email notifications for store users. |
| `StoreSeoResolver` | `ISeoResolver` | Resolves SEO information by slug/permalink for stores with caching. |
| `StoreSeoBySlugResolver` | `ISeoBySlugResolver` | Legacy (obsolete) SEO resolution by slug for backward compatibility. |
| `PublicStoreSettings` | `IPublicStoreSettings` | Retrieves publicly accessible store settings grouped by module. |
| `StoreAuthenticationService` | `IStoreAuthenticationService` | Manages per-store authentication schemes, merging stored schemes with globally registered providers. |
| `StoreAuthenticationSchemeService` | `IStoreAuthenticationSchemeService` | CRUD operations for store authentication scheme entities. |
| `StoreAuthenticationSchemeSearchService` | `IStoreAuthenticationSchemeSearchService` | Search authentication schemes filtered by store ID. |
| `StoreSetupService` | `IStoreSetupService` | A store's setup across the parts that modules register: lists the parts and works out what a store is missing. |
| `StoreSetupSettings` | `IStoreSetupSettings` | Describes settings for a setup (titles, defaults, values), tells secrets apart and converts values sent by a request. |

### REST API

Base route: `api/stores`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `api/stores/search` | Search stores by criteria (keyword, states, fulfillment centers, domain). |
| GET | `api/stores` | Get all stores. **Obsolete** — use `POST api/stores/search` instead. |
| GET | `api/stores/{id}` | Get a store by its ID with optional response group. |
| GET | `api/stores/outer/{outerId}` | Get a store by its outer (integration) ID. |
| POST | `api/stores` | Create a new store. Requires `store:create` permission. |
| PUT | `api/stores` | Update an existing store. |
| PATCH | `api/stores/{id}` | Partial update a store using JSON Patch. |
| DELETE | `api/stores?ids=` | Delete stores by IDs. |
| POST | `api/stores/send/dynamicnotification` | Send a dynamic email notification to a store's email address. |
| GET | `api/stores/{storeId}/accounts/{id}/loginonbehalf` | Check if a contact has login-on-behalf permission for a store. |
| GET | `api/stores/allowed/{userId}` | Get the list of stores a user is allowed to sign in to. |
| GET | `api/stores/{id}/public-settings` | Get public store settings (anonymous access). |

Base route: `api/store-authentication-schemes/{storeId}`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `api/store-authentication-schemes/{storeId}` | Get authentication schemes for a store. Requires `store:read`. |
| PUT | `api/store-authentication-schemes/{storeId}` | Update authentication schemes for a store. Requires `store:update`. |

## Documentation

* [Store module user documentation](https://docs.virtocommerce.org/platform/user-guide/store/overview/)
* [GraphQL API documentation](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Store/overview/)
* [REST API reference](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.Store)
* [GitHub repository](https://github.com/VirtoCommerce/vc-module-store)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-store/releases/latest)

## License

Copyright (c) Virto Solutions LTD. All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at:

https://virtocommerce.com/open-source-license

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
