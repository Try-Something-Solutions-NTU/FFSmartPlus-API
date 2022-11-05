using FluentValidation;

namespace Application.Fridge;

public class UnitValidator : AbstractValidator<Domain.Unit>
{
    public UnitValidator()
    {
        RuleFor(x => x.Quantity).LessThan(100);
    }
    
}