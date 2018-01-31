using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Data {
    /// <summary> Object to generate Update Script. </summary>
    public class DeleteScript {

        #region Property

        /// <summary> Table Name </summary>
        protected string TableName;

        /// <summary> Where values. ( use this field when WhereString is null ) </summary>
        protected ListS<string,object> WhereValues = new ListS<string, object>();

        /// <summary> Where string in delete script. ( use this first ) </summary>
        protected string WhereString = null;

        /// <summary> Determine how to quote the keyword. </summary>
        protected EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> DataBase provider to provide specific infomation </summary>
        public IDataBaseProvider Provider { get; private set; }

        #endregion

        #region Constructor and assign data method

        /// <summary> Create DeleteScript object. </summary>
        /// <param name="Provider"> DataBase provider to provide specific infomation. </param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public DeleteScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto ) {
            this.Provider = Provider;
            this.QuoteType = QuoteType;
        }  // public DeleteScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto )
        

        /// <summary> set table name to delete </summary>
        /// <param name="TableName"> table name to delete </param>
        public virtual DeleteScript DeleteFrom( string TableName ) {
            this.TableName = TableName;
            return this;
        } // public virtual DeleteScript DeleteFrom( string TableName )
        
        

        /// <summary> use the where script to set Where caluse, use this script first and ignore WhereAnd objects. </summary>
        /// <param name="whereScript"> where caluse </param>
        public virtual DeleteScript Where( string whereScript ) {
            this.WhereString = whereScript;
            return this;
        } // public virtual DeleteScript Where( string whereScript )

        /// <summary>
        /// add the where value with column name.
        /// ( only support "And" caluse, if need complex where, please use Where method. ) </summary>
        /// <param name="Column"> Column name </param>
        /// <param name="Value"> Value </param>
        public virtual DeleteScript Where( string Column, object Value ) {
            WhereValues.Add( Column, Value );
            return this;
        } // public override DeleteScript Where( string Column, object Value )
        
        #endregion

        #region ToString, Clone

        /// <summary> generate the Delete script. </summary>
        public override string ToString() {
            // if has [WhereString], use it.
            string whereString = this.WhereString;

            // otherwise, get whereString from [WhereValues].
            if ( whereString.IsNullOrEmpty() ) {
                var whereValuePairString =
                    WhereValues.Select( item => {
                        if ( item.Item2 == null || item.Item2 == DBNull.Value ) // if value is null, use "ColumnName is null" script.
                            return $"{Provider.Quote( item.Item1, QuoteType )} is null";
                        else
                            return $"{Provider.Quote( item.Item1, QuoteType )} = {Provider.ToDBValueString( item.Item2 )}";
                    } );

                whereString = string.Join( " And ", whereValuePairString );
            } // if
            
            if ( !whereString.IsNullOrWhiteSpace() ) whereString = " Where " + whereString;
            
            return $"Delete From {Provider.Quote( TableName, QuoteType )}{whereString}";
        } // public override string ToString()
        
        /// <summary> Clone this object. </summary>
        public virtual DeleteScript Clone() {
            var result = this.MemberwiseClone().CastTo<DeleteScript>();
            result.WhereValues = new ListS<string, object>( this.WhereValues );
            return result;
        } // public virtual DeleteScript Clone()

        #endregion

    } // public class DeleteScript
} // namespace PGLibrary.Data
