# Pug.Groups

Pug.Groups is a framework for and implementation of multi-tenant (domains) group membership management.

In addition, it also provides implementation for IUserRoleProvider that can be used by authorization provider library intended to be used to provide access control mechanism to it's own resources.

## Projects Structure

- Pug.Groups.Models: Contains domain entity models for group membership management
- Pug.Groups.Common: Contains domain level abstractions
- Pug.Groups.Data.Common: Contains abstractions for data access layer components
- Pug.Groups.Data.Postgres: Contains implementation for data access layer components for PostgreSQL database
- Pug.Groups.Data.Sqlite: Contains implementation for data access layer components for SQLite database
- Pug.Groups.DependencyInjection: Contains .NET IServiceCollection dependency injection configuration for Pug.Groups