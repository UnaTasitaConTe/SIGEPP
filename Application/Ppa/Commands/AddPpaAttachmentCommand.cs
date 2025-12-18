using System.ComponentModel.DataAnnotations;
using Domain.Ppa;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para agregar un anexo a un PPA.
/// NOTA: Este comando solo maneja la metadata del anexo.
/// La subida física del archivo al almacenamiento se maneja en otra capa/servicio.
/// </summary>
public class AddPpaAttachmentCommand
{
    /// <summary>
    /// ID del PPA al que se agregará el anexo.
    /// </summary>
    [Required(ErrorMessage = "El ID del PPA es requerido.")]
    public Guid PpaId { get; set; }

    /// <summary>
    /// Tipo de anexo (documento PPA, autorización, evidencia, etc.).
    /// </summary>
    [Required(ErrorMessage = "El tipo de anexo es requerido.")]
    public PpaAttachmentType Type { get; set; }

    /// <summary>
    /// Nombre amigable del anexo para mostrar al usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre del anexo es requerido.")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 300 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Clave o ruta lógica del archivo en el sistema de almacenamiento.
    /// </summary>
    [Required(ErrorMessage = "La clave del archivo es requerida.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "La clave del archivo debe tener entre 1 y 500 caracteres.")]
    public string FileKey { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME del archivo (opcional).
    /// </summary>
    [StringLength(100, ErrorMessage = "El tipo de contenido no puede exceder los 100 caracteres.")]
    public string? ContentType { get; set; }

    /// <summary>
    /// ID del usuario que sube el archivo.
    /// Este valor lo asignará el controller usando el usuario autenticado.
    /// </summary>
    [Required(ErrorMessage = "El ID del usuario que sube el archivo es requerido.")]
    public Guid UploadedByUserId { get; set; }
}
