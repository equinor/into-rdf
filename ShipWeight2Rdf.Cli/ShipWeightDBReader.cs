using Microsoft.Data.SqlClient;
using System.Data;

namespace ShipWeight.Database;
public static class ShipWeightDBReader
{
    private static string GetConnectionString()
    {
        var omniaServer = "plantengineeringsqlprod.database.windows.net";
        var omniaDb = "ShipWeight";
        return $"Server={omniaServer}; Authentication=Active Directory Default; Database={omniaDb};";
    }

    public static DataSet GetData(string assetName)
    {
        DataSet dataset = new DataSet();
        dataset.Tables.Add(GetWeightData(assetName));
        dataset.Tables.Add(GetPhaseCodeData(assetName));
        dataset.Tables.Add(GetPhaseFilterData(assetName));

        return dataset;
    }

    private static DataTable GetWeightData(string assetName)
    {
        string connectionString = GetConnectionString();

        List<(string, string)> columns = GetDefaultColumns();
        columns.AddRange(GetCodeTypeColumns(assetName, connectionString));

        DataTable data = GetEquipment(assetName, connectionString, columns);
        data.TableName = "Weight";

        return data.Copy();
    }

    //Phase referes to building phase, i.e when something was built
    private static DataTable GetPhaseCodeData(string assetName)
    {
        string connectionString = GetConnectionString();
        DataSet dataset = new DataSet();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT ProjectID, CodeID, Description, Start, Stop FROM om.Code WHERE ProjectId = '{assetName}' AND CodeType = 'C02'";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandTimeout = 120;

            connection.Open();
            dataAdapter.Fill(dataset);
        }

        var data = dataset.Tables[0];
        data.TableName = "PhaseCode";

        return data.Copy();
    }

    //Phase filter is an normalized timestamp that can be used to detect (among other things) the as-built state
    private static DataTable GetPhaseFilterData(string assetName)
    {
        string connectionString = GetConnectionString();
        DataSet dataset = new DataSet();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT ProjectID, FilterID, Time FROM om.ITEM_FILTER WHERE ProjectId = '{assetName}'";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandTimeout = 120;

            connection.Open();
            dataAdapter.Fill(dataset);
        }

        var data = dataset.Tables[0];
        data.TableName = "PhaseFilter";

        return data.Copy();
    }

    private static List<(string, string)> GetCodeTypeColumns(string assetName, string connectionString)
    {
        List<(string, string)> codeTypesColumns = new List<(string, string)>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT CodeType, Title FROM om.CODETYPE WHERE ProjectID = '{assetName}'";

            SqlCommand command = new SqlCommand(query, connection);
            command.Connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                throw new InvalidDataException($"{assetName} does not have code types. ProjectID may be wrong, or data doesn't exist");
            }

            while (reader.Read())
            {
                codeTypesColumns.Add((reader[0].ToString()!, reader[1].ToString()!));
            }

        }

        return codeTypesColumns;
    }

    private static DataTable GetEquipment(string assetName, string connectionString, List<(string, string)> columns)
    {
        string query = CreateQuery(assetName, columns);
        DataSet dataset = new DataSet();
        
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandTimeout = 120;

            connection.Open();
            dataAdapter.Fill(dataset);
        }

        return dataset.Tables[0];
    }

    private static string CreateQuery(string assetName, List<(string, string)> columns)
    {
        var query = $"SELECT ";

        

        foreach (var column in columns)
        {
            query = $"{query} {column.Item1} AS '{column.Item2}',";
        }

        query = query.Remove(query.Length - 1);

        query = $"{query} FROM om.ITEM WHERE ProjectID = '{assetName}' AND C12 = 'E'";
        return query;
    }

    private static List<(string, string)> GetDefaultColumns()
    {
        List<(string, string)> defaultColumns = new List<(string, string)>
        {
            ("META_SourceDatabase", "META_SourceDatabase"),
            ("ProjectID", "ProjectID"),
            ("WgtGrp", "WgtGrp"),
            ("ItemNo", "ItemNo"),
            ("Description", "Description"),
            ("RegUser", "RegUser"),
            ("RegDate", "RegDate"),
            ("CAST(NoOff AS NVARCHAR)", "NoOff"),
            ("CAST(Factor AS NVARCHAR)", "Factor"),
            ("CAST(Length AS NVARCHAR)", "Length"),
            ("CAST(Width AS NVARCHAR)", "Width"),
            ("CAST(UnitWeight AS NVARCHAR)", "UnitWeight"),
            ("CAST(Weight AS NVARCHAR)", "Weight"),
            ("CAST(VCG AS NVARCHAR)", "VCG"),
            ("CAST(LCG AS NVARCHAR)", "LCG"),
            ("CAST(TCG AS NVARCHAR)", "TCG"),
            ("EditUser", "EditUser"),
            ("EditDate", "EditDate"),
            ("UniqueNo", "UniqueNo")
        };

        return defaultColumns;
    }
}
