using Presentation.Helpers;
using Models.Equipment;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class ColumnDesignerConstants
{
    
    public const int MaxHeaderTextLength = 64;
    
    public const string HeaderTextEmptyErrorMessageUi = "Заголовок не може бути порожнім.";

    public static readonly string HeaderTextLengthErrorMessageUi = $"Максимальна довжина заголовку - {PluralizedHelper
        .GetPluralizedText(MaxHeaderTextLength, "символ", "символи", "символів")}";

    public const string MinGreaterMaxErrorMessageUi = "Мінімальне значення не може бути більшим за максимальне.";

    public const string MaxLessMinErrorMessageUi = "Максимальне значення не може бути меншим за мінімальне.";

    public const string MinHeaderWidthErrorMessageUi = "Ширина стовпця не може бути меншим або дорівнювати 0";
    
    public const string EmptyDefaultValueErrorMessageUi = "Значення за замовчуванням не може бути порожнім.";

    public static string DefaultNumberValueGreaterMaxValueErrorMessageUi(double maxValue, long decimalDigits)
    {
        return $"Введене значення більше за максимальне значення. Максимальне значення - {maxValue.ToString($"F{decimalDigits}")}";
    }
    
    public static string DefaultNumberValueLessMinValueErrorMessageUi(double minValue, long decimalDigits)
    {
        return $"Введене значення менше за мінімальне значення. Мінімальне значення - {minValue.ToString($"F{decimalDigits}")}";
    }

    public const string ShowListValuesButtonText = "Показати список значень";

    public const string LinkDefaultValue = "https://www.google.com/";
    
    public const string TextDefaultValue = "Введіть значення";

    #region Default properties

    public const string DefaultHeaderText = "Стовпець";

    public const double DefaultColumnWidth = 150.00;

    public const ColumnDataType DefaultColumnDataType = ColumnDataType.Text;

    public const double DefaultMinNumberValue = 0.00;

    public const double DefaultMaxNumberValue = 9999999.00;

    public const int DefaultNumberDecimalDigits = 2;
    
    public const bool DefaultIsUnique = false;
    
    public const bool DefaultIsRequired = false;
    
    public const bool DefaultIsFrozen = false;
    

    #endregion

    #region Column properties displayed names

    public const string HeaderTextDisplayedUiName = "Заголовок";

    public const string DefaultValueDisplayedUiName = "Значення за замовчуванням";

    public const string IsFrozenDisplayedUiName = "Закріпити";

    public const string WidthDisplayedUiName = "Ширина";

    public const string IsUniqueDisplayedUiName = "Має унікальні значення";

    public const string IsRequiredDisplayedUiName = "Обов'язкове для заповнення";

    public const string MinNumberValueUiName = "Мінімальне значення";

    public const string MaxNumberValueUiName = "Максимальне значення";

    public const string NumberDecimalDigitsUiName = "Значень після коми";

    public const string DatePatternUiName = "Формат дати";

    public const string CurrencySymbolUiName = "Символ валюти";

    public const string ListValuesUiName = "Список значень";

    #endregion

    #region Descriptions for column properties

    public const string HeaderTextPropertyUiDescription = "Заголовок стовпця, який видно користувачу у таблиці.";

    public const string DefaultValuePropertyUiDescription = "Визначає значення яке автоматочино підставиться в комірку цього стовпця при створенні нового запису.";

    public const string IsFrozenPropertyUiDescription = "Визначає чи буде цей стовпець закріпленим на початку.";

    public const string WidthPropertyUiDescription = "Визначає, скільки місця відводиться під дані.";

    public const string IsUniquePropertyUiDescription = "Визначає чи всі значення для цього стовпця мають бути унікальними.";

    public const string IsRequiredPropertyUiDescription = "Визначає чи повинні бути обов'язковими для заповнення комірки цього стовпця.";

    public const string MinNumberValuePropertyUiDescription = "Визначає правило вводу мінімального числа для цього стовпця.";

    public const string MaxNumberValuePropertyUiDescription = "Визначає правило вводу максимального числа для цього стовпця.";

    public const string NumberDecimalDigitsPropertyUiDescription = "Визначає скільки чисел буде після коми.";

    public const string DatePatternUiDescription = "Визначає формат відображення дати.";

    public const string CurrencySymbolUiDescription = "Визначає символ валюти.";

    public const string ListValueUiDescription = "Визначає список доступних значень довідника.";
    
    #endregion

    #region Properties group names
    
    public const string GeneralPropertiesGroupName = "Загальні параметри";

    public const string ValidatePropertiesGroupName = "Валідаційні параметри";
    
    #endregion

    public const string ColumnDataTypeTextDisplay = "Текст";

    public const string ColumnDataTypeNumberDisplay = "Число ціле/дробове";

    public const string ColumnDataTypeDateDisplay = "Дата";

    public const string ColumnDataTypeBooleanDisplay = "Логічне значення";

    public const string ColumnDataTypeListDisplay = "Довідник";

    public const string ColumnDataTypeHyperlinkDisplay = "Гіперпосилання";

    public const string ColumnDataTypeCurrencyDisplay = "Валюта";
    
}