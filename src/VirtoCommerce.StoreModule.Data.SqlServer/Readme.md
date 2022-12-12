
## Package manager 
Add-Migration RenameStoreIndex -Context VirtoCommerce.StoreModule.Data.Repositories.StoreDbContext  -Verbose -OutputDir Migrations -Project VirtoCommerce.StoreModule.Data.SqlServer -StartupProject VirtoCommerce.StoreModule.Data.SqlServer  -Debug


### Entity Framework Core Commands
```

dotnet tool install --global dotnet-ef --version 6.*
```

**Generate Migrations**

```
dotnet ef migrations add Initial -- "{connection string}"
dotnet ef migrations add Update1 -- "{connection string}"
dotnet ef migrations add Update2 -- "{connection string}"
```

etc..

**Apply Migrations**

`dotnet ef database update -- "{connection string}"`
