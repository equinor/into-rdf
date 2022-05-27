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

    public static string GetPlantId(string facilityName)
    {
        string connectionString = GetConnectionString();

        var codeColumns = GetCodeTypeColumns(facilityName, connectionString);
        var plantIdColumn = (codeColumns.First(x => x.Item2 == "Plant")).Item1;

        var plantId = string.Empty;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT TOP 1 {plantIdColumn} FROM om.ITEM WHERE ProjectID = '{facilityName}'";

            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            
            plantId = (string)command.ExecuteScalar();
        }
        return plantId;
    }

    public static DataTable GetData(string facilityName, string tableName)
    {
        string connectionString = GetConnectionString();

        DataSet dataset = new DataSet();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT * FROM om.{tableName} WHERE ProjectId = '{facilityName}'";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandTimeout = 120;

            connection.Open();
            dataAdapter.Fill(dataset);
        }

        var data = dataset.Tables[0];
        data.TableName = tableName.ToLower();

        return data.Copy();
    }

    public static DataTable GetAsBuiltData(string facilityName)
    {
        string connectionString = GetConnectionString();

        List<(string, string)> columns = GetDefaultColumns();
        columns.AddRange(GetCodeTypeColumns(facilityName, connectionString));

        DataTable items = GetAsBuiltItems(facilityName, connectionString, columns);
        items.TableName = "item";

        return items.Copy();
    }

    //Phase referes to building phase, i.e when something was built
    private static List<string> GetAsBuiltPhases(string facilityName)
    {
        var builtTime = GetAsBuiltTime(facilityName);

        string connectionString = GetConnectionString();
        DataSet dataset = new DataSet();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT ProjectID, CodeID, Description, Start, Stop FROM om.Code WHERE ProjectId = '{facilityName}' AND CodeType = 'C02'";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandTimeout = 120;

            connection.Open();
            dataAdapter.Fill(dataset);
        }

        var dataRows = dataset.Tables[0].Select($"Start <= {builtTime} AND Stop >= {builtTime}");

        List<string> asBuiltPhases = dataRows.Select(r => r["CodeId"].ToString()!).ToList();

        return asBuiltPhases;
    }

    //Phase filter is an normalized timestamp that can be used to detect (among other things) the as-built state
    private static int GetAsBuiltTime(string facilityName)
    {
        string connectionString = GetConnectionString();
        DataSet dataset = new DataSet();

        var builtTime = -1;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT ProjectID, FilterID, Time FROM om.ITEM_FILTER WHERE ProjectId = '{facilityName}' AND FilterID LIKE '%Built%'";

            connection.Open();
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Int32.TryParse(reader["Time"].ToString(), out builtTime);
                break;
            }
        }

        if (builtTime == -1)
        {
            throw new InvalidOperationException($"No As-Built phase found for {facilityName}");
        }

        return builtTime;
    }

    private static List<(string, string)> GetCodeTypeColumns(string facilityName, string connectionString)
    {
        List<(string, string)> codeTypesColumns = new List<(string, string)>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = $"SELECT CodeType, Title FROM om.CODETYPE WHERE ProjectID = '{facilityName}'";

            SqlCommand command = new SqlCommand(query, connection);
            command.Connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                throw new InvalidDataException($"{facilityName} does not have code types. ProjectID may be wrong, or data doesn't exist");
            }

            while (reader.Read())
            {
                codeTypesColumns.Add((reader[0].ToString()!, reader[1].ToString()!));
            }

        }

        return codeTypesColumns;
    }

    private static DataTable GetAsBuiltItems(string facilityName, 
                                            string connectionString, 
                                            List<(string, string)> columns)
    {

        var asBuiltPhases = GetAsBuiltPhases(facilityName);

        string query = CreateQuery(facilityName, columns);
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

        DataTable items = dataset.Tables[0].Clone();

        foreach (DataRow row in dataset.Tables[0].Rows)
        {
            if (asBuiltPhases.Contains(row["Phase Code"]))
            {
                items.ImportRow(row);
            }
        }

        return items;
    }

    private static string CreateQuery(string facilityName, List<(string, string)> columns)
    {
        var query = $"SELECT ";

        foreach (var column in columns)
        {
            query = $"{query} {column.Item1} AS '{column.Item2}',";
        }

        query = query.Remove(query.Length - 1);

        query = $"{query} FROM om.ITEM WHERE ProjectID = '{facilityName}' AND C12 = 'E'";
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
