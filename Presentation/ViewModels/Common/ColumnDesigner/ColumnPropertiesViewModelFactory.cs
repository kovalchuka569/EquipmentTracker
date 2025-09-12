using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Models.Common.Table.ColumnProperties;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public static class ColumnPropertiesViewModelFactory
{
    public static BaseColumnPropertiesViewModel FromDomain(BaseColumnProperties domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        BaseColumnPropertiesViewModel vm = domain switch
        {
            TextColumnProperties      => new TextColumnPropertiesViewModel(),
            NumberColumnProperties    => new NumberColumnPropertiesViewModel(),
            DateColumnProperties      => new DateColumnPropertiesViewModel(),
            BooleanColumnProperties   => new BooleanColumnPropertiesViewModel(),
            CurrencyColumnProperties  => new CurrencyColumnPropertiesViewModel(),
            ListColumnProperties      => new ListColumnPropertiesViewModel(),
            LinkColumnProperties      => new LinkColumnPropertiesViewModel(),
            _ => throw new NotSupportedException(
                $"Domain type {domain.GetType().Name} is not supported")
        };

        vm.FromDomain(domain);
        return vm;
    }
    
    public static ObservableCollection<BaseColumnPropertiesViewModel> FromDomainMany(
        IEnumerable<BaseColumnProperties> domains)
    {
        return new ObservableCollection<BaseColumnPropertiesViewModel>(
            domains.Select(FromDomain));
    }
    
    public static BaseColumnProperties ToDomain(BaseColumnPropertiesViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return vm switch
        {
            TextColumnPropertiesViewModel      textVm    => textVm.ToDomain(),
            NumberColumnPropertiesViewModel    numberVm  => numberVm.ToDomain(),
            DateColumnPropertiesViewModel      dateVm    => dateVm.ToDomain(),
            BooleanColumnPropertiesViewModel   boolVm    => boolVm.ToDomain(),
            CurrencyColumnPropertiesViewModel  currVm    => currVm.ToDomain(),
            ListColumnPropertiesViewModel      listVm    => listVm.ToDomain(),
            LinkColumnPropertiesViewModel      linkVm    => linkVm.ToDomain(),
            _ => throw new NotSupportedException(
                $"ViewModel type {vm.GetType().Name} is not supported")
        };
    }
    
    public static IEnumerable<BaseColumnProperties> ToDomainMany(
        IEnumerable<BaseColumnPropertiesViewModel> vms)
    {
        return vms.Select(ToDomain);
    }
}