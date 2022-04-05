using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace Doc2Rdf.Library.IO
{
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

            List<Tuple<string, string>> columns = GetDefaultColumns();
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

        private static List<Tuple<string, string>> GetCodeTypeColumns(string assetName, string connectionString)
        {
            List<Tuple<string, string>> codeTypesColumns = new List<Tuple<string, string>>();

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
                    codeTypesColumns.Add(new Tuple<string, string>(reader[0].ToString()!, reader[1].ToString()!));
                }
            }

            return codeTypesColumns;
        }

        private static DataTable GetEquipment(string assetName, string connectionString, List<Tuple<string, string>> columns)
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

        private static string CreateQuery(string assetName, List<Tuple<string, string>> columns)
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

        private static List<Tuple<string, string>> GetDefaultColumns()
        {
            List<Tuple<string, string>> defaultColumns = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("META_SourceDatabase", "META_SourceDatabase"),
            new Tuple<string, string>("ProjectID", "ProjectID"),
            new Tuple<string, string>("WgtGrp", "WgtGrp"),
            new Tuple<string, string>("ItemNo", "ItemNo"),
            new Tuple<string, string>("Description", "Description"),
            new Tuple<string, string>("RegUser", "RegUser"),
            new Tuple<string, string>("RegDate", "RegDate"),
            new Tuple<string, string>("CAST(NoOff AS NVARCHAR)", "NoOff"),
            new Tuple<string, string>("CAST(Factor AS NVARCHAR)", "Factor"),
            new Tuple<string, string>("CAST(Length AS NVARCHAR)", "Length"),
            new Tuple<string, string>("CAST(Width AS NVARCHAR)", "Width"),
            new Tuple<string, string>("CAST(UnitWeight AS NVARCHAR)", "UnitWeight"),
            new Tuple<string, string>("CAST(Weight AS NVARCHAR)", "Weight"),
            new Tuple<string, string>("CAST(VCG AS NVARCHAR)", "VCG"),
            new Tuple<string, string>("CAST(LCG AS NVARCHAR)", "LCG"),
            new Tuple<string, string>("CAST(TCG AS NVARCHAR)", "TCG"),
            new Tuple<string, string>("EditUser", "EditUser"),
            new Tuple<string, string>("EditDate", "EditDate"),
            new Tuple<string, string>("UniqueNo", "UniqueNo")
        };

            return defaultColumns;
        }
    }
}