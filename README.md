# KK.AspNetCore.EasyAuthAuthentication

This helps getting azure appservice authentication working with asp.net core

## Nuget

The EasyAuth handler is provided as a nuget package and can be found on nuget.org.

| Name                                 | Status                                                                                                                                                          |
| ------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| KK.AspNetCore.EasyAuthAuthentication | [![Nuget Badge](https://img.shields.io/nuget/v/KK.AspNetCore.EasyAuthAuthentication.svg)](https://www.nuget.org/packages/KK.AspNetCore.EasyAuthAuthentication/) |

You can add the package for example with the following `dotnet` command:

```bash
dotnet add package KK.AspNetCore.EasyAuthAuthentication
```

Pre-releases of this Package are pushed to an internal feed an Azure DevOps. There is no public access to this feeds at the moment.

## Build

The build environment for this project is on Azure DevOps and can be found here [dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_build)

### Nuget package build

| Name                                    | Status                                                                                                                                                                                                                                             |
| --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| KK.AspNetCore.EasyAuthAuthentication-CI | [![Build Status](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_apis/build/status/KK.AspNetCore.EasyAuthAuthentication-CI)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_build/latest?definitionId=24) |
| Alpha                                   | [![Alpha](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/1)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                       |
| Beta                                    | [![Beta](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/2)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                        |
| Release                                 | [![Release](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/3)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                     |

## Usage

> **INFO**: For detailed usage information please have a look in the `samples` folder.

### Startup.cs

Add something like this in the `public void ConfigureServices` method:

```csharp
services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
    }
).AddEasyAuth();
```

and this to the `public void Configure` method before `app.UseMvc...`:

```csharp
app.UseAuthentication();
```

This will enable the `EasyAuthAuthenticationHandler` in your app.

### ...Controller.cs

In your controllers you can access the `User` property as usual:

```csharp
[Authorize]
public string UserName()
{
    _ = User.HasClaim(ClaimTypes.Name, "user@somecloud.onmicrosoft.com");
    _ = User.HasClaim(ClaimTypes.Role, "SystemAdmin");
    _ = HttpContext.User.IsInRole("SystemAdmin");
    _ = User.IsInRole("SystemAdmin");
    return HttpContext.User.Identity.Name;
}
```

### Adding custom roles

If you want to add roles to the `User` property you can have a look in `Transformers/ClaimsTransformer.cs` in the Sample project. There you can see an example how to get started with this.

### Configure options via configuration (recommended)

You can use the default behavior of asp.net core to configure EasyAuth. You must only change in your `Startup.cs` the `.AddEasyAuth()` to `.AddEasyAuth(this.Configuration)`.

> To get the property `this.Configuration` in your `Startup.cs` you must add `IConfiguration configuration` to your constructor parameters and create a property.

To configure you providers you simple add the following to your appsettings.json. (or to your environment variables, or other [configuration sources](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/).)

```json
"easyAuthOptions": {
    "AuthEndpoint": ".auth/me",
    "providerOptions": [
      {
        "ProviderName": "EasyAuthForApplicationsService",
        "Enabled": true
      },
      {
        "ProviderName": "EasyAuthWithHeaderService",
        "Enabled": true
      }
    ]
  }
```

Here are some notes to the json above:

- each provider is disabled in default so you must enabled it
- you can create own providers but this must implement `IEasyAuthAuthentificationService`. But you must also activate them here. (Don't but them in the DI. This package will do this by it's own.)
- The `ProviderName` is the class name of the provider. that must be unique in your application.

> A list of all providers can be found in the headline `Auth Provider`

### Configure options via code (not recommended)

#### Custom options

You can provide additional options vor the middleware:

```csharp
).AddEasyAuth(
   options => {
      // Override the auth endpoint
      options.AuthEndpoint = ClaimTypes.Email;
      // Add the EasyAuthForApplicationService auth provider and enabled it. Also Change the NameClaimType
      options.AddProviderOptions(new ProviderOptions("EasyAuthForApplicationsService"){Enabled = true, NameClaimType = "Test"})
   }
);
```

The `NameClaimType` is the ClaimType of the value which one will be used to fill the `User.Identity.Name` field.

#### Local Debugging

For debugging your application you can place a `me.json` in the `wwwroot/.auth` folder of your web app and add some configuration to the `AddEasyAuth` call.  
For example:

```csharp
).AddEasyAuth(
    options =>
    {
        if (this.Environment.IsDevelopment())
        {
            options.AuthEndpoint = ".auth/me.json";
        }
    }
);
```

> **Info**: You can obtain the content for this file from an Azure Web App with EasyAuth configured by requesting the `/.auth/me` endpoint.

> **Info**: Make sure you added static file handling to your pipeline by adding `app.UseStaticFiles();` to your `public void Configure` method in the `Startup.cs`, e.g. just after `app.UseHttpsRedirection();` entry. Otherwise the static file can not be found at runtime.

> **Info**: Using a wwwroot sub-folder name that starts with `'.'`&nbsp;, like the suggested `.auth` folder name, is useful for content relevant only for localhost debugging as these are treated as hidden folders and are not included in publish output.

## Auth Provider

There are some predefined providers in this package. If you need your own or want contribute to our existing providers you must implement the `IEasyAuthAuthentificationService`.

### `EasyAuthWithAuthMeService` (always on)

This is a little bit special provider. That provider can't be configured and it isn't implementing `IEasyAuthAuthentificationService`. This provider is for the development case, so a developer can create a JSON with the content of the `/.auth/me` endpoint of an EasyAuth Azure Web App. So you don't need a internet connection or azure for development and can use only local things.

### `EasyAuthForApplicationsService`

This provider is for the case you have a Azure Web App that is not only be used by humans. so maybe you want access your app with an Service Principal (SPN). If you enabled this provider you can access your app with Azure Ad Service Principals (SPN).

To create an Service Principal (SPN) that can have access to your EasyAuth protected Application you must change the app manifest for you application in your Azure AD. Thanks to [Suzuko123](https://github.com/Suzuko123) for the following sample:

```json
"appRoles": [
	{
	    "allowedMemberTypes": [
			"Application"
		],
	    "description": "allow a call as system admin.",
		"displayName": "SystemAdmin",
		"id": "dd6d2784-5fa1-4c97-9f9b-8376a85b4163",
		"isEnabled": true,
		"lang": null,
		"origin": "Application",
		"value": "SystemAdmin"
	}
]
```

This will allow a spn to get the role `SystemAdmin` in your protected application. The default `User.Identity.Name` of an SPN is the SPN Guid.

### `EasyAuthWithHeaderService`

This is the most common auth provider. This let you use Azure Active Directory Users in your easy auth application.

## Authors

-   **Kirsten Kluge** - _Initial work_ - [kirkone](https://github.com/kirkone)
-   **paule96** - _Refactoring / implementing the new stuff_  - [paule96](https://github.com/paule96)
-   **Christoph Sonntag** - _Made things even more uber_ - [Compufreak345](https://github.com/Compufreak345)
-   **myusrn** - _Dropped some knowledge about making IsInRoles work_ - [myusrn](https://github.com/myusrn)
-   **Suzuko123** - _Dropped some knowledge about Service Principals with easy auth_ - [Suzuko123](https://github.com/Suzuko123)

See also the list of [contributors](https://github.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

-   Inspired by this [StackOverflow post](https://stackoverflow.com/a/42402163/6526640) and this [GitHub](https://github.com/lpunderscore/azureappservice-authentication-middleware) repo
