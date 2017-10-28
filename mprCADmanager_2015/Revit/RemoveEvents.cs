using System;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace mprCADmanager.Revit
{
    public class RemoveEvents : IExternalEventHandler
    {
        private readonly ExternalEvent _exEvent;
        private EventHandler<DocumentClosedEventArgs> _docClosed;
        private EventHandler<ViewActivatedEventArgs> _viewActivated;
        private EventHandler<DocumentChangedEventArgs> _docChanged;
        private EventHandler<DocumentCreatedEventArgs> _docCreated;

        public RemoveEvents()
        {
            _exEvent = ExternalEvent.Create(this);
        }

        public void SetAction(
            EventHandler<DocumentClosedEventArgs> docClosed,
            EventHandler<ViewActivatedEventArgs> viewActivated,
            EventHandler<DocumentChangedEventArgs> docChanged,
            EventHandler<DocumentCreatedEventArgs> docCreated)
        {
            _docClosed = docClosed;
            _viewActivated = viewActivated;
            _docChanged = docChanged;
            _docCreated = docCreated;
            _exEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            app.Application.DocumentClosed -= _docClosed;
            app.Application.DocumentChanged -= _docChanged;
            app.Application.DocumentCreated -= _docCreated;
            app.ViewActivated -= _viewActivated;
        }

        public string GetName()
        {
            return "RemoveEvents";
        }
    }
}
