using System;
using System.Collections.Generic;
using System.Linq;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Generate where clause with DBParameters and And condition. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class WhereParameters : IClause {

        #region Property & Constructor

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;

        /// <summary> Initializes a new instance of the <see cref="WhereParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public WhereParameters( IEnumerable<DBParameter> Parameters ) {
            this.mParameters = Parameters?.ToList();
        } // public WhereParameters( IEnumerable<DBParameter> Parameters )

        /// <summary> Initializes a new instance of the <see cref="WhereParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public WhereParameters( params DBParameter[] Parameters ) : this( (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region IWhereClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mParameters.IsNullOrEmpty();

        /// <summary> Generate instance to where script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> where script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each parameter to [Equal-Expression] and use And condition to join them.
            var result = string.Join( " And ", this.mParameters.Select( item => {
                if ( item.Value == null || item.Value == DBNull.Value )
                    return $"{Provider.Quote( item.Name, QuoteType )} is {Provider.ToDBValueString( item.Value, item.ColumnRule )}";
                else return $"{Provider.Quote( item.Name, QuoteType )} = {Provider.ToDBValueString( item.Value, item.ColumnRule )}";
            } ) );
            
            return " Where " + result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new WhereParameters( this.mParameters );

        #endregion

    } // internal class WhereParameters : IWhereClause
    
    /// <summary> Generate where clause with where script and DBParameters. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class WhereScript : IClause {

        #region Property & Constructor

        /// <summary> where script </summary>
        private string mScript = null;

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;

        /// <summary> throw exception if parameter is not exact match between mParameters and mScript </summary>
        private bool mThrowExceptionIfParameterNotMatch = true;
        
        /// <summary> Initializes a new instance of the <see cref="WhereParameters"/> class with where script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The where script.</param>
        /// <param name="Parameters">The parameter list.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public WhereScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.mScript = Script;
            this.mParameters = Parameters.ToList();
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public WhereScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Initializes a new instance of the <see cref="WhereParameters"/> class with where script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The where script.</param>
        /// <param name="Parameters">The parameter list.</param>
        public WhereScript( string Script, params DBParameter[] Parameters ) : this( Script, (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region IWhereClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mScript.IsNullOrWhiteSpace();
        
        /// <summary> Generate instance to where script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> where script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            return " Where " + DBParameter.ReplaceParameters( this.mScript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch );
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )
        
        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new WhereScript( this.mScript, this.mParameters );

        #endregion

    } // internal class WhereScript : IClause
    
} // namespace PGLibrary.Database
