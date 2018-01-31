#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {

    public interface ITableSchema {

        string Name { get; }
        
        string FullName { get; }

        IEnumerable<IColumnSchema> Columns { get; }

        string PrimaryKeyName { get; }

        string DDL { get; }

    } // public interface ITableSchema

    public interface IColumnSchema {

        string Name { get; }

        bool Nullable { get; }

        string DataType { get; }

        Type DataTypeInDoNet { get; }

        string Comment { get; }

        bool IsPrimaryKey { get; }

    } // public interface IColumnSchema
    
} // namespace PGLibrary.Database
