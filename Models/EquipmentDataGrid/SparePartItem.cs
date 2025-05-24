using Prism.Mvvm;

namespace Models.EquipmentDataGrid;

public class SparePartItem : BindableBase
{
    private int _id;
    private int _equipmentId;
    private string _sparePartName;
    private string _sparePartCategory;
    private string _sparePartSerialNumber;
    private decimal _sparePartQuantity;
    private string _sparePartUnit;  
    private string _sparePartNotes;

    public int EquipmentId
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    public string SparePartName
    {
        get => _sparePartName;
        set => SetProperty(ref _sparePartName, value);
    }
    public string SparePartCategory
    {
        get => _sparePartCategory;
        set => SetProperty(ref _sparePartCategory, value);
    }
    public string SparePartSerialNumber
    {
        get => _sparePartSerialNumber;
        set => SetProperty(ref _sparePartSerialNumber, value);
    }
    public decimal SparePartQuantity
    {
        get => _sparePartQuantity;
        set => SetProperty(ref _sparePartQuantity, value);
    }
    public string SparePartUnit
    {
        get => _sparePartUnit;
        set => SetProperty(ref _sparePartUnit, value);
    }
    public string SparePartNotes
    {
        get => _sparePartNotes;
        set => SetProperty(ref _sparePartNotes, value);
    }

    public string SparePartNameDisplay => SparePartName;
    public string SparePartCategoryDisplay => SparePartCategory;
    public string SparePartSerialNumberDisplay => SparePartSerialNumber;
    public string SparePartQuantityDisplay => SparePartQuantity.ToString();
    public string SparePartUnitDisplay => SparePartUnit;
    public string SparePartNotesDisplay => SparePartNotes;
}