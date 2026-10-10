using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PriceScaleAttribute : ValidationAttribute
    {
        public PriceScaleAttribute() : base("{0} must have at most two decimal places.")
        {
        }

        public override bool IsValid(object value)
        {
            // Compare amounts, so harmless trailing zeros (19.4900) remain valid.
            return value is decimal price && decimal.Round(price, 2) == price;
        }
    }
}
