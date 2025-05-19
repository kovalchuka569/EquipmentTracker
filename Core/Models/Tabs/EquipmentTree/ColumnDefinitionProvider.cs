using System.Collections.ObjectModel;

namespace Core.Models.Tabs.EquipmentTree
{
    public static class ColumnDefinitionProvider
    {
        public static ObservableCollection<Column> GetColumns()
        {
            return new ObservableCollection<Column>
            {
                new() { ColumnName = "Інвентарний номер", ColumnType = "TEXT", Category = "Основні характеристики", IsMandatory = true },
                new() { ColumnName = "Бренд", ColumnType = "VARCHAR(255)", Category = "Основні характеристики", IsMandatory = true },
                new() { ColumnName = "Модель", ColumnType = "VARCHAR(255)", Category = "Основні характеристики", IsMandatory = true },
                new() { ColumnName = "Серійний номер", ColumnType = "TEXT", Category = "Основні характеристики" },
                new() { ColumnName = "Клас", ColumnType = "VARCHAR(255)", Category = "Основні характеристики" },
                new() { ColumnName = "Рік", ColumnType = "INTEGER", Category = "Основні характеристики" },

                new() { ColumnName = "Розмір (см)", ColumnType = "DECIMAL(10,2)", Category = "Фізичні характеристики" },
                new() { ColumnName = "Вага (кг)", ColumnType = "DECIMAL(10,2)", Category = "Фізичні характеристики" },

                new() { ColumnName = "Поверх", ColumnType = "VARCHAR(255)", Category = "Локація" },
                new() { ColumnName = "Відділ", ColumnType = "VARCHAR(255)", Category = "Локація" },
                new() { ColumnName = "Кімната", ColumnType = "VARCHAR(255)", Category = "Локація" },

                new() { ColumnName = "Споживання (кв/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики" },
                new() { ColumnName = "Напруга (В)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики" },
                new() { ColumnName = "Вода (л/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики" },
                new() { ColumnName = "Повітря (л/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики" },

                new() { ColumnName = "Балансова вартість (грн)", ColumnType = "DECIMAL(10,2)", Category = "Фінансові характеристики" },

                new() { ColumnName = "Нотатки", ColumnType = "TEXT", Category = "Інше" },
                new() { ColumnName = "Відповідальний", ColumnType = "TEXT", Category = "Інше" },
                new() { ColumnName = "У використанні", ColumnType = "BOOLEAN DEFAULT TRUE", Category = "Інше" }
            };
        }
    }
}

