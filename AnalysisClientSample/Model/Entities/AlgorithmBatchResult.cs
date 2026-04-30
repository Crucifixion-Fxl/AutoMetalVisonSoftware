namespace AutoMetal.Models
{
    internal class AlgorithmBatchResult
    {
        public string BatchName { get; set; }
        public string SampleName { get; set; }
        public double? Coverage { get; set; }
        public double? Uniformity { get; set; }
        public string Status { get; set; }
        public string AlgorithmImagePath { get; set; }
    }
}
