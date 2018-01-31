using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Object to generate Select Script. </summary>
    public class SelectCommand : IDBCommand {

        #region Property & Constructor
        
        /// <summary> Table object </summary>
        private DBTable Table;
        
        /// <summary> Columns Clause </summary>
        private IClause SelectClause = null;
        
        /// <summary> Where Clause </summary>
        private IClause WhereClause = null;
        
        /// <summary> OrderBy Clause </summary>
        private IClause OrderByClause = null;

        /// <summary> Determine how to quote the keyword. </summary>
        private EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> Create SelectCommand object. </summary>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public SelectCommand( EQuoteType QuoteType = EQuoteType.Auto ) {
            this.QuoteType = QuoteType;
        }  // public SelectCommand( EQuoteType QuoteType = EQuoteType.Auto )

        #endregion

        #region Select
        
        /// <summary> Use speficy Select script and Parameter list to generate Select clause. </summary>
        /// <param name="SelectScript">Select script of Select clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in SelectScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SelectCommand SelectScript( string SelectScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.SelectClause = new SelectScript( SelectScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public SelectCommand SelectScript( string SelectScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy Select script and Parameter list to generate Select clause. </summary>
        /// <param name="SelectScript">Select script of Select clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in SelectScript.</param>
        public SelectCommand SelectScript( string SelectScript, params DBParameter[] Parameters ) {
            return this.SelectScript( SelectScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public SelectCommand SelectScript( string SelectScript, params DBParameter[] Parameters )
        
        /// <summary> Generate Select script by DBColumn. </summary>
        /// <param name="Columns">Column list to generate to Select script.</param>
        public SelectCommand Select( IEnumerable<DBColumn> Columns ) {
            this.SelectClause = new SelectColumns( Columns );
            return this;
        } // public SelectCommand Select( IEnumerable<DBColumn> Columns )
        
        /// <summary> Generate Select script by DBColumns. </summary>
        /// <param name="Columns">Column list to generate to Select script.</param>
        public SelectCommand Select( params DBColumn[] Columns )
            => this.Select( Columns.CastTo<IEnumerable<DBColumn>>() );
        
        /// <summary> Generate Select script by :Select *". </summary>
        public SelectCommand SelectAll() {
            this.SelectClause = new SelectScript( "*" );
            return this;
        } // public SelectCommand SelectAll()

        #endregion

        #region From

        /// <summary> Specify Select from which table </summary>
        /// <param name="Table"> table to Select </param>
        public SelectCommand From( DBTable Table ) {
            this.Table = Table;
            return this;
        } // public SelectCommand From( DBTable Table )
        
        /// <summary>
        /// Specify Select from which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <param name="DBTableType"> Type to get Table name </param>
        public SelectCommand From( Type DBTableType ) {
            this.Table = IDatabaseTableExtension.Table( DBTableType );
            return this;
        } // public SelectCommand From( Type DBTableType )
        
        /// <summary>
        /// Specify Select from which table by Type of IDatabaseTable
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public SelectCommand From<T>()
            where T : IDatabaseTable {
            return this.From( typeof ( T ) );
        } // public SelectCommand From<T>()

        #endregion

        #region SelectFrom
        
        /// <summary>
        /// Generate Select script by DBColumns and specify Select from which table by Type of IDatabaseTable.
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Columns">Column list to generate to Select script.</param>
        public SelectCommand SelectFrom<T>( IEnumerable<DBColumn> Columns )
            where T : IDatabaseTable {
            this.From( typeof ( T ) ).SelectClause = new SelectColumns( Columns );
            return this;
        } // public SelectCommand Select( IEnumerable<DBColumn> Columns )
        
        /// <summary>
        /// Generate Select script by DBColumns and specify Select from which table by Type of IDatabaseTable.
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name.
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        /// <param name="Columns">Column list to generate to Select script.</param>
        public SelectCommand SelectFrom<T>( params DBColumn[] Columns )
            where T : IDatabaseTable
            => this.From( typeof ( T ) ).Select( Columns.CastTo<IEnumerable<DBColumn>>() );
        
        /// <summary>
        /// Generate Select script by :Select *". and specify Select from which table by Type of IDatabaseTable.
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name. 
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public SelectCommand SelectAllFrom<T>() 
            where T : IDatabaseTable =>
            this.From( typeof ( T ) ).SelectAll();
        
        /// <summary>
        /// Generate Select script by :Select *". and specify Select from which table by Type of IDatabaseTable.
        /// * find DBTableAttribute to get table name.
        /// * if no DBTableAttrubite, use class name to be table name. 
        /// </summary>
        /// <typeparam name="T">Type to get table name.</typeparam>
        public SelectCommand SelectFromScript<T>( string Script )
            where T : IDatabaseTable =>
            this.From( typeof ( T ) ).SelectAll();
        
        /// <summary> Generate Select script by script and Parameter list from which table by Type of IDatabaseTable. </summary>
        /// <param name="SelectScript">Select script of Select clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in SelectScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SelectCommand SelectFromScript<T>( string SelectScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
            where T : IDatabaseTable =>
            this.From( typeof ( T ) ).SelectScript( SelectScript, Parameters, throwExceptionIfParameterNotMatch );

        #endregion

        #region Where

        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SelectCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.WhereClause = new WhereScript( whereScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public SelectCommand WhereScript( string whereScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy where script and Parameter list to generate where clause. </summary>
        /// <param name="whereScript">where script of where clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in whereScript.</param>
        public SelectCommand WhereScript( string whereScript, params DBParameter[] Parameters ) {
            return this.WhereScript( whereScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public SelectCommand WhereScript( string whereScript, params DBParameter[] Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public SelectCommand WhereAnd( IEnumerable<DBParameter> Parameters ) {
            this.WhereClause = new WhereParameters( Parameters );
            return this;
        } // public SelectCommand WhereAnd( IEnumerable<DBParameter> Parameters )
        
        /// <summary> Generate where script by DBParameters with And condition. </summary>
        /// <param name="Parameters">Parameter list to generate to where script.</param>
        public SelectCommand WhereAnd( params DBParameter[] Parameters )
            => this.WhereAnd( Parameters.CastTo<IEnumerable<DBParameter>>() );
        
        /// <summary> Generate where script by one Column and value. </summary>
        /// <param name="Column">Column of value.</param>
        /// <param name="value">value of column.</param>
        public SelectCommand WhereOne( DBColumn Column, object value )
            => this.WhereAnd( new DBParameter( Column, value ) );

        #endregion
        
        #region OrderBy
        
        /// <summary> Use speficy OrderBy script and Parameter list to generate OrderBy clause. </summary>
        /// <param name="OrderByScript">OrderBy script of OrderBy clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in OrderByScript.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SelectCommand OrderByScript( string OrderByScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.OrderByClause = new OrderByScript( OrderByScript, Parameters, throwExceptionIfParameterNotMatch );
            return this;
        } // public SelectCommand OrderByScript( string OrderByScript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Use speficy OrderBy script and Parameter list to generate OrderBy clause. </summary>
        /// <param name="OrderByScript">OrderBy script of OrderBy clause.</param>
        /// <param name="Parameters">Parameter list which specify the value in OrderByScript.</param>
        public SelectCommand OrderByScript( string OrderByScript, params DBParameter[] Parameters ) {
            return this.OrderByScript( OrderByScript, Parameters.CastTo<IEnumerable<DBParameter>>() );
        } // public SelectCommand OrderByScript( string OrderByScript, params DBParameter[] Parameters )
        
        /// <summary> Generate OrderBy asc script by DBColumns. </summary>
        /// <param name="Columns">Column list to generate to OrderBy script.</param>
        public SelectCommand OrderByAsc( IEnumerable<DBColumn> Columns ) {
            this.OrderByClause = new OrderByAscColumns( Columns );
            return this;
        } // public SelectCommand OrderByAsc( IEnumerable<DBColumn> Columns )
        
        /// <summary> Generate OrderBy asc script by DBColumns. </summary>
        /// <param name="Columns">Column list to generate to OrderBy script.</param>
        public SelectCommand OrderByAsc( params DBColumn[] Columns )
            => this.OrderByAsc( Columns.CastTo<IEnumerable<DBColumn>>() );
        
        /// <summary> Generate OrderBy asc script by DBColumns. </summary>
        /// <param name="Columns">Column list to generate to OrderBy script.</param>
        public SelectCommand OrderByDesc( IEnumerable<DBColumn> Columns ) {
            this.OrderByClause = new OrderByDescColumns( Columns );
            return this;
        } // public SelectCommand OrderByDesc( IEnumerable<DBColumn> Columns )
        
        /// <summary> Generate OrderBy asc script by DBColumns. </summary>
        /// <param name="Columns">Column list to generate to OrderBy script.</param>
        public SelectCommand OrderByDesc( params DBColumn[] Columns )
            => this.OrderByDesc( Columns.CastTo<IEnumerable<DBColumn>>() );

        #endregion

        #region ToScript, Clone

        /// <summary> Generate the Select script. </summary>
        /// <param name="Provider">Database provider to get personal setting.</param>
        /// <param name="AddCommandSpliter">Add command spliter at the end or not.</param>
        public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false ) {
            // generate select column clause
            string selectColumnString = null;
            if ( this.SelectClause?.AnyClause() ?? false )
                selectColumnString = this.SelectClause.ToScript( Provider, this.QuoteType );

            // generate where clause
            string whereString = null;
            if ( this.WhereClause?.AnyClause() ?? false )
                whereString = this.WhereClause.ToScript( Provider, this.QuoteType );

            // generate order by clause
            string orderByString = null;
            if ( this.OrderByClause?.AnyClause() ?? false )
                orderByString = this.OrderByClause.ToScript( Provider, this.QuoteType );

            // generate whold command.
            var result = $"{selectColumnString} From {this.Table.ToScript( Provider, QuoteType )}{whereString}{orderByString}";
            if ( AddCommandSpliter )
                result += Provider.CommandSpliter;

            return result;
        } // public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false )
        
        /// <summary> Clone this object. </summary>
        public SelectCommand Clone() {
            var result = (SelectCommand)this.MemberwiseClone();
            result.SelectClause = result.SelectClause.Clone();
            result.WhereClause = result.WhereClause.Clone();
            return result;
        } // public SelectCommand Clone()

        #endregion

    } // public class SelectCommand : IDBCommand
    
} // namespace PGLibrary.Database
