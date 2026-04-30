using System;
using System.Threading.Tasks;
using AutoMetal.Infrastructure;
using AutoMetal.Services;
using AutoMetal.Shared;

namespace AutoMetal.Presenters
{
    internal class AutoMetalTrainingPresenter
    {
        private readonly IAutoMetalTrainingView _view;
        private readonly IModelConversionService _modelConversionService;
        private readonly IDeepLabTrainingService _trainingService;
        private readonly ICoverageInferenceService _inferenceService;
        private readonly IFileDialogService _dialogService;

        public AutoMetalTrainingPresenter(
            IAutoMetalTrainingView view,
            IModelConversionService modelConversionService,
            IDeepLabTrainingService trainingService,
            ICoverageInferenceService inferenceService,
            IFileDialogService dialogService)
        {
            _view = view;
            _modelConversionService = modelConversionService;
            _trainingService = trainingService;
            _inferenceService = inferenceService;
            _dialogService = dialogService;
        }

        public void BrowseBestPt()
        {
            string selected = _dialogService.SelectFile(
                _view as System.Windows.Forms.IWin32Window,
                "PyTorch权重 (*.pt;*.pth)|*.pt;*.pth|All files (*.*)|*.*",
                _view.BestPtPath);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                _view.BestPtPath = selected;
            }
        }

        public async Task ExportPtToEngineAsync()
        {
            _view.SetConversionInProgress(true);
            _view.SetConversionStatus("转换状态：转换中...");
            try
            {
                var result = await _modelConversionService.ConvertPtToEngineAsync(
                    _view.BestPtPath?.Trim() ?? string.Empty,
                    _view.TensorEnginePath?.Trim() ?? string.Empty,
                    AutoMetalConstants.deepLabProjectPath,
                    AutoMetalConstants.deepLabPythonExe);

                if (!result.success)
                {
                    _view.SetConversionStatus("转换状态：失败");
                    _view.ShowWarning(result.errorMessage);
                    return;
                }

                _view.TensorEnginePath = result.enginePath;
                _view.SetConversionStatus("转换状态：已完成");
                _view.ShowInfo($"Engine转换完成：{result.enginePath}");
            }
            catch (Exception ex)
            {
                _view.SetConversionStatus("转换状态：异常");
                _view.ShowWarning($"pt转engine异常：{ex.Message}");
            }
            finally
            {
                _view.SetConversionInProgress(false);
            }
        }

        public async Task ToggleTrainingAsync()
        {
            if (_trainingService.IsRunning)
            {
                _trainingService.Stop(_view.SetTrainStatus);
                return;
            }

            if (!int.TryParse(_view.EpochText?.Trim(), out int epoch) || epoch <= 0)
            {
                _view.ShowWarning("请先输入有效的Epoch");
                return;
            }
            if (!int.TryParse(_view.BatchSizeText?.Trim(), out int batchSize) || batchSize <= 0)
            {
                _view.ShowWarning("请先输入有效的BatchSize");
                return;
            }

            _view.ResetTrainProgress(epoch);
            _view.SetTrainButtonText("停止");
            _view.SetTrainStatus($"训练状态：启动中(1/{epoch})");

            var result = await _trainingService.StartAsync(
                epoch,
                batchSize,
                AutoMetalConstants.deepLabProjectPath,
                AutoMetalConstants.deepLabVocPath,
                AutoMetalConstants.deepLabPythonExe,
                _view.UpdateTrainProgress,
                _view.SetTrainStatus);

            _view.SetTrainButtonText("开始训练");
            if (result.success)
            {
                _view.LoadLatestTrainingCurveImage();
            }
            else if (!string.IsNullOrWhiteSpace(result.errorMessage))
            {
                _view.ShowWarning(result.errorMessage);
            }
        }

        public void BrowseTestSet()
        {
            string selected = _dialogService.SelectFolder(_view as System.Windows.Forms.IWin32Window, _view.TestSetPath);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                _view.TestSetPath = selected;
            }
        }

        public void BrowseEnginePath()
        {
            string selected = _dialogService.SelectFile(
                _view as System.Windows.Forms.IWin32Window,
                "TensorRT Engine (*.engine)|*.engine|All files (*.*)|*.*",
                _view.TensorEnginePath);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                _view.TensorEnginePath = selected;
            }
        }

        public void StartInference()
        {
            var result = _inferenceService.RunCoverageBatch(
                _view.TestSetPath?.Trim() ?? string.Empty,
                _view.TensorEnginePath?.Trim() ?? string.Empty,
                _view.UpdateInferProgress,
                _view.SetInferStatus);

            if (!result.success && !string.IsNullOrWhiteSpace(result.errorMessage))
            {
                _view.ShowWarning(result.errorMessage);
            }
        }
    }
}
