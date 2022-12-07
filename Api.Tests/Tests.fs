module Api.Tests

open Xunit
open FsUnit.Xunit
open System.Net.Http
open System.Net.Http.Headers
open System.Net
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Services.RdfServices
open API
open Api.Test
open FsUnit.CustomMatchers
open System.Threading.Tasks

let query = "select ?p (count(?s) as ?cont) where {graph ?g {?s ?p ?o} union {?s ?p ?o}} group by ?p  limit 3"

let post (server: TestServer) method (uri: string) (contentType: string) payload = 
    server
        .CreateRequest(uri)
        .And(fun req -> req.Content <- new StringContent(payload, System.Text.Encoding.UTF8, contentType))
        .PostAsync() 
        |> Async.AwaitTask
    

let (^<) f1 a b = f1 (b, a >> ignore) 
let send (server:TestServer) (request: HttpRequestMessage) =
    server.Host.GetTestClient().SendAsync request |> Async.AwaitTask


type ContentTypeFixture() =
    member val server : TestServer = null with get,set
    interface IAsyncLifetime with
        member f.InitializeAsync() = 
            task {
                let h = 
                    (host
                        (
                            ServiceCollectionServiceExtensions.AddScoped<IRdfService, Fake.RdfService>
                            >> ServiceCollectionExtensions.AddAPIEndpoints
                            )

                        (fun x -> (EndpointRoutingApplicationBuilderExtensions.UseEndpoints 
                        ^< RouteBuilderExtensions.MapAPIEndpoints) x)
                    )
                let! t = h.Host.StartAsync() |> Async.AwaitTask
                printf "%s\n" "Host started..."
                f.server <- h
            } 
        member f.DisposeAsync() = Task.CompletedTask

type ContentTypeTests(fixture: ContentTypeFixture) =
    
    let testhost = fixture.server
        

    [<Theory>]
    [<InlineData("text/plain", 200)>]
    [<InlineData("application/sparql-query", 200)>]
    [<InlineData("text/csv", 415)>]
    member t.``Content-Type: query/[instance]/sparql`` (contentType: string, status: int) =
        async {
            let! response =
                post testhost  HttpMethod.Post "query/test/sparql" contentType query
            let! out = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            // printf "%d %s\n%s" (int response.StatusCode) response.ReasonPhrase out
            int response.StatusCode |> should equal status
        }
    member t.``Accept:  query/[instance]/sparql`` (accept: string, ok:bool) =
        true
    
    interface IClassFixture<ContentTypeFixture>

