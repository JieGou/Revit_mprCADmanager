namespace mprCADmanager.Revit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using View;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Visibility = System.Windows.Visibility;

    public class DeleteManyElementsEvent : IExternalEventHandler
    {
        private const string LangItem = "mprCADmanager";
        private IList<ElementId> _elementIds;
        private Document _doc;
        private readonly ExternalEvent _exEvent;
        private string _tName;
        private int _undeleted;
        public DWGImportManagerWindow MainWindow;
        public DeleteManyElementsEvent()
        {
            _exEvent = ExternalEvent.Create(this);
        }

        public void SetAction(IList<ElementId> elementId, string tName = "PIK_DeleteElement", Document doc = null)
        {
            _elementIds = elementId;
            _doc = doc;
            _tName = tName;
            _exEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            if (_elementIds != null && _elementIds.Any())
            {
                if (_doc == null) _doc = app.ActiveUIDocument.Document;
                app.Application.FailuresProcessing += Application_FailuresProcessing;
                MainWindow?.Dispatcher.Invoke(() =>
                {
                    MainWindow.ProgressBar.Visibility = Visibility.Visible;
                    MainWindow.ProgressText.Visibility = Visibility.Visible;
                    MainWindow.ProgressBar.Minimum = 0;
                    MainWindow.ProgressBar.Maximum = _elementIds.Count;
                    MainWindow.ProgressBar.Value = 0;
                    MainWindow.DgItems.IsEnabled = false;
                    MainWindow.CbSortVariants.IsEnabled = false;
                    MainWindow.TbSearch.IsEnabled = false;
                    MainWindow.BtDeleteSelected.IsEnabled = false;
                    MainWindow.Topmost = false;
                });
                System.Windows.Forms.Application.DoEvents();
                var progindex = 0;
                for (var i = 0; i < _elementIds.Count; i++)
                {
                    ElementId id = _elementIds[i];
                    progindex = i + 1;
                    MainWindow?.Dispatcher.Invoke(() =>
                    {
                        MainWindow.ProgressBar.Value = progindex;
                        MainWindow.ProgressText.Text = Language.GetItem(LangItem, "msg20") + " " + progindex + "/" + _elementIds.Count;
                    });
                    System.Windows.Forms.Application.DoEvents();
                    using (Transaction t = new Transaction(_doc, _tName))
                    {
                        FailureHandlingOptions failOpts = t.GetFailureHandlingOptions();
                        failOpts.SetClearAfterRollback(true);
                        t.SetFailureHandlingOptions(failOpts);
                        t.Start();
                        _doc.Delete(id);
                        t.Commit();
                    }
                }
                app.Application.FailuresProcessing -= Application_FailuresProcessing;
                //
                if (_undeleted > 0)
                {
                    MessageBox.Show(Language.GetItem(LangItem, "msg21") + ": " + _undeleted);
                    _undeleted = 0;
                }
                MainWindow?.Dispatcher.Invoke(() =>
                {
                    MainWindow.ProgressBar.Visibility = Visibility.Collapsed;
                    MainWindow.ProgressText.Visibility = Visibility.Collapsed;
                    MainWindow.DgItems.IsEnabled = true;
                    MainWindow.CbSortVariants.IsEnabled = true;
                    MainWindow.TbSearch.IsEnabled = true;
                    MainWindow.BtDeleteSelected.IsEnabled = true;
                    MainWindow.Topmost = true;
                });
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            // Inside event handler, get all warnings
            var failList = e.GetFailuresAccessor().GetFailureMessages();
            if (failList.Any())
            {
                _undeleted++;
                foreach (FailureMessageAccessor failure in failList)
                {
                    // check FailureDefinitionIds against ones that you want to dismiss, 
                    FailureDefinitionId failId = failure.GetFailureDefinitionId();
                    // prevent Revit from showing Unenclosed room warnings
                    if (failId == BuiltInFailures.EditingFailures.CannotEditDeletedElements)
                    {
                        e.GetFailuresAccessor().DeleteAllWarnings();
                    }

                }
                e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
            }
        }
        public string GetName()
        {
            return "DeleteElement";
        }
    }
}