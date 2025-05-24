namespace Models.EquipmentDataGrid;

public static class EquipmentColumnInfo
{
    public static readonly Dictionary<string, string> MappingToDbColumn = new()
    {
        { "InventoryNumber", "Інвентарний номер" },
        { "Brand", "Бренд" },
        { "Model", "Модель" },
        { "Category", "Категорія" },
        { "SerialNumber", "Серійний номер" },
        { "Class", "Клас" },
        { "Year", "Рік" },

        { "Height", "Висота (см)" },
        { "Width", "Ширина (см)" },
        { "Length", "Довжина (см)" },
        { "Weight", "Вага (кг)" },

        { "Floor", "Поверх" },
        { "Department", "Відділ" },
        { "Room", "Кімната" },

        { "Consumption", "Споживання (кв/год)" },
        { "Voltage", "Напруга (В)" },
        { "Water", "Вода (л/год)" },
        { "Air", "Повітря (л/год)" },

        { "BalanceCost", "Балансова вартість (грн)" },

        { "Notes", "Нотатки" },
        { "ResponsiblePerson", "Відповідальний" }
    };
}