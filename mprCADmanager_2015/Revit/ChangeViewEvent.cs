using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprCADmanager.Commands;
using mprCADmanager.Model;
using ModPlusAPI.Windows;

namespace mprCADmanager.Revit
{
    public class ChangeViewEvent : IExternalEventHandler
    {
        private readonly ExternalEvent _exEvent;
        private DwgImportsItem _dwgImportsItem;

        public ChangeViewEvent()
        {
            _exEvent = ExternalEvent.Create(this);
        }

        public void SetAction(DwgImportsItem dwgImportsItem)
        {
            _dwgImportsItem = dwgImportsItem;
            _exEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            try
            {
                if (_dwgImportsItem != null)
                {
                    var doc = app.ActiveUIDocument.Document;

                    if (_dwgImportsItem.ViewSpecific)
                    {
                        // get importsInstance element
                        var element = app.ActiveUIDocument.Document.GetElement(_dwgImportsItem.Id);
                        // get specific view
                        var view =
                            app.ActiveUIDocument.Document.GetElement(_dwgImportsItem.OwnerViewId) as
                                Autodesk.Revit.DB.View;

                        // check is hidden
                        if (element != null && view != null)
                        {
                            var areImportCategoriesVisible = false;
                            var enableRevealHiddenMode = false;
                            var areCategoryVisible = false;
                            var areElementVisible = false;
                            // Проверяеям, что импортированные виды на виде включены
                            if (view.AreImportCategoriesHidden)
                            {
                                var taskDialog = new TaskDialog("CAD менеджер")
                                {
                                    MainContent = "На виде " + view.Name + " скрыты все импортированные категории" +
                                                  Environment.NewLine + "Выберите дальнейшее действие:",
                                    CommonButtons = TaskDialogCommonButtons.None
                                };
                                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                    "Показать все импортированные категории");
                                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                    "Включить отображение скрытых элементов");
                                var result = taskDialog.Show();
                                if (result == TaskDialogResult.CommandLink1)
                                {
                                    using (Transaction t = new Transaction(doc, "PIK_ChangeView"))
                                    {
                                        t.Start();
                                        view.AreImportCategoriesHidden = false;
                                        t.Commit();
                                    }
                                    areImportCategoriesVisible = true;
                                }
                                else if (result == TaskDialogResult.CommandLink2)
                                {
                                    enableRevealHiddenMode = true;
                                }
                            }
                            else areImportCategoriesVisible = true;
                            // Если включили отображение импортированных категорий, тогда проверяем уже категорию
                            if (areImportCategoriesVisible)
                            {
                                if (!_dwgImportsItem.Category.get_Visible(view))
                                {
                                    var taskDialog = new TaskDialog("CAD менеджер")
                                    {
                                        MainContent = "Категория " + _dwgImportsItem.Category.Name +
                                                      " скрыта на виде " + view.Name +
                                                      Environment.NewLine + "Выберите дальнейшее действие:",
                                        CommonButtons = TaskDialogCommonButtons.None
                                    };
                                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                        "Сделать категорию видимой на виде");
                                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                        "Включить отображение скрытых элементов");
                                    var result = taskDialog.Show();
                                    if (result == TaskDialogResult.CommandLink1)
                                    {
                                        using (Transaction t = new Transaction(doc, "PIK_ChangeView"))
                                        {
                                            t.Start();
                                            _dwgImportsItem.Category.set_Visible(view, true);
                                            t.Commit();
                                        }
                                        areCategoryVisible = true;
                                    }
                                    else if (result == TaskDialogResult.CommandLink2)
                                    {
                                        enableRevealHiddenMode = true;
                                    }
                                }
                                else areCategoryVisible = true;
                            }
                            if (areImportCategoriesVisible && areCategoryVisible)
                            {
                                if (element.IsHidden(view))
                                {
                                    var taskDialog = new TaskDialog("CAD менеджер")
                                    {
                                        MainContent = "Обозначение импорта " + _dwgImportsItem.Name +
                                                      " скрыто на виде " + view.Name +
                                                      Environment.NewLine + "Выберите дальнейшее действие:",
                                        CommonButtons = TaskDialogCommonButtons.None
                                    };
                                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                        "Сделать элемент видимым");
                                    taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                        "Включить отображение скрытых элементов");
                                    //taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Ничего не делать");
                                    var result = taskDialog.Show();
                                    if (result == TaskDialogResult.CommandLink1)
                                    {
                                        var lstToShow = new List<ElementId> {_dwgImportsItem.Id};
                                        view.UnhideElements(lstToShow);
                                        areElementVisible = true;
                                    }
                                    else if (result == TaskDialogResult.CommandLink2)
                                    {
                                        enableRevealHiddenMode = true;
                                    }
                                }
                                else areElementVisible = true;
                            }

                            if (areImportCategoriesVisible && areCategoryVisible && areElementVisible)
                            {
                                app.ActiveUIDocument.ActiveView = view;

                                view.DisableTemporaryViewMode(TemporaryViewMode.RevealHiddenElements);
                                var lstToShow = new List<ElementId> {_dwgImportsItem.Id};
                                app.ActiveUIDocument.Selection.SetElementIds(lstToShow);

                                var bb = element.get_BoundingBox(view);
                                app.ActiveUIDocument.GetOpenUIViews()[0].ZoomAndCenterRectangle(bb.Min, bb.Max);
                            }
                            if (enableRevealHiddenMode)
                            {
                                // set view active
                                app.ActiveUIDocument.ActiveView = view;
                                using (Transaction t = new Transaction(doc, "PIK_ChangeView"))
                                {
                                    t.Start();
                                    view.EnableRevealHiddenMode();
                                    t.Commit();
                                }
                            }
                        }
                        else MessageBox.Show("Не удалось получить элемент");
                    }
                    else
                    {
                        var lstToShow = new List<ElementId> {_dwgImportsItem.Id};
                        app.ActiveUIDocument.Selection.SetElementIds(lstToShow);
                        app.ActiveUIDocument.ShowElements(lstToShow);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                if(DWGImportManagerCommand.MainWindow != null)
                    DWGImportManagerCommand.MainWindow.Topmost = true;
            }
        }
        public string GetName()
        {
            return "ChangeView";
        }
    }
}
