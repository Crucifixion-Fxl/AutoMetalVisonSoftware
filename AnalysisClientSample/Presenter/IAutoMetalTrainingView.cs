namespace AutoMetal.Presenters
{
    internal interface IAutoMetalTrainingView
    {
        string BestPtPath { get; set; }
        string TensorEnginePath { get; set; }
        string TestSetPath { get; set; }
        string EpochText { get; }
        string BatchSizeText { get; }

        void ShowWarning(string message);
        void ShowInfo(string message);
        void SetConversionStatus(string status);
        void SetConversionInProgress(bool inProgress);
        void SetTrainButtonText(string text);
        void SetTrainStatus(string status);
        void ResetTrainProgress(int total);
        void UpdateTrainProgress(int current, int total);
        void SetInferStatus(string status);
        void UpdateInferProgress(int current, int total);
        void LoadLatestTrainingCurveImage();
    }
}
