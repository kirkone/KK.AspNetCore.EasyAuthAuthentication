# KK.AspNetCore.EasyAuthAuthentication

This helps getting azure appservice authentication working with asp.net core

> **Caution**: This project is not finished jet!

## Nuget

The EasyAuth handler is provided as a nuget package and can be found on nuget.org.

| Name                                 | Status                                                                                                                                                          |
| ------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| KK.AspNetCore.EasyAuthAuthentication | [![Nuget Badge](https://img.shields.io/nuget/v/KK.AspNetCore.EasyAuthAuthentication.svg)](https://www.nuget.org/packages/KK.AspNetCore.EasyAuthAuthentication/) |

You can add the package for example with the following `dotnet` command:

```
dotnet add package KK.AspNetCore.EasyAuthAuthentication
```

Pre-releases of this Package are pushed to an internal feed an Azure DevOps. There is no public access to this feeds at the moment.

## Build

The build environment for this project is on Azure DevOps and can be found here [dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&view=mine&_a=releases)

### Nuget package build

| Name                                    | Status                                                                                                                                                                                                                                             |
| --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| KK.AspNetCore.EasyAuthAuthentication-CI | [![Build Status](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_apis/build/status/KK.AspNetCore.EasyAuthAuthentication-CI)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_build/latest?definitionId=24) |
| Alpha                                   | [![Alpha](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/1)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                       |
| Beta                                    | [![Beta](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/2)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                        |
| Release                                 | [![Release](https://vsrm.dev.azure.com/kirkone/_apis/public/Release/badge/b206bf59-b281-4d06-91c3-3877c3aeaaf9/1/3)](https://dev.azure.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/_releases2?definitionId=1&_a=releases)                     |

### Sample Web build

No build so far.

## Usage

> **INFO**: For detailed usage information please have a look in the `KK.AspNetCore.EasyAuthAuthentication.Sample` project.

### Startup.cs

Add something like this in the `public void ConfigureServices` method:

```
services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = EasyAuthAuthenticationDefaults.AuthenticationScheme;
    }
).AddEasyAuth();
```

and this to the `public void Configure` method before `app.UseMvc...`:

```
app.UseAuthentication();
```

This will enable the `EasyAuthAuthenticationHandler` in your app.

### ...Controller.cs

In your controllers you can access the `User` property as usual:

```
[Authorize]
public string UserName()
{
    var mulps = User.HasClaim(ClaimTypes.Name, "user@somecloud.onmicrosoft.com");
    var peng = User.HasClaim(ClaimTypes.Role, "SystemAdmin");
    var blubb = HttpContext.User.IsInRole("SystemAdmin");
    var pop = User.IsInRole("SystemAdmin");
    return HttpContext.User.Identity.Name;
}
```

### Adding custom roles

If you want to add roles to the `User` property you can have a look in `Transformers/ClaimsTransformer.cs` in the Sample project. There you can see an example how to get started with this.

## Authors

* **Kirsten Kluge** - *Initial work* - [kirkone](https://github.com/kirkone)
* **paule96** - *Refactoring* - [paule96](https://github.com/paule96)
* **Christoph Sonntag** - *Made things even more uber* - [Compufreak345](https://github.com/Compufreak345)

See also the list of [contributors](https://github.com/kirkone/KK.AspNetCore.EasyAuthAuthentication/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Inspired by this [StackOverflow post](https://stackoverflow.com/a/42402163/6526640) and this [GitHub](https://github.com/lpunderscore/azureappservice-authentication-middleware) repo
