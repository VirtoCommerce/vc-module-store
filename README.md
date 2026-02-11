# Store Module

[![CI status](https://github.com/VirtoCommerce/vc-module-store/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-store/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-store&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-store)

## Overview

The Store module provides multi-store management capabilities for the Virto Commerce platform. It allows creating and configuring multiple storefronts, each with individual settings for languages, currencies, fulfillment centers, SEO, tax calculation, authentication schemes, and email notifications. The module supports dynamic properties, scoped security permissions, export/import operations, and integrates with the platform's notification, SEO, and currency subsystems.

## Key Features

* **Multi-store management** — Create, update, delete, and search stores with configurable properties including name, URL, catalog, time zone, country, and region.
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

### Permissions

| Permission | Description |
|------------|-------------|
| `store:access` | Access store module functionality. |
| `store:create` | Create new stores. |
| `store:read` | Read store data. Supports `StoreSelectedScope` to restrict to specific stores. |
| `store:update` | Update existing stores. Supports `StoreSelectedScope` to restrict to specific stores. |
| `store:delete` | Delete stores. Supports `StoreSelectedScope` to restrict to specific stores. |

## Architecture

### Key Flow

1. An API request arrives at `StoreModuleController` or `StoreAuthenticationSchemeController`.
2. The `StoreAuthorizationHandler` evaluates the user's permissions, including any `StoreSelectedScope` restrictions.
3. The controller delegates to `StoreService` (CRUD) or `StoreSearchService` (search), which validate input via `StoreValidator`.
4. `StoreService` uses `StoreRepository` to load/persist `StoreEntity` objects through `StoreDbContext`.
5. The appropriate database provider (SqlServer, MySql, or PostgreSql) handles migrations and EF Core configuration.
6. After save, `StoreService` deep-saves store settings via `ISettingsManager` and publishes a `StoreChangedEvent`.
7. `LogChangesChangedEventHandler` enqueues a Hangfire background job to record changes in the platform change log.
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
| VirtoCommerce.StoreModule.Tests | Tests | Unit tests for the module. |

### Key Services

| Service | Interface | Responsibility |
|---------|-----------|----------------|
| `StoreService` | `IStoreService` | CRUD operations for stores, including settings deep-load/save, validation, deletion with change log, and resolving user-allowed store IDs. |
| `StoreSearchService` | `IStoreSearchService` | Search stores by keyword, object IDs, store states, fulfillment center IDs, and domain. |
| `StoreCurrencyResolver` | `IStoreCurrencyResolver` | Resolves a specific currency or all currencies for a store with culture-aware formatting and caching. |
| `StoreNotificationSender` | `IStoreNotificationSender` | Generates email verification links and sends confirmation email notifications for store users. |
| `StoreSeoResolver` | `ISeoResolver` | Resolves SEO information by slug/permalink for stores with caching. |
| `StoreSeoBySlugResolver` | `ISeoBySlugResolver` | Legacy (obsolete) SEO resolution by slug for backward compatibility. |
| `PublicStoreSettings` | `IPublicStoreSettings` | Retrieves publicly accessible store settings grouped by module. |
| `StoreAuthenticationService` | `IStoreAuthenticationService` | Manages per-store authentication schemes, merging stored schemes with globally registered providers. |
| `StoreAuthenticationSchemeService` | `IStoreAuthenticationSchemeService` | CRUD operations for store authentication scheme entities. |
| `StoreAuthenticationSchemeSearchService` | `IStoreAuthenticationSchemeSearchService` | Search authentication schemes filtered by store ID. |

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
