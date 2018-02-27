using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprCADmanager.Revit
{
    public class DeleteElementEvent : IExternalEventHandler
    {
        private const string LangItem = "mprCADmanager";
        private ElementId _elementId;
        private Document _doc;
        private readonly ExternalEvent _exEvent;
        private string _tName;

        private int _undeleted = 0;

        public DeleteElementEvent()
        {
            _exEvent = ExternalEvent.Create(this);
        }

        public void SetAction(ElementId elementId, string tName = "PIK_DeleteElement", Document doc = null)
        {
            _elementId = elementId;
            _doc = doc;
            _tName = tName;
            _exEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            if (_elementId != null)
            {
                if (_doc == null) _doc = app.ActiveUIDocument.Document;
                app.Application.FailuresProcessing += Application_FailuresProcessing;
                using (Transaction t = new Transaction(_doc, _tName))
                {
                    FailureHandlingOptions failOpts = t.GetFailureHandlingOptions();
                    failOpts.SetClearAfterRollback(true);
                    t.SetFailureHandlingOptions(failOpts);
                    t.Start();
                    //
                    _doc.Delete(_elementId);
                    t.Commit();
                }
                app.Application.FailuresProcessing -= Application_FailuresProcessing;
                //
                if (_undeleted > 0)
                {
                   MessageBox.Show(Language.GetItem(LangItem, "msg19"));
                    _undeleted = 0;
                }
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