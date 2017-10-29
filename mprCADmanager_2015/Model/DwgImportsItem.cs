using System;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprCADmanager.Commands;
using mprCADmanager.Revit;
using mprCADmanager.ViewModel;
using ModPlusAPI.Windows;

namespace mprCADmanager.Model
{
    public class DwgImportsItem : ViewModelBase
    {
        private readonly UIApplication _uiApplication;
        public DWGImportManagerVM DwgImportManagerVm;
        private readonly DeleteElementEvent _deleteElementEvent;
        private readonly ChangeViewEvent _changeViewEvent;
        #region Конструктор

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="importInstance">ImportInstance</param>
        /// <param name="uiApplication">Revit uiApp</param>
        /// <param name="dwgImportManagerVm">Link to main view model</param>
        /// <param name="deleteElementEvent"></param>
        /// <param name="changeViewEvent"></param>
        public DwgImportsItem(
            Element importInstance,
            UIApplication uiApplication,
            DWGImportManagerVM dwgImportManagerVm,
            DeleteElementEvent deleteElementEvent,
            ChangeViewEvent changeViewEvent)
        {
            _deleteElementEvent = deleteElementEvent;
            _changeViewEvent = changeViewEvent;
            _uiApplication = uiApplication;
            DwgImportManagerVm = dwgImportManagerVm;
            ViewSpecific = importInstance.ViewSpecific;
            OwnerViewId = importInstance.OwnerViewId;
            Category = importInstance.Category;
            Id = importInstance.Id;
            // commands
            CopyIdToClipboard = new RelayCommand(CopyIdToClipboardAction, o => true);
            CopyOwnerViewIdToClipboard = new RelayCommand(CopyOwnerViewIdToClipboardAction, o => true);
            ShowItem = new RelayCommand(ShowItemAction, o => true);
            DeleteItem = new RelayCommand(DeleteItemAction, o => true);
        }
        #endregion

        #region Поля

        private bool _viewSpecific;
        /// <summary>Принадлежит ли элемент вид</summary>
        public bool ViewSpecific
        {
            get => _viewSpecific;
            set { _viewSpecific = value; OnPropertyChanged(nameof(ViewSpecific)); }
        }

        private Category _category;
        /// <summary>Категория элемента</summary>
        /// <remarks>Может быть Null</remarks>
        public Category Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        /// <summary>Имя элемента</summary>
        public string Name
        {
            get
            {
                if (Category != null)
                    return Category.Name;
                return "Неопознанный элемент вставки";
            }
        }

        private ElementId _id;
        /// <summary>ElementId элемента вставки</summary>
        public ElementId Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string IdToShow => Id.ToString();

        private ElementId _ownerViewId;
        /// <summary>ElementId вида, которому принадлежит элемент вставки</summary>
        public ElementId OwnerViewId
        {
            get => _ownerViewId;
            set { _ownerViewId = value; OnPropertyChanged(nameof(OwnerViewId)); }
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
                        Element viewElement = _uiApplication.ActiveUIDocument.Document.GetElement(OwnerViewId);
                        return viewElement.Name;
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentNullException)
                    {
                        return "Ошибка при получении имени вида";
                    }
                }
                return "Не принадлежит виду";
            }
        }

        #endregion

        #region Commands

        public ICommand CopyIdToClipboard { get; set; }
        public ICommand CopyOwnerViewIdToClipboard { get; set; }
        public ICommand ShowItem { get; set; }
        public ICommand DeleteItem { get; set; }

        #endregion
        #region Методы

        private void CopyIdToClipboardAction(object o)
        {
            Clipboard.SetText(IdToShow);
            DWGImportManagerCommand.MainWindow.FlyoutCopied.IsOpen = true;
        }

        private void CopyOwnerViewIdToClipboardAction(object o)
        {
            Clipboard.SetText(OwnerViewIdToShow);
            DWGImportManagerCommand.MainWindow.FlyoutCopied.IsOpen = true;
        }

        private void ShowItemAction(object o)
        {
            DWGImportManagerCommand.MainWindow.Topmost = false;
            _changeViewEvent.SetAction(this);
        }

        private void DeleteItemAction(object o)
        {
            try
            {
                DWGImportManagerCommand.MainWindow.Topmost = false;
                var taskDialog = new TaskDialog("CAD менеджер")
                {
                    MainContent = "Обозначение импорта " + this.Name + " будет удалено безвозратно!" +
                                  Environment.NewLine + "Продолжить?",
                    CommonButtons = TaskDialogCommonButtons.None
                };
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Да");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Нет");
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
        
        #endregion
    }
}
