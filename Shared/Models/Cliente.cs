using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Nexo.Shared.Models
{
    public class Cliente : IValidatableObject
    {
        private const string SoloNumerosPattern = @"^\d+$";
        private const string CuitPattern = @"^\d{2}-\d{8}-\d{1}$";

        public int Id { get; set; }

        [Required]
        public TipoCliente Tipo { get; set; }

        // Persona física
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Apellido { get; set; }

        [StringLength(20)]
        public string Dni { get; set; }

        // Empresa
        [StringLength(100)]
        public string RazonSocial { get; set; }

        // Datos fiscales (ambos tipos)
        [StringLength(20)]
        public string Cuit { get; set; }

        [StringLength(50)]
        public string CondicionFiscal { get; set; }

        // Contacto (ambos tipos)
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Direccion { get; set; }

        [StringLength(60)]
        public string Ciudad { get; set; }

        [StringLength(200)]
        public string ComoLlego { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        public string NombreCompleto =>
            Tipo == TipoCliente.Empresa ? RazonSocial : $"{Nombre} {Apellido}".Trim();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tipo == TipoCliente.Persona)
            {
                if (string.IsNullOrWhiteSpace(Nombre))
                    yield return new ValidationResult("El nombre es obligatorio para una persona.", new[] { nameof(Nombre) });

                if (string.IsNullOrWhiteSpace(Apellido))
                    yield return new ValidationResult("El apellido es obligatorio para una persona.", new[] { nameof(Apellido) });

                if (string.IsNullOrWhiteSpace(Dni))
                    yield return new ValidationResult("El DNI es obligatorio para una persona.", new[] { nameof(Dni) });
                else if (!Regex.IsMatch(Dni, SoloNumerosPattern))
                    yield return new ValidationResult("El DNI debe contener solo números.", new[] { nameof(Dni) });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(RazonSocial))
                    yield return new ValidationResult("La razón social es obligatoria para una empresa.", new[] { nameof(RazonSocial) });

                if (string.IsNullOrWhiteSpace(Cuit))
                    yield return new ValidationResult("El CUIT es obligatorio para una empresa.", new[] { nameof(Cuit) });
            }

            if (!string.IsNullOrWhiteSpace(Cuit) && !Regex.IsMatch(Cuit, CuitPattern))
                yield return new ValidationResult("El CUIL/CUIT debe tener el formato xx-xxxxxxxx-x.", new[] { nameof(Cuit) });

            if (string.IsNullOrWhiteSpace(CondicionFiscal))
                yield return new ValidationResult("La condición fiscal es obligatoria.", new[] { nameof(CondicionFiscal) });

            if (string.IsNullOrWhiteSpace(Telefono))
                yield return new ValidationResult("El teléfono es obligatorio.", new[] { nameof(Telefono) });
            else if (!Regex.IsMatch(Telefono, SoloNumerosPattern))
                yield return new ValidationResult("El teléfono debe contener solo números.", new[] { nameof(Telefono) });

            if (string.IsNullOrWhiteSpace(Direccion))
                yield return new ValidationResult("La dirección es obligatoria.", new[] { nameof(Direccion) });

            if (string.IsNullOrWhiteSpace(Ciudad))
                yield return new ValidationResult("La ciudad es obligatoria.", new[] { nameof(Ciudad) });
        }
    }
}
