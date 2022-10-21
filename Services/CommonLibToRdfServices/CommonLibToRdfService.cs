using Common.GraphModels;
using Common.ProvenanceModels;
using Services.CommonlibServices;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
using Services.ProvenanceServices;

namespace Services.CommonLibToRdfServices;

public class CommonLibToRdfService : ICommonLibToRdfService
{
    private readonly ICommonLibService _commonLibService;
    private readonly ICommonLibTransformationService _commonLibTransformationService;
    private readonly IOntologyService _ontologyService;
    private readonly IFusekiService _fusekiService;
    private readonly IProvenanceService _provenanceService;

    public CommonLibToRdfService(ICommonLibService commonLibService, ICommonLibTransformationService commonLibTransformationService, IOntologyService ontologyService, IFusekiService fusekiService, IProvenanceService provenanceService)
    {
        _commonLibService = commonLibService;
        _commonLibTransformationService = commonLibTransformationService;
        _ontologyService = ontologyService;
        _fusekiService = fusekiService;
        _provenanceService = provenanceService;
    }

    public async Task<ResultGraph?> MoveCommonlibLibraryToRdf(string server, string library, string scope)
    {
        var lowercaseLibrary = library.ToLower();
        var upperCaseScope = scope.ToUpper();

        var records = await _commonLibService.GetFromScopedLibrary(lowercaseLibrary, upperCaseScope);

        if (!records.Any()) return null;

        var provenance = await _provenanceService
            .CreateProvenanceFromCommonLib(
                lowercaseLibrary,
                new RevisionRequirement(upperCaseScope, $@"{upperCaseScope}/commonlib/{lowercaseLibrary}", string.Empty, GetLatestCreatedDate(records))
            );

        if (provenance.RevisionStatus is not RevisionStatus.New or RevisionStatus.Update) return null;

        var ontology = await _ontologyService.GetSourceOntologies(lowercaseLibrary);
        var resultGraph = _commonLibTransformationService.Transform(provenance, ontology, records);
        await _fusekiService.PostAsApp(server, resultGraph, "text/turtle");
        return resultGraph;
    }

    private static DateTime GetLatestCreatedDate(List<Dictionary<string, object>> records)
    {
        return records
                .Where(record => record.ContainsKey("DateCreated"))
                .Max(record => Convert.ToDateTime(record["DateCreated"].ToString()));
    }
}
