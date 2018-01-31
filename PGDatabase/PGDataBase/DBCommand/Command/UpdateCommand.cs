using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Object to generate Update Script. </summary>
    public class UpdateCommand : IDBCommand {

        #region Property & Constructor

        /// <summary> Table object </summary>
        private DBTable Table;
        
        /// <summary> Set Clause </summary>
        private IClause SetClause = null;
        
        /// <summary> Where Clause </summary>
        private IClause WhereClause = null;

        /// <summary> Determine how to quote the keyword. </summary>
        private EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> Create UpdateCommand object. </summary>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public UpdateCommand( EQuoteType QuoteType = EQuoteType.Auto ) {
            this.QuoteType = QuoteType;
        }  // public UpdateCommand( EQuoteType QuoteType = EQuoteType.Auto )

        /// <summary> Create UpdateCommand object and generate script from source with Table and Non Primary Key of Set Clause and Primary Key of Where Clause. </summary>
        /// <param name="source">get Table and Values from source.</param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        /// <exception cref="Exception">No any primary key of type or no any non-primary key of type.</exception>
        public UpdateCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )
            : this( QuoteType ) {
            var primariKeys = source.PrimaryKey().Evaluate();
            if ( !primariKeys.Any() ) throw new Exception( "No any primary key column of source" );
            
            var nonPrimariKeys = source.NonPrimaryKey( EConvertType.ToDB ).Evaluate();
            if ( !nonPrimariKeys.Any() ) throw new Exception( "No any non primary key column of source" );

            this.UpdateTo( source.GetType() ).Set( nonPrimariKeys ).WhereAnd( primariKeys );
        }  // public UpdateCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )

        #endregion

        #region UpdateTo

        /// <summary> Specify Update to which table </summary>
        /// <param name="Table"> table to Update </param>
        public UpdateCommand UpdateTo( DBTable Table ) {
            this.Table = Table;
            return this;
        } // public UpdateCommand UpdateTo( DBTable Table )
        
        /// <summary>
        /// Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="DBTableType"> Type to get Table name </param>
        public UpdateCommand UpdateTo( Type DBTableType ) {
            this.Table = IDatabaseTableExtension.Table( DBTableType );
            return this;
        } // public UpdateCommand UpdateTo( Type DBTableType )
        
        /// <summary>
        /// Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public UpdateCommand UpdateTo<T>()
            where T : IDatabaseTable {
            return this.UpdateTo( typeof ( T ) );
        } // public UpdateCommand UpdateTo<T>()
        
        /// <summary>
        /// Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="instance"> instance to get table name </param>
        public UpdateCommand UpdateTo( IDatabaseTable instance ) {
            return this.UpdateTo( instance.GetType() );
        } // public UpdateCommand UpdateTo( IDatabaseTable instance )

        #endregion

        #region Set
        
        /// <summary> Use speficy Set script and Parameter list to generate Set clause. </summary>
        /// <param name="SetScript">Set script of Set clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in SetScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public UpdateCommand SetScript( string SetScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.SetClause = new SetScript( SetScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public UpdateCommand SetScript( string SetScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy Set script and Parameter list to generate Set clause. </summary>
        /// <param name="SetScript">Set script of Set clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in SetScript.</param>
        public UpdateCommand SetScript( string SetScript, params DBParameter[] Parameters ) {
            return this.SetScript( SetScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public UpdateCommand SetScript( string SetScript, params DBParameter[] Parameters )
        
        /// <summary> Generate Set script by DBParameters. </summary>
        /// <param name="Parameters">Parameter list to generate to Set script.</param>
        public UpdateCommand Set( IEnumerable<DBParameter> Parameters ) {
            this.SetClause = new SetParameters( Parameters );
            return this;
        } // public UpdateCommand Set( IEnumerable<DBParameter> Parameters )
        
        /// <summary> Generate Set script by DBParameters. </summary>
        /// <param name="Parameters">Parameter list to generate to Set script.</param>
        public UpdateCommand Set( params DBParameter[] Parameters )
            => this.Set( Parameters.CastTo<IEnumerable<DBParameter>>() );
        
        /// <summary> Generate Set script by Column and value. </summary>
        /// <param name="Column">Column object.</param>
        /// <param name="value">value of column.</param>
        public UpdateCommand Set( DBColumn Column, object value )
            => this.Set( new DBParameter( Column, value ) );

        #endregion

        #region UpdateToSet
        
        /// <summary>
        /// Generate Set script by DBParameters and Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Parameters">Parameter list to generate to Set script.</param>
        public UpdateCommand UpdateToSet<T>( IEnumerable<DBParameter> Parameters )
            where T : IDatabaseTable {
            this.UpdateTo( typeof ( T ) ).SetClause = new SetParameters( Parameters );
            return this;
        } // public UpdateCommand UpdateToSet<T>( IEnumerable<DBParameter> Parameters )
        
        /// <summary>
        /// Generate Set script by DBParameters and Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Parameters">Parameter list to generate to Set script.</param>
        public UpdateCommand UpdateToSet<T>( params DBParameter[] Parameters )
            where T : IDatabaseTable
            => this.UpdateTo( typeof ( T ) ).Set( Parameters.CastTo<IEnumerable<DBParameter>>() );
        
        /// <summary>
        /// Generate Set script by Column and value and Specify Update to which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="Column">Column object.</param>
        /// <param name="value">value of column.</param>
        public UpdateCommand UpdateToSet<T>( DBColumn Column, object value )
            where T : IDatabaseTable
            => this.UpdateTo( typeof ( T ) ).Set( new DBParameter( Column, value ) );

        #endregion

        #region Where

        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public UpdateCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.WhereClause = new WhereScript( whereScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public UpdateCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        public UpdateCommand WhereScript( string whereScript, params DBParameter[] Parameters ) {
            return this.WhereScript( whereScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public UpdateCommand WhereScript( string whereScript, params DBParameter[] Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public UpdateCommand WhereAnd( IEnumerable<DBParameter> Parameters ) {
            this.WhereClause = new WhereParameters( Parameters );
            return this;
        } // public UpdateCommand Where( IEnumerable<DBParameter> Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public UpdateCommand WhereAnd( params DBParameter[] Parameters )
            => this.WhereAnd( Parameters.CastTo<IEnumerable<DBParameter>>() );
        
        /// <summary> Generate where script by one Column and value. </summary>
        /// <param name="Column">Column of value.</param>
        /// <param name="value">value of column.</param>
        public UpdateCommand WhereOne( DBColumn Column, object value )
            => this.WhereAnd( new DBParameter( Column, value ) );

        #endregion

        #region ToScript, Clone

        /// <summary> Generate the Update script. </summary>
        /// <param name="Provider">Database provider to get personal setting.</param>
        /// <param name="AddCommandSpliter">Add command spliter at the end or not.</param>
        public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false ) {
            // generate set clause
            string setString = null;
            if ( this.SetClause?.AnyClause() ?? false )
                setString = this.SetClause.ToScript( Provider, this.QuoteType );
            
            // generate where clause
            string whereString = null;
            if ( this.WhereClause?.AnyClause() ?? false )
                whereString = this.WhereClause.ToScript( Provider, this.QuoteType );

            // generate whold command.
            var result = $"Update {this.Table.ToScript( Provider, QuoteType )}{setString}{whereString}";
            if ( AddCommandSpliter )
                result += Provider.CommandSpliter;

            return result;
        } // public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false )
        
        /// <summary> Clone this object. </summary>
        public UpdateCommand Clone() {
            var result = (UpdateCommand)this.MemberwiseClone();
            result.SetClause = result.SetClause.Clone();
            result.WhereClause = result.WhereClause.Clone();
            return result;
        } // public UpdateCommand Clone()

        #endregion

    } // public class UpdateCommand : IDBCommand
    
} // namespace PGLibrary.Database
