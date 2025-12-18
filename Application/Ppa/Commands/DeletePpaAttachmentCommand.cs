using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para eliminar (soft delete) un anexo de PPA.
/// </summary>
public class DeletePpaAttachmentCommand
{
    /// <summary>
    /// ID del anexo a eliminar.
    /// </summary>
    [Required(ErrorMessage = "El ID del anexo es requerido.")]
    public Guid AttachmentId { get; set; }
}
