namespace AutoMetal.Services
{
    internal interface IAlgorithmBatchProcessingService
    {
        AlgorithmBatchResponse Execute(AlgorithmBatchRequest request);
    }
}
