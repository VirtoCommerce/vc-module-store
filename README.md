# VirtoCommerce.Store

VirtoCommerce.Store module represents stores management system.

 **Key features**:

1. Multiple stores;
1. SEO management;
1. Individual payment and shipping methods;
1. Tax providers management;
1. Individual settings for each store;
1. Customer account linking between stores;
1. Store asset management.

A store in VirtoCommerce represents the collection of settings for the web page (storefront) to function correctly. That includes item catalog to showcase, available currencies, languages, payment methods, even UI themes, pages and much more. A default storefront as well as sample stores are provided by the VirtoCommerce team.

Virto Commerce platform is  multi-store by design. This gives the ability to have and run multiple stores on the same system.

![stores UI](https://cloud.githubusercontent.com/assets/5801549/15567593/855799ac-2327-11e6-9c87-b0ff7811c48e.png)

## Manage Stores

[Add New Store](/docs/add-new-store.md)

[Edit Store Details](/docs/edit-store-details.md)

[Edit Widgets](/docs/edit-widgets.md)

## Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Store module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-store/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

## Settings
* **Stores.States** - states that a store can be in (Open, Closed, Restricted Access, etc.);
* **Stores.SeoLinksType** - determines how to generate links for products and categories. Long: /grandparent-category/parent-category/my-cool-category/my-cool-product, Short: /my-cool-product, None: /product/123.

## Available resources
* Module related service implementations as a <a href="https://www.nuget.org/packages/VirtoCommerce.StoreModule.Data" target="_blank">NuGet package</a>
* API client as a <a href="https://www.nuget.org/packages/VirtoCommerce.StoreModule.Client" target="_blank">NuGet package</a>
* API client documentation http://demo.virtocommerce.com/admin/docs/ui/index#!/Store_module

## License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
