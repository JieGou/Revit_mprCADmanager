﻿#pragma warning disable SA1600 // Elements should be documented
namespace mprCADmanager
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        public string Name => "mprCADmanager";

#if R2015
        public string AvailProductExternalVersion => "2015";
#elif R2016
        public string AvailProductExternalVersion => "2016";
#elif R2017
        public string AvailProductExternalVersion => "2017";
#elif R2018
        public string AvailProductExternalVersion => "2018";
#elif R2019
        public string AvailProductExternalVersion => "2019";
#elif R2020
        public string AvailProductExternalVersion => "2020";
#endif

        public string FullClassName => "mprCADmanager.Commands.DWGImportManagerCommand";

        public string AppFullClassName => string.Empty;

        public Guid AddInId => Guid.Empty;

        public string LName => "CAD менеджер";

        public string Description => "Управление всеми вставками dwg-файлов в текущем документе";

        public string Author => "Пекшев Александр aka Modis";

        public string Price => "0";

        public bool CanAddToRibbon => true;

        public string FullDescription => "Плагин отображает все виды вставленных dwg-файлов – как принадлежащие видам, так и не принадлежащие видам. Имеется возможность поиска в списке, копирования идентификатора dwg-вставки или вида, открытия вида, содержащего dwg-вставку, а также удаление dwg-вставки";

        public string ToolTipHelpImage => string.Empty;

        public List<string> SubFunctionsNames => new List<string>();

        public List<string> SubFunctionsLames => new List<string>();

        public List<string> SubDescriptions => new List<string>();

        public List<string> SubFullDescriptions => new List<string>();

        public List<string> SubHelpImages => new List<string>();

        public List<string> SubClassNames => new List<string>();
    }
}
#pragma warning restore SA1600 // Elements should be documented