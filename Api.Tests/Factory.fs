module Api.Test
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open System.Text.Encodings.Web
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Hosting
open Api.Authorization.Handlers.Fallback
open Microsoft.Extensions.Hosting
open System.Security.Cryptography.Xml
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Xunit

type AuthOptions() =
    inherit AuthenticationSchemeOptions()
    member val DefaultUserId : string option  = None with get,set

[<Literal>]
let ``User Id header for test`` ="user-id"
[<Literal>]
let ``Authentication scheme for test`` = "test"

type AuthHandler(
        options: IOptionsMonitor<AuthOptions>,
        logger: ILoggerFactory ,
        encoder: UrlEncoder ,
        clock: ISystemClock) =
    inherit AuthenticationHandler<AuthOptions>(options, logger, encoder, clock)
    member val DefaultUserId = options.CurrentValue.DefaultUserId with get, set

    override __.HandleAuthenticateAsync() =
        let c = base.Context
        let claims = [
            yield Claim(ClaimTypes.Name, "test user")
            yield Claim(ClaimTypes.Role, "admin")
            match c.Request.Headers.TryGetValue ``User Id header for test`` with 
            | true, value -> yield Claim(ClaimTypes.NameIdentifier, value[0])
            | _-> ()
            ]
        Task.FromResult (
            AuthenticateResult.Success (
                AuthenticationTicket(
                    ClaimsPrincipal(
                        ClaimsIdentity(
                            claims, 
                            ``Authentication scheme for test``)), 
                    ``Authentication scheme for test``)))


// Test Auth service registration delegates
let testHostBuilder service config (a: IWebHostBuilder) = 
    a.ConfigureServices(fun s  -> 
        s.Configure(fun (o: AuthOptions) -> 
            o.DefaultUserId <- Some "1"
        ).AddAuthentication(
            ``Authentication scheme for test``
        ).AddScheme<AuthOptions, AuthHandler>(``Authentication scheme for test``, ignore) |> ignore
        service s
        // s.AddMvc().AddControllersAsServices() |> ignore
    ).Configure(
        AuthAppBuilderExtensions.UseAuthentication >>
        EndpointRoutingApplicationBuilderExtensions.UseRouting >> 
        AuthorizationAppBuilderExtensions.UseAuthorization >> 
        config
    )// |> HostBuilderExtensions.AddLogging


let host service config =
    new TestServer(
        new WebHostBuilder()
        |> testHostBuilder 
            (service >> ignore)
            (config >> ignore)
    )

