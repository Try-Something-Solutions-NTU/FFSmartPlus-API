using FluentValidation;
namespace Application.Supplier;

public class SupplierValidator : AbstractValidator<Domain.Supplier>
{
    public SupplierValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        
    }
}