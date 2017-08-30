namespace Lykke.Logs
{
    public interface ILogEntityRowKeyGenerator<in TLogEntity>
    {
        string Generate(TLogEntity entity, int retryNum, int batchItemNum);
    }
}