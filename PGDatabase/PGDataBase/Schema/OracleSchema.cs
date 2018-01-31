#pragma warning disable CS1591
using PGCafe;
using PGCafe.Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    
    public class OracleTableSchema : ITableSchema {

        #region Constructor & Property

        public string Owner { get; private set; }

        public string Name { get; private set; }

        public string FullName => $"{Owner}.{Name}";

        public string PrimaryKeyName { get; internal set; }
        
        public List<OracleColumnSchema> Columns { get; private set; } = new List<OracleColumnSchema>();

        public OracleTableSchema( string Owner, string Name ){
            this.Name = Name;
            this.Owner = Owner;
        } // public OracleTableSchema( string Owner, string Name )

        #endregion

        #region DDL
        
        public string DDL => CreateDDL();

        private string CreateDDL(){

            /// create table header
            var result = new StringBuilder()
                .AppendLine( $@"CREATE TABLE ""{Owner}"".""{Name}"" (" );

            /// columns
            if ( Columns.Any() ){
                // create each part of column's DDL
                var columnTokens = Columns.Select( item => new {
                    Name = $"\"{item.Name}\"",
                    TypeString = item.DataType,
                    Comment = !item.Comment.IsNullOrWhiteSpace() ? $"-- {item.Comment}" : null,
                } ).Evaluate();

                // calculate name and type padding count.
                var namePadding = columnTokens.Max( item => item.Name.Length ) + 1;
                var typePadding = columnTokens.Max( item => item.TypeString.Length );

                foreach ( var token in columnTokens.SkipLast( 1 ) ){
                    result.Append( "    " + token.Name.PadRight( namePadding ) + token.TypeString.PadRight( typePadding ) + "," )
                        .AppendLine( token.Comment );
                } // foreach
                
                var lastToken = columnTokens.Last();
                result.Append( "    " + lastToken.Name.PadRight( namePadding ) + lastToken.TypeString.PadRight( typePadding ) )
                    .AppendLine( " " + lastToken.Comment );

            } // if
                
            /// create table's footer
            result.AppendLine( ");" );

            /// PrimaryKey
            var primaryKeyColumns = Columns.Where( item => item.IsPrimaryKey );
            if ( primaryKeyColumns.Any() ){
                // create each part of column's DDL
                var columnTokens = primaryKeyColumns.Select( item => $"\"{item.Name}\"" ).Evaluate();

                var columnString = string.Join( ", ", columnTokens );
                
                result.AppendLine();
                result.AppendLine( $"CREATE UNIQUE INDEX \"{Owner}\".\"{PrimaryKeyName}\" ON \"{Owner}\".\"{Name}\" ( {columnString} );" );
                result.AppendLine( $"ALTER TABLE \"{Owner}\".\"{Name}\" ADD CONSTRAINT \"{PrimaryKeyName}\" PRIMARY KEY ( {columnString} );" );
            } // if

            /// nullable DDL
            var notNullableColumns = Columns.Where( item => !item.Nullable ).Evaluate();
            if ( notNullableColumns.Any() ){
                var columnNames = notNullableColumns.Select( item => $"\"{item.Name}\"" ).Evaluate();
                var padding = columnNames.Max( item => item.Length ) + 1;

                result.AppendLine();
                foreach ( var column in columnNames ){
                    result.AppendLine( $"ALTER TABLE \"{Owner}\".\"{Name}\" MODIFY ({column.PadRight( padding )} NOT NULL ENABLE);" );
                } // foreach
            } // if
                
            return result.ToString();
        } // private string CreateDDL()
        
        #endregion
        
        #region Implements

        string ITableSchema.Name => Name;

        string ITableSchema.FullName => FullName;

        string ITableSchema.PrimaryKeyName => PrimaryKeyName;
        
        IEnumerable<IColumnSchema> ITableSchema.Columns => Columns;

        string ITableSchema.DDL => CreateDDL();

        #endregion

    } // public class OracleTableSchema : BaseTableSchema<OracleColumnSchema>

    public class OracleColumnSchema : IColumnSchema {

        #region Property
        
        public string Name { get; private set; }

        public bool Nullable { get; private set; }

        public string DataType { get; private set; }

        public Type DataTypeInDoNet { get; private set; }

        public string Comment { get; internal set; }

        public string DataTypeName { get; private set; }

        public int? CharLength { get; private set; }

        public int? DataPrecision { get; private set; }

        public int? DataScale { get; private set; }

        public bool IsPrimaryKey { get; internal set; }

        #endregion

        #region Constructor
        
        public OracleColumnSchema( string Name, bool Nullable, string DataType, Type DataTypeInDoNet, string DataTypeName ){
            this.Name = Name;
            this.Nullable = Nullable;
            this.DataType = DataType;
            this.DataTypeInDoNet = DataTypeInDoNet;
            this.DataTypeName = DataTypeName;
        } // public OracleColumnSchema( string Name, bool Nullable, string DataType, Type DataTypeInDoNet, string DataTypeName )
        
        #region Power creater

        internal static OracleColumnSchema PowerCreator( string Name, bool Nullable, string DataType,
            int? CharLength, int? DataPrecision, int? DataScale ){

            switch ( DataType ){
                case "DATE": return Date( Name, Nullable );
                case "CHAR":
                case "NCHAR":
                case "VARCHAR2":
                case "NVARCHAR2":
                    return PowerCreatorCharacter( Name, Nullable, DataType, CharLength );
                case "NUMBER":
                    return NUMBER( Name, Nullable, DataPrecision, DataScale );
                case "FLOAT":
                    if ( DataPrecision == null ) throw new NotSupportedException( $"Not support null DataPrecision with [DataType:{DataType}] at [ColumnName:{Name}]" );

                    return FLOAT( Name, Nullable, DataPrecision.Value );
                default: 
                    return new OracleColumnSchema( Name, Nullable, DataType, null, DataType ){
                        Comment = $"Not support [DataType:{DataType}], may has some error at DDL." };
            } // switch

        } // internal static OracleColumnSchema PowerCreator( string Name, bool Nullable, string DataType, int? CharLength ... )
        
        private static OracleColumnSchema PowerCreatorCharacter( string Name, bool Nullable, string DataType, int? CharLength ){
            if ( CharLength == null ) throw new NotSupportedException( $"Not support null CharLength with [DataType:{DataType}] at [ColumnName:{Name}]" );
            
            switch ( DataType ){
                case "CHAR": return Char( Name, Nullable, CharLength.Value );
                case "NCHAR": return NChar( Name, Nullable, CharLength.Value );
                case "VARCHAR2": return VarChar( Name, Nullable, CharLength.Value );
                case "NVARCHAR2" : return NVarChar( Name, Nullable, CharLength.Value );
                    
                default: throw new NotSupportedException( $"Not support [DataType:{DataType}] in Power character creator" );
            } // switch
        } // private static OracleColumnSchema PowerCreatorCharacter( string Name, bool Nullable, string DataType, int? CharLength )
        
        #endregion

        #region Simple type: DATE

        public static OracleColumnSchema Date( string Name, bool Nullable ) =>
            new OracleColumnSchema( Name, Nullable, "DATE", typeof( DateTime ), "DATE" );

        #endregion

        #region CHAR, NCHAR, VARCHAR2, NVARCHAR2

        private static OracleColumnSchema Charaeter( string Name, bool Nullable, string DataTypeName, int Length ){
            if ( Length < 0 ) throw new ArgumentException( $"{nameof( Length )} need a positive integer" );
            
            var DataType = $"{DataTypeName}({Length} BYTE)";
            var DataTypeInDoNet = typeof( string );

            return new OracleColumnSchema( Name, Nullable, DataType, DataTypeInDoNet, DataTypeName ){
                CharLength = Length,
            };

        } // private static OracleColumnSchema Charaeter( string Name, bool Nullable, string DataTypeName, int Length )

        private static OracleColumnSchema NCharaeter( string Name, bool Nullable, string DataTypeName, int Length ){
            if ( Length < 0 ) throw new ArgumentException( $"{nameof( Length )} need a positive integer" );
            
            var DataType = $"{DataTypeName}({Length})";
            var DataTypeInDoNet = typeof( string );

            return new OracleColumnSchema( Name, Nullable, DataType, DataTypeInDoNet, DataTypeName ){
                CharLength = Length,
            };

        } // private static OracleColumnSchema NCharaeter( string Name, bool Nullable, string DataTypeName, int Length )
        
        public static OracleColumnSchema Char( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "CHAR", Length );
        public static OracleColumnSchema NChar( string Name, bool Nullable, int Length ) => NCharaeter( Name, Nullable, "NCHAR", Length );
        public static OracleColumnSchema VarChar( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "VARCHAR", Length );
        public static OracleColumnSchema NVarChar( string Name, bool Nullable, int Length ) => NCharaeter( Name, Nullable, "NVARCHAR", Length );
        
        #endregion

        #region NUMBER, FLOAT
        
        public static OracleColumnSchema NUMBER( string Name, bool Nullable, int? Precision, int? Scale ){
            if ( Precision <= 0 ) throw new ArgumentException( $"{nameof( Precision )} need null or a positive integer" );
            if ( Scale < 0 ) throw new ArgumentException( $"{nameof( Scale )} need zero or a positive integer" );
            
            string DataType;
            if ( Precision == null && Scale == null )
                DataType = "NUMBER";
            else DataType = $"NUMBER({Precision.ToString() ?? "*"},{Scale.ToString() ?? "*"})";

            return new OracleColumnSchema( Name, Nullable, DataType, typeof( decimal ), "NUMBER" ){
                DataPrecision = Precision,
                DataScale = Scale,
            };
        } // public static OracleColumnSchema NUMBER( string Name, bool Nullable, int? Precision, int? Scale )

        public static OracleColumnSchema FLOAT( string Name, bool Nullable, int Precision ){
            if ( Precision <= 0 ) throw new ArgumentException( $"{nameof( Precision )} need a positive integer" );
            
            var DataType = $"FLOAT({Precision})";

            return new OracleColumnSchema( Name, Nullable, DataType, typeof( double ), "FLOAT" ){
                DataPrecision = Precision,
            };
        } // public static OracleColumnSchema FLOAT( string Name, bool Nullable, int Precision )

        #endregion

        #endregion

    } // public class OracleColumnSchema : BaseColumnSchema

} // namespace PGLibrary.Database
