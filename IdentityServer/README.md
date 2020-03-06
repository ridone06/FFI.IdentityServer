# Identity Server for FFI

## 1. Build requirements

* Visual Studio 2019
	* `.NET Core 3.1.1` or newer
	* `Identity Server4 3.1.2`

## 2. Visual Studio Build

Build `FFI.sln` using Visual Studio 2019.

## 3. Migration Database CLI (`Run on Package Manager Console` on VS 2019) if migration not exist

```txt
	dotnet ef migrations add InitialUsersDbMigration -c ApplicationDbContext -o Data/Migrations --project IdentityServer
	dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb --project IdentityServer
	dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb --project IdentityServer
```

Run and build app then check database `IdentityServer`

## Copyright

Copyright 2020 - now.

Licensed under the [MIT License](LICENSE.md)
