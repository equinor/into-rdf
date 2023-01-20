module API.Fake
open Services.RdfServices
open System.Threading.Tasks
open System.Net.Http
open System.Net.Http.Headers
open System.Net
open VDS.RDF
open VDS.RDF.Parsing
open VDS.RDF.Query
open VDS.RDF.Writing
open VDS.RDF.Query.Datasets

type RdfService =
    new() = {}
    member s.model = 
        let parser = TurtleParser()
        use g = new Graph()
        parser.Load(g, "TestData/revisionTestTrain.ttl")
        g
    member s.parser = new SparqlQueryParser()
    member s.processor = new LeviathanQueryProcessor(new InMemoryDataset(s.model))
    interface IRdfService with
        member this.QueryFusekiAsUser(server, query, accepts) = 
            let q = this.parser.ParseFromString(query)
            let resultContent, resultType = 
                match this.processor.ProcessQuery(q) with
                | :? IGraph as r -> 
                    let w = new CompressingTurtleWriter()
                    StringWriter.Write(r, w), "text/turtle"
                | :? SparqlResultSet as s -> 
                    let w = new SparqlJsonWriter()
                    StringWriter.Write(s, w), "application/sparql-results+json"
                | _ -> failwith "Unknown result type"
            Task.FromResult(
                let m = new HttpResponseMessage(HttpStatusCode.OK)
                m.Content <- new StringContent(resultContent, MediaTypeHeaderValue.Parse resultType)
                m
            )
