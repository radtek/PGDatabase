using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PGCafe;
using PGCafe.Object;
using PGLibrary.Data;

namespace PGLibrary.Data {
    /// <summary> Object to generate Update Script. </summary>
    public class UpdateScript {

        #region Property

        /// <summary> Table Name </summary>
        protected string TableName;

        /// <summary> Column name and values to update. </summary>
        protected ListS<string,object> Values = new ListS<string, object>();

        /// <summary> Where values. ( use this field when WhereString is null ) </summary>
        protected ListS<string,object> WhereValues = new ListS<string, object>();

        /// <summary> Where string in update script. ( use this first ) </summary>
        protected string WhereString = null;

        /// <summary> Determine how to quote the keyword. </summary>
        protected EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> DataBase provider to provide specific infomation </summary>
        public IDataBaseProvider Provider { get; private set; }
        
        #endregion

        #region Constructor and assign data method

        /// <summary> Create UpdateScript object. </summary>
        /// <param name="Provider"> DataBase provider to provide specific infomation. </param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public UpdateScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto ) {
            this.Provider = Provider;
            this.QuoteType = QuoteType;
        }  // public UpdateScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto )
        

        /// <summary> set table name to update </summary>
        /// <param name="TableName"> table name to update </param>
        public virtual UpdateScript Update( string TableName ) {
            this.TableName = TableName;
            return this;
        } // public virtual UpdateScript Update( string TableName )

        /// <summary> add the update value with column name. </summary>
        /// <param name="Column"> Column name </param>
        /// <param name="Value"> Value </param>
        public virtual UpdateScript Set( string Column, object Value ) {
            Values.Add( Column, Value );
            return this;
        } // public override UpdateScript Set( string Column, object Value )

        /// <summary> Add column name and value by [ColumnAttribute] of entity. </summary>
        /// <typeparam name="T"> type of Entity </typeparam>
        /// <param name="Entity"> the Entity of column name and value. </param>
        public virtual UpdateScript Set<T>( T Entity ) {
            
            // get mapping member from target type.
            var membersHasColumnAttribute = _DBColumnAttribute_MemberInfo.GetCorrespondMember( typeof( T ) );

            // get member with column name and first member with column name.
            var members = membersHasColumnAttribute
                .Select( item => new { ColumnName = item.Key, MemberInfo = item.Value.First() } )
                .Where( item => item.MemberInfo.ConvertType.HasFlag( EConvertType.ToDB ) );
            
            // for each member, try get value from datarow.
            foreach ( var member in members ) {
                var valueObj = member.MemberInfo.GetValue( Entity );
                Values.Add( member.ColumnName, valueObj );
            } // foreach

            return this;
        } // public override UpdateScript Set<T>( T Entity )

        /// <summary> use the where script to set Where caluse, use this script first and ignore WhereAnd objects. </summary>
        /// <param name="whereScript"> where caluse </param>
        public virtual UpdateScript Where( string whereScript ) {
            this.WhereString = whereScript;
            return this;
        } // public virtual UpdateScript Where( string whereScript )
        
        /// <summary>
        /// add the where value with column name.
        /// ( only support "And" caluse, if need complex where, please use Where method. ) </summary>
        /// <param name="Column"> Column name </param>
        /// <param name="Value"> Value </param>
        public virtual UpdateScript Where( string Column, object Value ) {
            WhereValues.Add( Column, Value );
            return this;
        } // public override UpdateScript Where( string Column, object Value )
        
        #endregion

        #region ToString, Clone
        
        /// <summary> generate the Update script. </summary>
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
            
            string columnValueSetString = string.Join( ", ", Values.Select( item => $"{Provider.Quote( item.Item1, QuoteType )} = {Provider.ToDBValueString( item.Item2 )}" ) );
            return $"Update {Provider.Quote( TableName, QuoteType )} Set {columnValueSetString}{whereString}";
        } // public override string ToString()
        
        /// <summary> Clone this object. </summary>
        public virtual UpdateScript Clone() {
            var result = this.MemberwiseClone().CastTo<UpdateScript>();
            result.Values = new ListS<string, object>( this.Values );
            result.WhereValues = new ListS<string, object>( this.WhereValues );
            return result;
        } // public virtual UpdateScript Clone()

        #endregion

    } // public class UpdateScript
} // namespace PGLibrary.Data
