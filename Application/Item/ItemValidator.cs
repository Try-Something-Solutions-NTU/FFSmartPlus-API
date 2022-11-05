using FluentValidation;
namespace Application.Item;

public class ItemValidator : AbstractValidator<Domain.Item>
{
    public ItemValidator()
    {
        RuleFor(x => x.Name).Length(3, 40);
        RuleFor(x => x.UnitDesc).Length(3, 20);
    }
    
}