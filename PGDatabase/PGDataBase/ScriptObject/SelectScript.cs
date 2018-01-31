using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGLibrary.Data;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Data {
    /// <summary> Object to generate Select Script. </summary>
    public class SelectScript {

        #region Property

        /// <summary> Table Name </summary>
        protected string TableName;

        /// <summary> Columns to select. ( use this field when ColumnString is null ) </summary>
        protected List<string> Columns = new List<string>();

        /// <summary> Where values. ( use this field when WhereString is null ) </summary>
        protected ListS<string,object> WhereValues = new ListS<string, object>();

        /// <summary> Order values. ( use this field when OrderString is null ) </summary>
        protected ListS<string,EOrderType> OrderValues = new ListS<string, EOrderType>();

        /// <summary> Column string in select script. ( use this first ) </summary>
        protected string ColumnString = null;

        /// <summary> Where string in select script. ( use this first ) </summary>
        protected string WhereString = null;

        /// <summary> Order by string in select script. ( use this first ) </summary>
        protected string OrderString = null;

        /// <summary> Determine how to quote the keyword. </summary>
        protected EQuoteType QuoteType = EQuoteType.Auto;

        /// <summary> DataBase provider to provide specific infomation </summary>
        public IDataBaseProvider Provider { get; private set; }

        #endregion
        
        /// <summary> Create SelectScript object. </summary>
        /// <param name="Provider"> DataBase provider to provide specific infomation. </param>
        /// <param name="QuoteType"> Determine how to quote the keyword </param>
        public SelectScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto ) {
            this.Provider = Provider;
            this.QuoteType = QuoteType;
        }  // public SelectScript( IDataBaseProvider Provider, EQuoteType QuoteType = EQuoteType.Auto )

        #region Select

        /// <summary> use the columns script to set select caluse, use this script first and ignore Column names array. </summary>
        /// <param name="columnsScript"> where caluse </param>
        public virtual SelectScript Select( string columnsScript ) {
            this.ColumnString = columnsScript;
            return this;
        } // public virtual SelectScript Select( string columnsScript )
        
        /// <summary> same with => .Select( "*" ) </summary>
        public virtual SelectScript SelectAll() {
            this.ColumnString = "*";
            return this;
        } // public virtual SelectScript SelectAll()
        
        /// <param name="Columns"> columns name of select script. </param>
        public virtual SelectScript Select( params string[] Columns ) {
            this.Columns.AddRange( Columns );
            return this;
        } // public virtual SelectScript Select( params string[] Columns )


        /// <summary> Add column name and value by [ColumnAttribute] of entity. </summary>
        /// <typeparam name="T"> type of Entity </typeparam>
        /// <param name="Entity"> the Entity of column name and value. </param>
        public virtual SelectScript Select<T>( T Entity )
            where T : IDataBaseExtension {
            
            // get mapping member from target type.
            var membersHasColumnAttribute = _DBColumnAttribute_MemberInfo.GetCorrespondMember( typeof( T ) );
            var membersNeedSelect = membersHasColumnAttribute
                .Where( item => item.Value.Any( member => member.ConvertType.HasFlag( EConvertType.FromDB ) ) )
                .Select( item => item.Key );
            
            // get member with column name.
            this.Columns.AddRange( membersNeedSelect );

            return this;
        } // public override SelectScript Select<T>( T Entity )

        #endregion

        #region From

        /// <summary> set table name to select </summary>
        /// <param name="TableName"> table name to select </param>
        public virtual SelectScript From( string TableName ) {
            this.TableName = TableName;
            return this;
        } // public virtual SelectScript From( string TableName )

        #endregion

        #region Where

        /// <summary> use the where script to set Where caluse, use this script first and ignore Where objects. </summary>
        /// <param name="whereScript"> where caluse </param>
        public virtual SelectScript Where( string whereScript ) {
            this.WhereString = whereScript;
            return this;
        } // public virtual SelectScript Where( string whereScript )
        
        /// <summary>
        /// add the where value with column name.
        /// ( only support "And" caluse, if need complex where, please use Where method of whereScript. ) </summary>
        /// <param name="Column"> Column name </param>
        /// <param name="Value"> Value </param>
        public virtual SelectScript Where( string Column, object Value ) {
            WhereValues.Add( Column, Value );
            return this;
        } // public override SelectScript Where( string Column, object Value )

        #endregion

        #region Order

        /// <summary> use the order script to set Order caluse, use this script first and ignore Order objects. </summary>
        /// <param name="OrderScript"> order caluse </param>
        public virtual SelectScript Order( string OrderScript ) {
            this.OrderString = OrderScript;
            return this;
        } // public virtual SelectScript Order( string OrderScript )

        /// <summary> use the order script to set Order caluse, use this script first and ignore Order objects. </summary>
        /// <param name="Column"> order column name </param>
        /// <param name="OrderType"> how to order this column </param>
        public virtual SelectScript Order( string Column, EOrderType OrderType ) {
            OrderValues.Add( Column, OrderType );
            return this;
        } // public virtual SelectScript Order( string Column, OrderType OrderType )

        #endregion

        #region ToString, Clone
        
        /// <summary> generate the Select script. </summary>
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


            // if has [OrderString], use it.
            string orderString = this.OrderString;

            // otherwise, get OrderString from [OrderValues].
            if ( orderString.IsNullOrEmpty() )
                orderString = string.Join( ", ", OrderValues.Select( item => $"{Provider.Quote( item.Item1, QuoteType )} {item.Item2.ToString()}" ) );
            
            if ( !orderString.IsNullOrWhiteSpace() ) orderString = " Order " + orderString;
            
                    
            string columnsString = this.ColumnString;
            if ( columnsString.IsNullOrEmpty() )
                columnsString = string.Join( ", ", Columns.Select( item => $"{Provider.Quote( item, QuoteType )}" ) );

            return $"Select {columnsString} From {Provider.Quote( TableName, QuoteType )}{whereString}{orderString}";
        } // public override string ToString()

        /// <summary> Clone this object. </summary>
        public virtual SelectScript Clone() {
            var result = this.MemberwiseClone().CastTo<SelectScript>();
            result.Columns = new List<string>( this.Columns );
            result.WhereValues = new ListS<string, object>( this.WhereValues );
            result.OrderValues = new ListS<string, EOrderType>( this.OrderValues );
            return result;
        } // public virtual SelectScript Clone()

        #endregion

    } // public class SelectScript
} // namespace PGLibrary.Data
