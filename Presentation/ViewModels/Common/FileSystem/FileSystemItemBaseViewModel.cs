using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Prism.Mvvm;

using Common.Enums;

using Presentation.Mappers;

namespace Presentation.ViewModels.Common.FileSystem;

public abstract class FileSystemItemBaseViewModel : BindableBase, IDataErrorInfo
{
    #region Constants

    private const string FixErrorsMessageUi = "Виправіть помилки.";

    private const string EmptyNameErrorMessageUi = "Назва не може бути порожньою.";

    private static readonly string MaxNameLengthErrorMessage = $"Назва не може бути більше за {MaxNameLength} символів.";

    private const int MaxNameLength = 164;

    #endregion
    
    #region Private Fields

    private Guid _id = Guid.NewGuid();

    private Guid? _parentId;

    private bool _isNew = true;

    private bool _isExpanded;

    private bool _isSelected;

    private bool _hasChilds;

    private bool _isDummy;

    private string _name = string.Empty;

    private int _order;

    private bool _isMarkedForDelete;

    private bool _isLoading;

    private FileFormat _format = FileFormat.None;

    private MenuType _menuType = MenuType.None;

    private ObservableCollection<FileSystemItemBaseViewModel> _childs = new();

    private FileSystemItemBaseViewModel? _parent;
    
    private bool _isChildsLoaded;
    
    private string _error = string.Empty;

    private readonly Dictionary<string, string> _errors = new();
    
    #endregion
    
    #region Public Fields
    
    /// <summary>
    /// File system item ID.
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    /// <summary>
    /// Parent file system item ID.
    /// </summary>
    public Guid? ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }

    /// <summary>
    /// Indicates is the new or existing.
    /// </summary>
    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
    }

    /// <summary>
    /// Indicates whether the file system item is currently expanded in the UI.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>
    /// Indicates whether the file system item is currently selected in the UI.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Indicates whether the file system item is currently selected in the UI.
    /// </summary>
    public bool HasChilds
    {
        get => _hasChilds;
        set => SetProperty(ref _hasChilds, value);
    }

    /// <summary>
    /// Determines whether the children of this item are fully loaded.
    /// Returns <c>true</c> if the item has no children,
    /// or if the <see cref="Childs"/> collection contains real child items
    /// (i.e., it is not empty and does not contain any dummy placeholders).
    /// </summary>
    public bool IsChildsLoaded
    {
        get => _isChildsLoaded || !HasChilds;
        private set => SetProperty(ref _isChildsLoaded, value);
    }
    
    /// <summary>
    /// Indicates whether the file system item is currently dummy (for expanding in UI)
    /// </summary>
    public bool IsDummy
    {
        get => _isDummy;
        set => SetProperty(ref _isDummy, value);
    }
    
    /// <summary>
    /// Name of file system item.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Order of a file system item in sibling node.
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    /// <summary>
    /// Indicates is the deleted file.
    /// </summary>
    public bool IsMarkedForDelete
    {
        get => _isMarkedForDelete;
        set => SetProperty(ref _isMarkedForDelete, value);
    }

    /// <summary>
    /// Indicates whether the file system item is loading
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Format of file system item.
    /// </summary>
    public FileFormat Format
    {
        get => _format;
        set
        {
            if (!SetProperty(ref _format, value)) return;

            if (string.IsNullOrEmpty(_name))
            {
                Name = FileSystemMapper.FileFormatToNewName(_format);
            }
        }
    }

    /// <summary>
    /// Menu type of file system item.
    /// </summary>
    public MenuType MenuType
    {
        get => _menuType;
        set => SetProperty(ref _menuType, value);
    }

    /// <summary>
    /// Collection of children nodes.
    /// </summary>
    public ObservableCollection<FileSystemItemBaseViewModel> Childs
    {
        get => _childs;
        set => SetProperty(ref _childs, value);
    }

    /// <summary>
    /// Parent item.
    /// </summary>
    public FileSystemItemBaseViewModel? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }
    
    /// <summary>
    /// Gets the current error string.
    /// </summary>
    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    /// <summary>
    /// Gets the number of errors in the error collection.
    /// </summary>
    public int ErrorCount => _errors.Count;

    /// <summary>
    /// Indicates whether the current object is valid (has no errors).
    /// </summary>
    public bool IsValid => ErrorCount == 0;

    /// <summary>
    /// Validates the property with the specified <paramref name="columnName"/>.
    /// Updates the internal error collection and raises property changed notifications
    /// for <see cref="ErrorCount"/> and <see cref="IsValid"/>.
    /// Returns the validation error message for the specified property, or null/empty if valid.
    /// </summary>
    public string this[string columnName]
    {
        get
        {
            var error = OnValidate(columnName);

            if (string.IsNullOrEmpty(error))
                _errors.Remove(columnName);

            else
                _errors[columnName] = error;

            Error = _errors.Count != 0 ? FixErrorsMessageUi : string.Empty;

            RaisePropertyChanged(nameof(ErrorCount));
            RaisePropertyChanged(nameof(IsValid));

            return error;
        }
    }
    
    #endregion
    
    #region Private methods
   
    private string OnValidate(string columnName)
    {
        var result = string.Empty;

        if (columnName is nameof(Name))
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return EmptyNameErrorMessageUi;
            }

            if (Name.Length > MaxNameLength)
            {
                return MaxNameLengthErrorMessage;
            }
        }

        return result;
    }
    
    #endregion

    #region Public methods
    
    internal void SetChildsLoaded(bool value)
    {
        _isChildsLoaded = value;
        RaisePropertyChanged(nameof(IsChildsLoaded));
    }
    
    public List<(Guid Id, Guid? ParentId, int Order)> ReindexChildSiblings()
    {
        var result = new List<(Guid Id, Guid? ParentId, int Order)>();
        var i = 0;
        foreach (var child in Childs.Where(c => !c.IsDummy))
        {
            child.Order = i++;
            result.Add((child.Id, child.ParentId, child.Order));
        }
        
        return result;
    }
    
    #endregion
    
}
