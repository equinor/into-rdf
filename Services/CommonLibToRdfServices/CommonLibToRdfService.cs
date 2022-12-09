using Common.GraphModels;
using Common.ProvenanceModels;
using Common.Utils;
using Services.CommonlibServices;
using Services.FusekiServices;
using Services.ProvenanceServices;
using Repositories.OntologyRepository;

namespace Services.CommonLibToRdfServices;

public class CommonLibToRdfService : ICommonLibToRdfService
{
    private readonly ICommonLibService _commonLibService;
    private readonly ICommonLibTransformationService _commonLibTransformationService;
    private readonly IOntologyRepository _ontologyRepository;
    private readonly IFusekiService _fusekiService;
    private readonly IProvenanceService _provenanceService;

    public CommonLibToRdfService(ICommonLibService commonLibService, ICommonLibTransformationService commonLibTransformationService, IOntologyRepository ontologyRepository, IFusekiService fusekiService, IProvenanceService provenanceService)
    {
        _commonLibService = commonLibService;
        _commonLibTransformationService = commonLibTransformationService;
        _ontologyRepository = ontologyRepository;
        _fusekiService = fusekiService;
        _provenanceService = provenanceService;
    }

    public async Task<ResultGraph?> MoveCommonlibLibraryToRdf(string server, string library, string scope)
    {
        var lowercaseLibrary = library.ToLower();
        var upperCaseScope = scope.ToUpper();

        var records = await _commonLibService.GetFromScopedLibrary(lowercaseLibrary, upperCaseScope);

        var revisionTrain = $@"{upperCaseScope}/commonlib/{lowercaseLibrary}";
        var previousRevisions = await _provenanceService.GetPreviousRevisions(server, revisionTrain);

        if (!records.Any()) return null;

        var provenance = _provenanceService
            .CreateProvenanceFromCommonLib(
                lowercaseLibrary,
                previousRevisions,
                new RevisionRequirement(upperCaseScope, revisionTrain, string.Empty, GetLatestCreatedDate(records))
            );

        if (provenance.RevisionStatus is not RevisionStatus.New or RevisionStatus.Update) return null;

        var ontology = await _ontologyRepository.Get(ServerKeys.Main, lowercaseLibrary);
        var resultGraph = _commonLibTransformationService.Transform(provenance, ontology, records);
        await _fusekiService.AddData(server, resultGraph, "text/turtle");
        return resultGraph;
    }

    private static DateTime GetLatestCreatedDate(List<Dictionary<string, object>> records)
    {
        return records
                .Where(record => record.ContainsKey("DateCreated"))
                .Max(record => Convert.ToDateTime(record["DateCreated"].ToString()));
    }
}
