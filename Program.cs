using CustomORM.CustomORM;
using CustomORM;
using System;
using System.Collections.Generic;

   
        [Table("Roles")]
        public class Role : BaseModel
        {
            [PrimaryKey]
            public int Id { get; set; }

            [Unique]
            public string Name { get; set; }

            [DefaultValue("CURRENT_TIMESTAMP")]
            public DateTime CreatedAt { get; set; }
    }


        [Table("Users")]
        public class User : BaseModel
        {
            [PrimaryKey]
            public int Id { get; set; }

            [Unique]
            public string Username { get; set; }

            [ForeignKey("Roles", "Id")]
            public int RoleId { get; set; }

            [DefaultValue("CURRENT_TIMESTAMP")]
            public DateTime CreatedAt { get; set; }
}
    




    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=COGNINE-L105;Database=bb2;Trusted_Connection=True;Trust Server Certificate=True";

            // Step 1: Read existing database schema
            var dbReader = new DatabaseSchemaReader(connectionString);
            var dbSchemas = dbReader.GetDatabaseSchema();

            // Step 2: Define model types (extract from classes with attributes)
            var modelTypes = new List<Type>
            {
                typeof(Role),
                typeof(User)
            };

            // Step 3: Compare schemas and generate ALTER commands
            var schemaComparer = new SchemaComparer();
            var alterCommands = schemaComparer.CompareSchemas(dbSchemas, modelTypes);

            // Step 4: Execute the ALTER commands
            var executor = new SqlExecutor(connectionString);
            executor.ExecuteCommands(alterCommands);
        }
    }

