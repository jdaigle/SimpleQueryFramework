using System.Data;

namespace SimpleQuery {
    public interface IPersistenceManager {
        void Open();
        bool IsOpened();
        void Rollback();
        void Commit();
        IDbCommand CreateCommand();
    }
}
