using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM
{
    using System;

    namespace CustomORM
    {
        public abstract class BaseModel
        {
            // Method for validation (to be expanded later)
            public virtual void Validate()
            {
                // Basic implementation to ensure the object is ready to be saved
            }

            // Method for mapping model properties to database columns
            public virtual string GetTableName()
            {
                var tableAttribute = GetType().GetCustomAttributes(typeof(TableAttribute), false);
                if (tableAttribute.Length > 0)
                {
                    return ((TableAttribute)tableAttribute[0]).Name;
                }
                return GetType().Name;
            }
        }
    }

}
