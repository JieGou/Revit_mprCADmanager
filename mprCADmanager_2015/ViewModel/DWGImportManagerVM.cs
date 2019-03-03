using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprCADmanager.Commands;
using mprCADmanager.Model;
using mprCADmanager.Revit;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprCADmanager.ViewModel
{
    // ReSharper disable once InconsistentNaming
    public class DWGImportManagerVM : ViewModelBase
    {
        private const string LangItem = "mprCADmanager";
        private readonly DeleteElementEvent _deleteElementEvent;
        private readonly ChangeViewEvent _changeViewEvent;
        private readonly DeleteManyElementsEvent _deleteManyElementsEvent;
        public ICommand DeleteSelectedCommand { get; set; }
        public ICommand SelectAllCommand { get; set; }

        #region Конструктор

        public DWGImportManagerVM(
            UIApplication uiApplication,
            List<Element> elements,
            DeleteElementEvent deleteElementEvent,
            ChangeViewEvent changeViewEvent,
            DeleteManyElementsEvent deleteManyElementsEvent)
        {
            _deleteElementEvent = deleteElementEvent;
            _changeViewEvent = changeViewEvent;
            _deleteManyElementsEvent = deleteManyElementsEvent;
            UiApplication = uiApplication;
            FillDwgImportsItems(elements);
            CurrentSortVariant = SortVariants[0];
            DeleteSelectedCommand = new RelayCommand(DeleteSelectedItems, o => true);
            SelectAllCommand = new RelayCommand(SelectAll, o => true);
        }
        #endregion

        #region Поля

        public UIApplication UiApplication { get; set; }
        
        public Document Doc => UiApplication.ActiveUIDocument.Document;

        private ObservableCollection<DwgImportsItem> _dwgImportsItems = new ObservableCollection<DwgImportsItem>();
        /// <summary>Коллекция обозначений импорта</summary>
        public ObservableCollection<DwgImportsItem> DwgImportsItems
        {
            get => _dwgImportsItems;
            set
            {
                _dwgImportsItems = value;
                OnPropertyChanged(nameof(DwgImportsItems));
                OnPropertyChanged(nameof(DwgImportsItemsToShow));
                OnPropertyChanged(nameof(SortVariants));
            }
        }

        public ObservableCollection<DwgImportsItem> DwgImportsItemsToShow
        {
            get
            {
                if (_currentSortVariant.Equals(Language.GetItem(LangItem, "sv1")))
                {
                    if (string.IsNullOrEmpty(SearchText))
                        return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(x => x.Category == null));
                    return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(
                        x => x.Category == null &&
                        (x.Name.ToLower().Contains(SearchText.ToLower()) ||
                        x.OwnerViewName.ToLower().Contains(SearchText.ToLower()))));
                }
                if (_currentSortVariant.Equals(Language.GetItem(LangItem, "sv2")))
                {
                    if (string.IsNullOrEmpty(SearchText))
                        return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(x => x.Category != null && x.ViewSpecific));
                    return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(
                        x => x.Category != null && x.ViewSpecific &&
                        (x.Name.ToLower().Contains(SearchText.ToLower()) ||
                        x.OwnerViewName.ToLower().Contains(SearchText.ToLower()))));
                }
                if (_currentSortVariant.Equals(Language.GetItem(LangItem, "sv3")))
                {
                    if (string.IsNullOrEmpty(SearchText))
                        return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(x => x.Category != null && x.ViewSpecific == false));
                    return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(
                        x => x.Category != null && x.ViewSpecific == false &&
                        (x.Name.ToLower().Contains(SearchText.ToLower()) ||
                        x.OwnerViewName.ToLower().Contains(SearchText.ToLower()))));
                }
                if (string.IsNullOrEmpty(SearchText))
                    return DwgImportsItems;
                return new ObservableCollection<DwgImportsItem>(DwgImportsItems.Where(
                    x => x.Name.ToLower().Contains(SearchText.ToLower()) ||
                    x.OwnerViewName.ToLower().Contains(SearchText.ToLower())));
            }
        }

        private List<string> _sortVariants;
        /// <summary>Варианты сортировки</summary>
        public List<string> SortVariants
        {
            get
            {
                _sortVariants = new List<string> { ModPlusAPI.Language.GetItem(LangItem, "all") };
                var hasUnidentified = false;
                var hasViewSpecificImports = false;
                var hasModelImports = false;
                foreach (DwgImportsItem item in DwgImportsItems)
                {
                    if (item.Category == null) hasUnidentified = true;
                    else
                    {
                        if (item.ViewSpecific) hasViewSpecificImports = true;
                        else hasModelImports = true;
                    }
                }
                if (hasUnidentified) _sortVariants.Add(Language.GetItem(LangItem, "sv1"));
                if (hasViewSpecificImports) _sortVariants.Add(Language.GetItem(LangItem, "sv2"));
                if (hasModelImports) _sortVariants.Add(Language.GetItem(LangItem, "sv3"));
                return _sortVariants;
            }
        }

        private string _currentSortVariant;
        /// <summary>Текущий выбранный вариант сортировки</summary>
        public string CurrentSortVariant
        {
            get => _currentSortVariant;
            set
            {
                _currentSortVariant = value;
                OnPropertyChanged(nameof(CurrentSortVariant));
                OnPropertyChanged(nameof(DwgImportsItemsToShow));
            }
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(DwgImportsItemsToShow));
            }
        }
        #endregion

        #region Методы

        private void FillDwgImportsItems(List<Element> collector)
        {
            foreach (Element element in collector)
            {
                DwgImportsItems.Add(new DwgImportsItem(element, UiApplication, this, _deleteElementEvent, _changeViewEvent));
            }
        }

        private void DeleteSelectedItems(object o)
        {
            try
            {
                System.Collections.IList items = (System.Collections.IList)o;
                if (items != null && items.Count > 0)
                {
                    DWGImportManagerCommand.MainWindow.Topmost = false;
                    var taskDialog = new TaskDialog(Language.GetItem(LangItem, "h1"))
                    {
                        MainContent = Language.GetItem(LangItem, "msg1"),
                        CommonButtons = TaskDialogCommonButtons.None
                    };
                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, Language.GetItem(LangItem, "yes"));
                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, Language.GetItem(LangItem, "no"));
                    var result = taskDialog.Show();
                    if (result == TaskDialogResult.CommandLink1)
                    {
                        var selectedDwgimports = items.Cast<DwgImportsItem>().ToList();
                        var ids = new List<ElementId>();
                        foreach (var item in selectedDwgimports)
                            ids.Add(item.Id);
                        _deleteManyElementsEvent.SetAction(ids, doc: UiApplication.ActiveUIDocument.Document);
                    }
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
                DwgImportsItems.Clear();
                FillDwgImportsItems(DWGImportManagerCommand.GetElements(UiApplication.ActiveUIDocument.Document));
            }
        }

        private void SelectAll(object o)
        {
            try
            {
                if (DWGImportManagerCommand.MainWindow != null)
                    DWGImportManagerCommand.MainWindow.DgItems.SelectAll();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
        #endregion
    }
}
