using Common.FusekiModels;
using Services.FusekiServices;


namespace Services.FusekiMappers;

public static class FusekiResponseToPropsMapper
{
    public static T Map<T>(Dictionary<string, FusekiTriplet> bindings) where T : new()
    {
        return Map(bindings, new T());
    }

    public static T Map<T>(Dictionary<string, FusekiTriplet> bindings, T obj)
    {
        if (obj == null) return obj;
        var propertyInfos = obj.GetType().GetProperties().ToDictionary(prop => prop.Name);
        foreach (var binding in bindings)
        {
            if (!propertyInfos.TryGetValue(binding.Key, out var prop)) continue;

            if (prop.PropertyType == typeof(string))
                prop.SetValue(obj, bindings.GetFusekiString(prop.Name), null);
            else if (prop.PropertyType == typeof(decimal))
                prop.SetValue(obj, bindings.GetFusekiDecimal(prop.Name), null);
            else if (prop.PropertyType == typeof(int))
                prop.SetValue(obj, bindings.GetFusekiInteger(prop.Name), null);
            else if (prop.PropertyType == typeof(Uri))
                prop.SetValue(obj, bindings.GetFusekiUri(prop.Name), null);
            else if (prop.PropertyType == typeof(DateTime))
                prop.SetValue(obj, bindings.GetFusekiDateTime(prop.Name), null);
            else throw new ArgumentException($"Unsupported type {prop.PropertyType} for {prop.Name}");
        }
        return obj;
    }

    public static List<T> MapResponse<T>(FusekiSelectResponse? fusekiResponse) where T : new()
    {
        return fusekiResponse switch
        {
            null => new List<T>(),
            { Results: null} => new List<T>(),
            { Results.Bindings: null} => new List<T>(),
            _ => fusekiResponse.Results.Bindings.Select(Map<T>).ToList()
        };
    }
}
