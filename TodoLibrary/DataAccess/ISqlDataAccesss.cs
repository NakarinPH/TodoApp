namespace TodoLibrary.DataAccess
{
    public interface ISqlDataAccesss
    {
        Task<List<T>> LoadData<T, U>(string storeProcedure, U parameters, string connectionStringName);
        Task SaveData<T>(string storedProcedure, T parameters, string connectionStringName);
    }
}