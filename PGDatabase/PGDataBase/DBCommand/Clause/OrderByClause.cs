using System.Collections.Generic;
using System.Linq;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Generate OrderBy asc clause with DBColumns. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class OrderByAscColumns : IClause {

        #region Property & Constructor

        /// <summary> DBColumns list </summary>
        private readonly List<DBColumn> mColumns = null;

        /// <summary> Initializes a new instance of the <see cref="OrderByAscColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public OrderByAscColumns( IEnumerable<DBColumn> Columns ) {
            this.mColumns = Columns?.ToList();
        } // public OrderByAscColumns( IEnumerable<DBColumn> Columns )

        /// <summary> Initializes a new instance of the <see cref="OrderByAscColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public OrderByAscColumns( params DBColumn[] Columns ) : this( (IEnumerable<DBColumn>)Columns ) { }

        #endregion

        #region IOrderByClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mColumns.IsNullOrEmpty();

        /// <summary> Generate instance to OrderBy script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> OrderBy script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each Column to [Column-Name] and use comma to join them.
            var result = string.Join( ", ", this.mColumns.Select( item => $"{Provider.Quote( item.Name, QuoteType )}" ) );
            
            return " Order By " + result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new OrderByAscColumns( this.mColumns );

        #endregion

    } // internal class OrderByAscColumns : IOrderByClause

    /// <summary> Generate OrderBy desc clause with DBColumns. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class OrderByDescColumns : IClause {

        #region Property & Constructor

        /// <summary> DBColumns list </summary>
        private readonly List<DBColumn> mColumns = null;

        /// <summary> Initializes a new instance of the <see cref="OrderByDescColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public OrderByDescColumns( IEnumerable<DBColumn> Columns ) {
            this.mColumns = Columns?.ToList();
        } // public OrderByDescColumns( IEnumerable<DBColumn> Columns )

        /// <summary> Initializes a new instance of the <see cref="OrderByDescColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public OrderByDescColumns( params DBColumn[] Columns ) : this( (IEnumerable<DBColumn>)Columns ) { }

        #endregion

        #region IOrderByClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mColumns.IsNullOrEmpty();

        /// <summary> Generate instance to OrderBy script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> OrderBy script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each Column to [Column-Name] and use comma to join them.
            var result = string.Join( ", ", this.mColumns.Select( item => $"{Provider.Quote( item.Name, QuoteType )} desc" ) );
            
            return " Order By " + result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new OrderByDescColumns( this.mColumns );

        #endregion

    } // internal class OrderByDescColumns : IOrderByClause
    
    /// <summary> Generate OrderBy clause with OrderBy script and DBParameters. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class OrderByScript : IClause {

        #region Property & Constructor

        /// <summary> OrderBy script </summary>
        private readonly string mScript = null;

        /// <summary> DBParameters list </summary>
        private readonly List<DBParameter> mParameters = null;

        /// <summary> throw exception if parameter is not exact match between mParameters and mScript </summary>
        private readonly bool mThrowExceptionIfParameterNotMatch = true;
        
        /// <summary> Initializes a new instance of the <see cref="DBParameter"/> class with OrderBy script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The OrderBy script.</param>
        /// <param name="Parameters">The parameter list.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public OrderByScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.mScript = Script;
            this.mParameters = Parameters.ToList();
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public OrderByScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Initializes a new instance of the <see cref="DBParameter"/> class with OrderBy script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The OrderBy script.</param>
        /// <param name="Parameters">The parameter list.</param>
        public OrderByScript( string Script, params DBParameter[] Parameters ) : this( Script, (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region IOrderByClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mScript.IsNullOrWhiteSpace();
        
        /// <summary> Generate instance to OrderBy script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> OrderBy script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            return " Order By " + DBParameter.ReplaceParameters( this.mScript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch );
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )
        
        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new OrderByScript( this.mScript, this.mParameters );

        #endregion

    } // internal class OrderByScript : IClause
    
} // namespace PGLibrary.Database
