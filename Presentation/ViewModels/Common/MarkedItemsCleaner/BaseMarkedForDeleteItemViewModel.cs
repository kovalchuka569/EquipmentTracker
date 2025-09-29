using System;
using System.Collections.ObjectModel;
using Common.Enums;
using Common.Logging;
using Notification.Wpf;
using Prism.Mvvm;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public abstract class BaseMarkedForDeleteItemViewModel : BindableBase
{
    #region Contracts
    public abstract MarkedForDeleteItemType MarkedForDeleteItemType { get; }
    
    #endregion
    
    #region Private fields
    
    private ObservableCollection<BaseMarkedForDeleteItemViewModel> _markedForDeleteItemViewModels = new();
    private string _title = string.Empty;
    private bool _isSelected;
    private Guid _id = Guid.Empty;
    private Guid? _parentId;
    
    #endregion
    
    #region Public fields
    
    public virtual ObservableCollection<BaseMarkedForDeleteItemViewModel> Childs
    {
        get => _markedForDeleteItemViewModels;
        set => SetProperty(ref _markedForDeleteItemViewModels, value);
    }

    public virtual string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public virtual bool IsSelectedForDelete
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public virtual Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public virtual Guid? ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }
    
    #endregion
    
    #region Constructor
    
    protected  BaseMarkedForDeleteItemViewModel()
    {
    }
    
    #endregion

    public void SetTitle(string title)
    {
        Title = title;
    }

    public void AddChild(BaseMarkedForDeleteItemViewModel child)
    {
        Childs.Add(child);
    }

    public void RemoveChild(BaseMarkedForDeleteItemViewModel child)
    {
        Childs.Remove(child);
    }
}