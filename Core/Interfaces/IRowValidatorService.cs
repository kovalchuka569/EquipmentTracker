using Core.Models;

namespace Core.Interfaces;

public interface IRowValidatorService
{
    RowValidationResult ValidateRow(RowValidationArgs args);
}