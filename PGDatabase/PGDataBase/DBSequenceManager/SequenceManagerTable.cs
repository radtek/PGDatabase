using System;

namespace PGLibrary.Database {

    /// <summary> Manager the SequenceManagerTable in DB. </summary>
    /// <typeparam name="TKeyEnum">The type of the key's enum.</typeparam>
    /// <seealso cref="PGLibrary.Database.IDatabaseTable" />
    public abstract class SequenceManagerTable<TKeyEnum> : IDatabaseTable
        where TKeyEnum : struct, IConvertible {

        /// <summary> specific key to manager sequence. ( usually use table name or table name + column name ) . </summary>
        public abstract TKeyEnum Key { get; set; }

        /// <summary> current used max sequence no in specific key. </summary>
        public abstract int Sequence { get; set; }

        /// <summary> when the key is not found in table, call this method to generate sequence. </summary>
        /// <param name="mDBObj">The database object.</param>
        /// <returns> the sequence should used when the key is not found in table. </returns>
        public virtual int InitialSequence( PGDatabase mDBObj ) {
            return 1;
        } // public virtual int InitialSequence( PGDatabase mDBObj )

        /// <summary> insert into data to table. </summary>
        public virtual void Insert( PGDatabase DBObject ) {
            DBObject.ExecuteNonQuery( new InsertCommand( this ) );
        } // public virtual void Insert( PGDatabase DBObject )


        /// <summary> update the data to table. </summary>
        public virtual void Update( PGDatabase DBObject ) {
            DBObject.ExecuteNonQuery( new UpdateCommand()
                .UpdateTo( this.GetType() )
                .Set( this.Columns( nameof( this.Sequence ) ) ) );
        } // public virtual void Update( PGDatabase DBObject )

    } // public abstract class SequenceManagerTable<TEnum> : IDatabaseTable

} // namespace PGLibrary.Database
