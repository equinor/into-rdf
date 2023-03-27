using IntoRdf.Public.Exceptions;
using IntoRdf.Public.Models;

namespace IntoRdf.Validation
{
    internal class TransformationDetailsValidation
    {
        public static void ValidateTransformationDetails(TransformationDetails details)
        {
            if (details.BaseUri != null)
            {
                CheckUri(details.BaseUri, nameof(details.BaseUri));
            }

            if (details.SourcePredicateBaseUri != null)
            {
                CheckUri(details.SourcePredicateBaseUri, nameof(details.SourcePredicateBaseUri));
            }
        }

        private static void CheckUri(Uri uri, string nameForDebug) {
            if (! uri.AbsoluteUri.EndsWith("/") && ! uri.AbsoluteUri.EndsWith("#"))
            {
                throw new IntoRdfException($"{nameForDebug} {uri} is invalid, must end with '/' or '#'");
            }
        }
    }
}