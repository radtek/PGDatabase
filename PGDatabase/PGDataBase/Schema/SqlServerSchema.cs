#pragma warning disable CS1591
using PGCafe;
using PGCafe.Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    
    public class SQLServerTableSchema : ITableSchema {

        #region Property, Constructor

        public string Schema { get; private set; }

        public string Name { get; private set; }

        public string FullName => $"{Schema}.{Name}";

        public string PrimaryKeyName { get; internal set; }
        
        public List<SQLServerColumnSchema> Columns { get; private set; } = new List<SQLServerColumnSchema>();

        private SQLServerTableSchema(){ }
        
        public SQLServerTableSchema( string Schema, string Name ){
            this.Name = Name;
            this.Schema = Schema;
            this.PrimaryKeyName = PrimaryKeyName;
        } // public SQLServerTableSchema( string Schema, string Name )

        #endregion

        #region DDL
        
        public string DDL => CreateDDL();

        private string CreateDDL(){
            /// create table header
            var result = new StringBuilder()
                .AppendLine( $@"CREATE TABLE [{Schema}].[{Name}] (" );

            /// column description
            if ( Columns.Any() ){
                // create each part of column's DDL
                var columnTokens = Columns.Select( item => new {
                    Name = $"[{item.Name}]",
                    TypeString = item.DataType,
                    NullableString = item.Nullable ? "Null" : "Not Null",
                    Comment = !item.Comment.IsNullOrWhiteSpace() ? $"-- {item.Comment}" : null,
                } ).Evaluate();

                // calculate name and type and nullable's padding count.
                var namePadding = columnTokens.Max( item => item.Name.Length ) + 1;
                var typePadding = columnTokens.Max( item => item.TypeString.Length ) + 1;
                var nullablePadding = columnTokens.Max( item => item.NullableString.Length );
                    
                foreach ( var token in columnTokens ){
                    result.Append( "    " + token.Name.PadRight( namePadding ) + token.TypeString.PadRight( typePadding )
                        + token.NullableString.PadRight( nullablePadding ) + "," )
                        .AppendLine( token.Comment );
                } // foreach

            } // if

            /// PrimaryKey
            var primaryKeyColumns = Columns.Where( item => item.IsPrimaryKey );
            if ( primaryKeyColumns.Any() ){
                result.AppendLine( $"    CONSTRAINT [{PrimaryKeyName}] PRIMARY KEY CLUSTERED (" );
                    
                // create each part of column's DDL
                var columnTokens = primaryKeyColumns.Select( item => new {
                    Name = $"[{item.Name}]",
                } ).Evaluate();

                // calculate name and type and nullable's padding count.
                var namePadding = columnTokens.Max( item => item.Name.Length ) + 1;
                    
                var lastToken = columnTokens.Last();
                foreach ( var token in columnTokens ){
                    result.Append( "        " + token.Name.PadRight( namePadding ) + "ASC" );
                    if ( token != lastToken )
                        result.AppendLine( "," );
                    else result.AppendLine();
                } // foreach

                result.AppendLine( "    )" );
            } // if
                
            /// create table's footer
            result.AppendLine( ");" );
                
            return result.ToString();
        } // private string CreateDDL()

        #endregion
        
        #region Implements

        string ITableSchema.Name => Name;

        string ITableSchema.FullName => FullName;

        string ITableSchema.PrimaryKeyName => PrimaryKeyName;
        
        IEnumerable<IColumnSchema> ITableSchema.Columns => Columns;

        string ITableSchema.DDL => DDL;

        #endregion

    } // public class SQLServerTableSchema : ITableSchema

    
    public class SQLServerColumnSchema : IColumnSchema {

        #region Property
        
        public string Name { get; private set; }

        public bool Nullable { get; private set; }

        public string DataType { get; private set; }

        public Type DataTypeInDoNet { get; private set; }

        public string Comment { get; internal set; }

        public string DataTypeName { get; private set; }

        public int? CharacterLength { get; private set; }

        public int? NumericPrecision { get; private set; }

        public int? NumericScale { get; private set; }

        public bool IsPrimaryKey { get; internal set; }

        #endregion

        #region Constructor
        
        private SQLServerColumnSchema(){ }

        public SQLServerColumnSchema( string Name, bool Nullable, string DataType, Type DataTypeInDoNet, string DataTypeName ){
            this.Name = Name;
            this.Nullable = Nullable;
            this.DataType = DataType;
            this.DataTypeInDoNet = DataTypeInDoNet;
            this.DataTypeName = DataTypeName;
        } // public SQLServerColumnSchema( string Name, bool Nullable, string DataType, Type DataTypeInDoNet, string DataTypeName )
        
        #region Power creater

        internal static SQLServerColumnSchema PowerCreator( string Name, bool Nullable, string DataType,
            int? CharacterLength, int? NumericPrecision, int? NumericScale ){

            switch ( DataType ){
                case "text": return Text( Name, Nullable );
                case "bit": return Bit( Name, Nullable );
                case "tinyint": return Tinyint( Name, Nullable );
                case "smallint": return Smallint( Name, Nullable );
                case "int": return Int( Name, Nullable );
                case "bigint": return Bigint( Name, Nullable );
                case "float": return Float( Name, Nullable );
                case "date": return Date( Name, Nullable );
                case "datetime": return DateTime( Name, Nullable );
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    return PowerCreatorCharacter( Name, Nullable, DataType, CharacterLength );
                case "numeric":
                case "decimal":
                    return PowerCreatorNumber( Name, Nullable, DataType, NumericPrecision, NumericScale );
                default:
                    return new SQLServerColumnSchema( Name, Nullable, DataType, null, DataType ){
                        Comment = $"Not support [DataType:{DataType}], may has some error at DDL." };
            } // switch

        } // internal static SQLServerColumnSchema PowerCreator( string Name, bool Nullable, string DataType, int? CharacterLength ... )
        
        private static SQLServerColumnSchema PowerCreatorCharacter( string Name, bool Nullable, string DataType, int? CharacterLength ){
            if ( CharacterLength == null ) throw new NotSupportedException( $"Not support null CharacterLength with [DataType:{DataType}] at [ColumnName:{Name}]" );
            
            switch ( DataType ){
                case "char" when CharacterLength > 0 : return Char( Name, Nullable, CharacterLength.Value );
                case "nchar" when CharacterLength > 0: return NChar( Name, Nullable, CharacterLength.Value );
                case "varchar" when CharacterLength > 0: return VarChar( Name, Nullable, CharacterLength.Value );
                case "nvarchar" when CharacterLength > 0: return NVarChar( Name, Nullable, CharacterLength.Value );

                case "char" when CharacterLength == -1: return MaxChar( Name, Nullable );
                case "nchar"  when CharacterLength == -1: return MaxNChar( Name, Nullable );
                case "varchar"  when CharacterLength == -1: return MaxVarChar( Name, Nullable );
                case "nvarchar"  when CharacterLength == -1: return MaxNVarChar( Name, Nullable );
                default: throw new NotSupportedException( $"Not support [DataType:{DataType}] in Power character creator" );
            } // switch
        } // private static SQLServerColumnSchema PowerCreatorCharacter( string Name, bool Nullable, string DataType, int? CharacterLength )
        
        private static SQLServerColumnSchema PowerCreatorNumber( string Name, bool Nullable, string DataType, int? NumericPrecision, int? NumericScale ){
            if ( NumericPrecision == null ) throw new NotSupportedException( $"Not support null NumericPrecision with [DataType:{DataType}] at [ColumnName:{Name}]" );
            if ( NumericScale == null ) throw new NotSupportedException( $"Not support null NumericScale with [DataType:{DataType}] at [ColumnName:{Name}]" );
            
            switch ( DataType ){
                case "numeric": return Numeric( Name, Nullable, NumericPrecision.Value, NumericScale.Value );
                case "decimal": return Decimal( Name, Nullable, NumericPrecision.Value, NumericScale.Value );
                default: throw new NotSupportedException( $"Not support [DataType:{DataType}] in Power number creator" );
            } // switch
        } // private static SQLServerColumnSchema PowerCreatorNumber( string Name, bool Nullable, string DataType, ... )

        #endregion

        #region Simple type: text, bit, int, float, date, datetime

        public static SQLServerColumnSchema Text( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "text", typeof( string ), "text" );

        public static SQLServerColumnSchema Bit( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "bit", typeof( bool ), "bit" );

        public static SQLServerColumnSchema Tinyint( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "tinyint", typeof( byte ), "tinyint" );

        public static SQLServerColumnSchema Smallint( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "smallint", typeof( short ), "smallint" );

        public static SQLServerColumnSchema Int( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "int", typeof( int ), "int" );

        public static SQLServerColumnSchema Bigint( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "bigint", typeof( long ), "bigint" );

        public static SQLServerColumnSchema Float( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "float", typeof( float ), "float" );

        public static SQLServerColumnSchema Date( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "date", typeof( DateTime ), "date" );

        public static SQLServerColumnSchema DateTime( string Name, bool Nullable ) =>
            new SQLServerColumnSchema( Name, Nullable, "datetime", typeof( DateTime ), "datetime" );

        #endregion

        #region char, nchar, varchar, nvarchar

        private static SQLServerColumnSchema Charaeter( string Name, bool Nullable, string DataTypeName, int Length ){
            if ( Length <= 0 ) throw new ArgumentException( $"{nameof( Length )} need a positive integer" );
            
            var DataType = $"{DataTypeName}({Length})";

            return new SQLServerColumnSchema( Name, Nullable, DataType, typeof( string ), DataTypeName ){
                CharacterLength = Length,
            };

        } // private static SQLServerColumnSchema Charaeter( string Name, bool Nullable, string DataTypeName, int Length )

        private static SQLServerColumnSchema MaxCharaeter( string Name, bool Nullable, string DataTypeName ){
            
            var DataType = $"{DataTypeName}(MAX)";

            return new SQLServerColumnSchema( Name, Nullable, DataType, typeof( string ), DataTypeName ){
                CharacterLength = -1,
            };

        } // private static SQLServerColumnSchema MaxCharaeter( string Name, bool Nullable, string DataTypeName )


        public static SQLServerColumnSchema Char( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "char", Length );
        public static SQLServerColumnSchema NChar( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "nchar", Length );
        public static SQLServerColumnSchema VarChar( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "varchar", Length );
        public static SQLServerColumnSchema NVarChar( string Name, bool Nullable, int Length ) => Charaeter( Name, Nullable, "nvarchar", Length );
        
        public static SQLServerColumnSchema MaxChar( string Name, bool Nullable ) => MaxCharaeter( Name, Nullable, "char" );
        public static SQLServerColumnSchema MaxNChar( string Name, bool Nullable ) => MaxCharaeter( Name, Nullable, "nchar" );
        public static SQLServerColumnSchema MaxVarChar( string Name, bool Nullable ) => MaxCharaeter( Name, Nullable, "varchar" );
        public static SQLServerColumnSchema MaxNVarChar( string Name, bool Nullable ) => MaxCharaeter( Name, Nullable, "nvarchar" );
        
        #endregion

        #region numeric, decimal
        
        public static SQLServerColumnSchema Numeric( string Name, bool Nullable, int Precision, int Scale ){
            if ( Precision <= 0 ) throw new ArgumentException( $"{nameof( Precision )} need a positive integer" );
            if ( Scale < 0 ) throw new ArgumentException( $"{nameof( Scale )} need zero or a positive integer" );
            
            var DataType = $"numeric({Precision},{Scale})";

            return new SQLServerColumnSchema( Name, Nullable, DataType, typeof( double ), "numeric" ){
                NumericPrecision = Precision,
                NumericScale = Scale,
            };
        } // public static SQLServerColumnSchema Numeric( string Name, bool Nullable, int Precision, int Scale )

        public static SQLServerColumnSchema Decimal( string Name, bool Nullable, int Precision, int Scale ){
            if ( Precision <= 0 ) throw new ArgumentException( $"{nameof( Precision )} need a positive integer" );
            if ( Scale < 0 ) throw new ArgumentException( $"{nameof( Scale )} need zero or a positive integer" );
            
            var DataType = $"decimal({Precision},{Scale})";

            return new SQLServerColumnSchema( Name, Nullable, DataType, typeof( decimal ), "decimal" ){
                NumericPrecision = Precision,
                NumericScale = Scale,
            };
        } // public static SQLServerColumnSchema Numeric( string Name, bool Nullable, int Precision, int Scale )

        #endregion

        #endregion

    } // public class SQLServerColumnSchema : IColumnSchema

} // namespace PGLibrary.Database
