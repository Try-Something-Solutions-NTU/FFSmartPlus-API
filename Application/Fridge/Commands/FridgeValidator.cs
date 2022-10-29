using FluentValidation;

namespace Application.Fridge.Commands;

public class FridgeValidator : AbstractValidator<Domain.Fridge>
{
    public FridgeValidator()
    {
        RuleFor(x => x.ManufacturerId).Length(10, 10);
    }
    
}