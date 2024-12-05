using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
namespace CustomORM
{
        public class TableSchema
        {
            public string TableName { get; set; }
            public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
        }

        public class ColumnSchema
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public string ForeignKeyTable { get; set; }
            public string ForeignKeyColumn { get; set; }
            public bool IsUnique { get; set; }
            public bool IsIndexed { get; set; }
            public object DefaultValue { get; set; }
            public string CheckConstraint { get; set; }
    }
    




        public class SchemaSynchronizer
        {
            public static string GenerateCreateTableScript(Type modelType)
            {
                var tableAttribute = modelType.GetCustomAttribute<TableAttribute>();
                if (tableAttribute == null)
                    throw new InvalidOperationException($"Class {modelType.Name} must have a [Table] attribute.");

                string tableName = tableAttribute.Name;
                List<string> columnDefinitions = new List<string>();

                foreach (var property in modelType.GetProperties())
                {
                    string columnDefinition = GenerateColumnDefinition(property);
                    if (!string.IsNullOrEmpty(columnDefinition))
                    {
                        columnDefinitions.Add(columnDefinition);
                    }
                }

                string columnsSql = string.Join(",\n", columnDefinitions);
                return $"CREATE TABLE {tableName} (\n{columnsSql}\n);";
            }

            private static string GenerateColumnDefinition(PropertyInfo property)
            {
                var columnDefinition = $"{property.Name} {GetSqlDataType(property.PropertyType)}";

                if (property.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                {
                    columnDefinition += " PRIMARY KEY";
                }

                if (property.GetCustomAttribute<UniqueAttribute>() != null)
                {
                    columnDefinition += " UNIQUE";
                }

                var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttribute != null)
                {
                    columnDefinition += $" DEFAULT {FormatDefaultValue(defaultValueAttribute.Value)}";
                }

                var checkConstraintAttribute = property.GetCustomAttribute<CheckConstraintAttribute>();
                if (checkConstraintAttribute != null)
                {
                    columnDefinition += $" CHECK ({checkConstraintAttribute.Condition})";
                }

                return columnDefinition;
            }

            private static string GetSqlDataType(Type type)
            {
                return type switch
                {
                    var t when t == typeof(int) => "INT",
                    var t when t == typeof(string) => "NVARCHAR(MAX)",
                    var t when t == typeof(DateTime) => "DATETIME",
                    _ => throw new NotSupportedException($"Type {type.Name} is not supported.")
                };
            }

            private static string FormatDefaultValue(object value)
            {
                return value is string || value is DateTime ? $"'{value}'" : value.ToString();
            }
    }

    public class DatabaseSchemaReader
    {
        private readonly string _connectionString;

        public DatabaseSchemaReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TableSchema> GetDatabaseSchema()
        {
            var tables = new List<TableSchema>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to fetch table and column details
                string query = @"
                    SELECT 
                        TABLE_NAME, COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT, 
                        IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
                    FROM INFORMATION_SCHEMA.COLUMNS";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    var tableSchemas = new Dictionary<string, TableSchema>();

                    while (reader.Read())
                    {
                        string tableName = reader["TABLE_NAME"].ToString();
                        string columnName = reader["COLUMN_NAME"].ToString();
                        string dataType = reader["DATA_TYPE"].ToString();
                        string defaultValue = reader["COLUMN_DEFAULT"]?.ToString();
                        bool isNullable = reader["IS_NULLABLE"].ToString() == "YES";

                        if (!tableSchemas.ContainsKey(tableName))
                        {
                            tableSchemas[tableName] = new TableSchema
                            {
                                TableName = tableName
                            };
                        }

                        tableSchemas[tableName].Columns.Add(new ColumnSchema
                        {
                            Name = columnName,
                            DataType = dataType,
                            DefaultValue = defaultValue,
                            IsPrimaryKey = false, // Primary key info will be fetched separately
                            IsForeignKey = false, // Foreign key info will be fetched separately
                            IsUnique = false,     // Unique constraint info will be fetched separately
                        });
                    }

                    tables.AddRange(tableSchemas.Values);
                }
            }

            return tables;
        }
    }

        public class SchemaComparer
        {
            public List<string> CompareSchemas(List<TableSchema> dbSchemas, List<TableSchema> modelSchemas)
            {
                var alterCommands = new List<string>();

                foreach (var modelSchema in modelSchemas)
                {
                    var dbSchema = dbSchemas.FirstOrDefault(t => t.TableName == modelSchema.TableName);

                    if (dbSchema == null)
                    {
                        // Table doesn't exist in the database, create it
                        alterCommands.Add(SchemaSynchronizer.GenerateCreateTableScript(modelSchema.GetType()));
                        continue;
                    }

                    foreach (var modelColumn in modelSchema.Columns)
                    {
                        var dbColumn = dbSchema.Columns.FirstOrDefault(c => c.Name == modelColumn.Name);

                        if (dbColumn == null)
                        {
                            // Column doesn't exist, add it
                            alterCommands.Add($"ALTER TABLE {modelSchema.TableName} ADD {modelColumn.Name} {modelColumn.DataType};");
                        }
                        else if (dbColumn.DataType != modelColumn.DataType ||
                                 dbColumn.DefaultValue != modelColumn.DefaultValue)
                        {
                            // Column exists but differs, modify it
                            alterCommands.Add($"ALTER TABLE {modelSchema.TableName} ALTER COLUMN {modelColumn.Name} {modelColumn.DataType};");
                        }
                    }

                    foreach (var dbColumn in dbSchema.Columns)
                    {
                        if (!modelSchema.Columns.Any(c => c.Name == dbColumn.Name))
                        {
                            // Column exists in the database but not in the model, drop it
                            alterCommands.Add($"ALTER TABLE {modelSchema.TableName} DROP COLUMN {dbColumn.Name};");
                        }
                    }
                }

                return alterCommands;
            }
    }

        public class SqlExecutor
        {
            private readonly string _connectionString;

            public SqlExecutor(string connectionString)
            {
                _connectionString = connectionString;
            }

            // Executes a single SQL command
            public void ExecuteCommand(string sqlCommand)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sqlCommand, connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine($"Executed: {sqlCommand}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing SQL: {sqlCommand}\n{ex.Message}");
                            throw;
                        }
                    }
                }
            }

            // Executes a batch of SQL commands within a transaction
            public void ExecuteCommands(IEnumerable<string> sqlCommands)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var sqlCommand in sqlCommands)
                            {
                                using (var command = new SqlCommand(sqlCommand, connection, transaction))
                                {
                                    command.ExecuteNonQuery();
                                    Console.WriteLine($"Executed: {sqlCommand}");
                                }
                            }

                            transaction.Commit();
                            Console.WriteLine("Transaction committed successfully.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction rolled back. Error: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }
    }




