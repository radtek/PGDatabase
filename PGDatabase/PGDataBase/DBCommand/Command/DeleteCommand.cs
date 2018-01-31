using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Object to generate Delete Script. </summary>
    public class DeleteCommand : IDBCommand {

        #region Property & Constructor

        /// <summary> Table object </summary>
        private DBTable Table;
        
        /// <summary> Where Clause </summary>
        private IClause WhereClause = null;

        /// <summary> Determine how to quote the keyword. </summary>
        private EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> Create DeleteCommand object. </summary>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public DeleteCommand( EQuoteType QuoteType = EQuoteType.Auto ) {
            this.QuoteType = QuoteType;
        }  // public DeleteCommand( EQuoteType QuoteType = EQuoteType.Auto )

        /// <summary> Create DeleteCommand object and generate script from source with Table and Primary Key. </summary>
        /// <param name="source">get Table and Values from source.</param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        /// <exception cref="Exception">No any primary key of type.</exception>
        public DeleteCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )
            : this( QuoteType ) {
            var primariKeys = source.PrimaryKey();
            if ( !primariKeys.Any() ) throw new Exception( "No any primary key of source" );

            this.DeleteFrom( source.GetType() ).WhereAnd( primariKeys );
        }  // public DeleteCommand( IDatabaseTable source, EQuoteType QuoteType = EQuoteType.Auto )

        #endregion

        #region DeleteFrom

        /// <summary> Specify delete from which table </summary>
        /// <param name="Table"> table to delete </param>
        public DeleteCommand DeleteFrom( DBTable Table ) {
            this.Table = Table;
            return this;
        } // public DeleteCommand DeleteFrom( DBTable Table )
        
        /// <summary>
        /// Specify delete from which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="DBTableType"> Type to get Table name </param>
        public DeleteCommand DeleteFrom( Type DBTableType ) {
            this.Table = IDatabaseTableExtension.Table( DBTableType );
            return this;
        } // public DeleteCommand DeleteFrom( Type DBTableType )
        
        /// <summary>
        /// Specify delete from which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public DeleteCommand DeleteFrom<T>()
            where T : IDatabaseTable {
            return this.DeleteFrom( typeof ( T ) );
        } // public DeleteCommand DeleteFrom<T>()

        #endregion

        #region Where
        
        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public DeleteCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.WhereClause = new WhereScript( whereScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public DeleteCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        public DeleteCommand WhereScript( string whereScript, params DBParameter[] Parameters ) {
            return this.WhereScript( whereScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public DeleteCommand WhereScript( string whereScript, params DBParameter[] Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public DeleteCommand WhereAnd( IEnumerable<DBParameter> Parameters ) {
            this.WhereClause = new WhereParameters( Parameters );
            return this;
        } // public DeleteCommand WhereAnd( IEnumerable<DBParameter> Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public DeleteCommand WhereAnd( params DBParameter[] Parameters )
            => this.WhereAnd( Parameters.CastTo<IEnumerable<DBParameter>>() );
        
        /// <summary> Generate where script by one Column and value. </summary>
        /// <param name="Column">Column of value.</param>
        /// <param name="value">value of column.</param>
        public DeleteCommand WhereOne( DBColumn Column, object value )
            => this.WhereAnd( new DBParameter( Column, value ) );

        #endregion

        #region ToScript, Clone

        /// <summary> Generate the Delete script. </summary>
        /// <param name="Provider">Database provider to get personal setting.</param>
        /// <param name="AddCommandSpliter">Add command spliter at the end or not.</param>
        public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false ) {
            // generate where clause
            string whereString = null;
            if ( this.WhereClause?.AnyClause() ?? false )
                whereString = this.WhereClause.ToScript( Provider, this.QuoteType );

            // generate whold command.
            var result = $"Delete From {this.Table.ToScript( Provider, QuoteType )}{whereString}";
            if ( AddCommandSpliter )
                result += Provider.CommandSpliter;

            return result;
        } // public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false )
        
        /// <summary> Clone this object. </summary>
        public DeleteCommand Clone() {
            var result = (DeleteCommand)this.MemberwiseClone();
            result.WhereClause = result.WhereClause.Clone();
            return result;
        } // public DeleteCommand Clone()

        #endregion

    } // public class DeleteCommand : IDBCommand
    
} // namespace PGLibrary.Database
