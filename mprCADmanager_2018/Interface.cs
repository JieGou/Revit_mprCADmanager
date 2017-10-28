using System;
using System.Collections.Generic;
using ModPlusAPI.Interfaces;

namespace mprCADmanager
{
    public class Interface : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;
        public string Name => "mprCADmanager";
        public string AvailProductExternalVersion => "2018";
        public string FullClassName => "mprCADmanager.Commands.DWGImportManagerCommand";
        public string AppFullClassName => string.Empty;
        public Guid AddInId => Guid.Empty;
        public string LName => "CAD менеджер";
        public string Description => "Управление всеми вставками dwg-файлов в текущем документе";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
        public bool CanAddToRibbon => true;
        public string FullDescription => "Функция отображает все виды вставленных dwg-файлов – как принадлежащие видам, так и не принадлежащие видам. Имеется возможность поиска в списке, копирования идентификатора dwg-вставки или вида, открытия вида, содержащего dwg-вставку, а также удаление dwg-вставки";
        public string ToolTipHelpImage => string.Empty;
        public List<string> SubFunctionsNames => new List<string>();
        public List<string> SubFunctionsLames => new List<string>();
        public List<string> SubDescriptions => new List<string>();
        public List<string> SubFullDescriptions => new List<string>();
        public List<string> SubHelpImages => new List<string>();
        public List<string> SubClassNames => new List<string>();
    }
}
