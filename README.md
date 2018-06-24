# DynamicRoleBasedAuthorization

## Background

This project is followed the article [Dynamic Role-Based Authorization in ASP.NET Core 2.0](https://www.codeproject.com/Articles/1248792/Dynamic-Role-Based-Authorization-in-ASP-NET-Core), and forked from the [github project](https://github.com/mo-esmp/DynamicRoleBasedAuthorizationNETCore).

## Feature

In this project, I make some change like

1. Use the **ASP.NET Core 2.1** to create the new project. So you will find out the identity didn't contains in the project directly. You can get more detail from [official website](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-2.1&tabs=visual-studio).
2. In orginal project, the **access** property is belong to **ApplicationRole**. I use the **RoleClaims** to store the access type instead of  role property. It can use the IdentityUser, IdentityRole directly.
3. Add **SeedData** class to initialize the data. We can get the default data like
   1. Default User: admin@demo.com, Pwd:123qwe!@#QWE
   2. Default Role: Administrator, and it contains **All** access function.
4. Add missing library like jquery-bonsai, jquery-qubit.

## Run

1. Download the project.
2. Use `Update-database` to create the database.
3. Run the project.
4. Visit **Contract** page, it contains the initilize data function.
5. Now you can login with admin@demo.com, and test the authorization function.