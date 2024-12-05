using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM
{
   
        // Attribute for specifying table name
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class TableAttribute : Attribute
        {
            public string Name { get; }
            public TableAttribute(string name) => Name = name;
        }

        // Attribute for specifying primary key
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class PrimaryKeyAttribute : Attribute { }

        // Attribute for specifying foreign key
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class ForeignKeyAttribute : Attribute
        {
            public string ReferencedTable { get; }
            public string ReferencedColumn { get; }

            public ForeignKeyAttribute(string referencedTable, string referencedColumn)
            {
                ReferencedTable = referencedTable;
                ReferencedColumn = referencedColumn;
            }
        }

        // Attribute for unique constraints
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class UniqueAttribute : Attribute { }

        // Attribute for default value
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class DefaultValueAttribute : Attribute
        {
            public object Value { get; }
            public DefaultValueAttribute(object value) => Value = value;
        }

        // Attribute for indexed columns
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class IndexedAttribute : Attribute { }

        // Attribute for check constraints
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class CheckConstraintAttribute : Attribute
        {
            public string Condition { get; }
            public CheckConstraintAttribute(string condition) => Condition = condition;
        }
    

}
