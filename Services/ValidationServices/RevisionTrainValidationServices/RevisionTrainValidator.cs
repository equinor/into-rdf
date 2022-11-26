using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using VDS.RDF.Shacl.Validation;

namespace Services.ValidationServices.RevisionTrainValidationServices;

public class RevisionTrainValidator : IRevisionTrainValidator
{
    public Report ValidateRevisionTrain(string turtle)
    {
        var revisionTrain = new Graph();
        revisionTrain.LoadFromString(turtle, new TurtleParser()); 

       var shapesGraph = GetRevisionTrainShape();

        return shapesGraph.Validate(revisionTrain);
    }

    private ShapesGraph GetRevisionTrainShape()
    {
        var revisionTrainShape = 
            @$"@prefix commonlib: <https://rdf.equinor.com/commonlib/tie#> .
            @prefix splinter: <https://rdf.equinor.com/splinter#> .
            @prefix spreadsheet: <https://rdf.equinor.com/splinter/spreadsheet#> .
            @prefix sh: <http://www.w3.org/ns/shacl#> .
            @prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
            
            splinter:RevisionTrainShape 
                a sh:NodeShape ;
                sh:targetClass splinter:RevisionTrain ;
                sh:property [
                    sh:path splinter:name ;
                    sh:minCount 1 ; 
                    sh:maxCount 1 ; 
                    sh:datatype xsd:string ;
                ] ;
                sh:property [
                    sh:path splinter:tripleStore ;
                    sh:minCount 1 ; 
                    sh:maxCount 1 ; 
                    sh:datatype xsd:string ;
                ] ;
                sh:property [
                    sh:path splinter:trainType ;
                    sh:minCount 1 ; 
                    sh:maxCount 1 ; 
                    sh:datatype xsd:string ;
                ] ;
                sh:property [
                    sh:path splinter:hasTieContext ;
                    sh:class splinter:TieContext ;
                    sh:minCount 1 ; 
                    sh:maxCount 1 ; 
                    sh:nodeKind sh:IRI ;
                ] .
                
            splinter:TieContextShape
                a sh:NodeShape ;
                sh:targetClass splinter:TieContext ;
                sh:property [
                    sh:path commonlib:facilityName ;
                    sh:minCount 1 ;
                    sh:maxCount 1 ;
                    sh:datatype xsd:string ;
                ] ;
                sh:property [
                    sh:path commonlib:objectName ;
                    sh:minCount 1 ;
                    sh:maxCount 1 ;
                    sh:datatype xsd:string ;
                ] ;
                sh:property [
                    sh:path commonlib:contractNumber ;
                    sh:minCount 1 ;
                    sh:maxCount 1 ;
                    sh:datatype xsd:string ;
                ] .";

        var shaclGraph = new Graph();
        shaclGraph.LoadFromString(revisionTrainShape, new TurtleParser());

        ShapesGraph shapesGraph = new ShapesGraph(shaclGraph);

        return shapesGraph;
    }
}