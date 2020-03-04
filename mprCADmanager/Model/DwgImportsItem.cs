namespace mprCADmanager.Model
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Commands;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Windows;
    using Revit;
    using ViewModel;

    public class DwgImportsItem : VmBase
    {
        private const string LangItem = "mprCADmanager";
        private readonly UIApplication _uiApplication;
        private readonly DeleteElementEvent _deleteElementEvent;
        private readonly ChangeViewEvent _changeViewEvent;
        private bool _viewSpecific;
        private ElementId _id;
        private Category _category;
        private ElementId _ownerViewId;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="element"><see cref="CADLinkType"/> or <see cref="ImportInstance"/></param>
        /// <param name="uiApplication">Revit uiApp</param>
        /// <param name="dwgImportManagerVm">Link to main view model</param>
        /// <param name="deleteElementEvent"></param>
        /// <param name="changeViewEvent"></param>
        public DwgImportsItem(
            Element element,
            UIApplication uiApplication,
            DWGImportManagerVM dwgImportManagerVm,
            DeleteElementEvent deleteElementEvent,
            ChangeViewEvent changeViewEvent)
        {
            _deleteElementEvent = deleteElementEvent;
            _changeViewEvent = changeViewEvent;
            _uiApplication = uiApplication;
            DwgImportManagerVm = dwgImportManagerVm;
            ViewSpecific = element.ViewSpecific;
            OwnerViewId = element.OwnerViewId;
            Category = element.Category;
            Id = element.Id;

            Name = GetName(element);

            // commands
            CopyIdToClipboard = new RelayCommandWithoutParameter(CopyIdToClipboardAction);
            CopyOwnerViewIdToClipboard = new RelayCommandWithoutParameter(CopyOwnerViewIdToClipboardAction);
            ShowItem = new RelayCommandWithoutParameter(ShowItemAction);
            DeleteItem = new RelayCommandWithoutParameter(DeleteItemAction);
        }
        
        public DWGImportManagerVM DwgImportManagerVm { get; }

        /// <summary>Принадлежит ли элемент вид</summary>
        public bool ViewSpecific
        {
            get => _viewSpecific;
            set
            {
                _viewSpecific = value;
                OnPropertyChanged(nameof(ViewSpecific));
            }
        }

        /// <summary>Категория элемента</summary>
        /// <remarks>Может быть Null</remarks>
        public Category Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        /// <summary>Имя элемента</summary>
        public string Name { get; }

        /// <summary>ElementId элемента вставки</summary>
        public ElementId Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string IdToShow => Id.ToString();

        /// <summary>ElementId вида, которому принадлежит элемент вставки</summary>
        public ElementId OwnerViewId
        {
            get => _ownerViewId;
            set
            {
                _ownerViewId = value;
                OnPropertyChanged(nameof(OwnerViewId));
            }
        }

        public string OwnerViewIdToShow => OwnerViewId.ToString();

        /// <summary>Имя вида, которому принадлежит элемент вставки</summary>
        public string OwnerViewName
        {
            get
            {
                if (ViewSpecific && OwnerViewId.IntegerValue != -1)
                {
                    try
                    {
                        var viewElement = _uiApplication.ActiveUIDocument.Document.GetElement(OwnerViewId);
                        return viewElement.Name;
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentNullException)
                    {
                        return Language.GetItem(LangItem, "msg4");
                    }
                }

                return Language.GetItem(LangItem, "msg5");
            }
        }
        
        public ICommand CopyIdToClipboard { get; set; }

        public ICommand CopyOwnerViewIdToClipboard { get; set; }

        public ICommand ShowItem { get; set; }

        public ICommand DeleteItem { get; set; }

        private void CopyIdToClipboardAction()
        {
            Clipboard.SetText(IdToShow);
            DWGImportManagerCommand.MainWindow.PopupCopied.IsOpen = true;
        }

        private void CopyOwnerViewIdToClipboardAction()
        {
            Clipboard.SetText(OwnerViewIdToShow);
            DWGImportManagerCommand.MainWindow.PopupCopied.IsOpen = true;
        }

        private void ShowItemAction()
        {
            DWGImportManagerCommand.MainWindow.Topmost = false;
            _changeViewEvent.SetAction(this);
        }

        private void DeleteItemAction()
        {
            try
            {
                DWGImportManagerCommand.MainWindow.Topmost = false;
                var taskDialog = new TaskDialog(Language.GetItem(LangItem, "h1"))
                {
                    MainContent = Language.GetItem(LangItem, "msg6") + " \"" + Name + "\" " + Language.GetItem(LangItem, "msg7"),
                    CommonButtons = TaskDialogCommonButtons.None
                };
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, Language.GetItem(LangItem, "yes"));
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, Language.GetItem(LangItem, "no"));
                var result = taskDialog.Show();
                if (result == TaskDialogResult.CommandLink1)
                {
                    _deleteElementEvent.SetAction(Id, doc: _uiApplication.ActiveUIDocument.Document);
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                if (DWGImportManagerCommand.MainWindow != null)
                    DWGImportManagerCommand.MainWindow.Topmost = true;
            }
        }

        private string GetName(Element element)
        {
            if (element is CADLinkType)
                return element.Name;

            if (element is ImportInstance importInstance)
            {
                try
                {
                    return _uiApplication.ActiveUIDocument.Document.GetElement(importInstance.GetTypeId()).Name;
                }
                catch
                {
                    return Language.GetItem(LangItem, "msg3");
                }
            }

            return Language.GetItem(LangItem, "msg3");
        }
    }
}
