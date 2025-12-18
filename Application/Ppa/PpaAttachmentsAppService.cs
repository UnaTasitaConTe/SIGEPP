using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Domain.Ppa.Entities;
using Domain.Ppa.Repositories;
using Domain.Users;

namespace Application.Ppa;

/// <summary>
/// Servicio de aplicación para gestión de anexos de PPA en SIGEPP.
/// Orquesta los casos de uso relacionados con archivos adjuntos a PPAs.
/// </summary>
public sealed class PpaAttachmentsAppService
{
    private readonly IPpaAttachmentRepository _ppaAttachmentRepository;
    private readonly IPpaRepository _ppaRepository;
    private readonly IUserRepository _userRepository;

    public PpaAttachmentsAppService(
        IPpaAttachmentRepository ppaAttachmentRepository,
        IPpaRepository ppaRepository,
        IUserRepository userRepository)
    {
        _ppaAttachmentRepository = ppaAttachmentRepository ?? throw new ArgumentNullException(nameof(ppaAttachmentRepository));
        _ppaRepository = ppaRepository ?? throw new ArgumentNullException(nameof(ppaRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Obtiene todos los anexos de un PPA (excluyendo los eliminados).
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de anexos del PPA.</returns>
    /// <exception cref="InvalidOperationException">Si el PPA no existe.</exception>
    public async Task<IReadOnlyCollection<PpaAttachmentDto>> GetByPpaAsync(
        Guid ppaId,
        CancellationToken ct = default)
    {
        // Validar que el PPA exista
        var ppa = await _ppaRepository.GetByIdAsync(ppaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{ppaId}' no encontrado.");

        var attachments = await _ppaAttachmentRepository.GetByPpaAsync(ppaId, includeDeleted: false, ct);
        return attachments.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los anexos de un PPA filtrados por tipo.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="type">Tipo de anexo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de anexos del PPA del tipo especificado.</returns>
    /// <exception cref="InvalidOperationException">Si el PPA no existe.</exception>
    public async Task<IReadOnlyCollection<PpaAttachmentDto>> GetByPpaAndTypeAsync(
        Guid ppaId,
        Domain.Ppa.PpaAttachmentType type,
        CancellationToken ct = default)
    {
        // Validar que el PPA exista
        var ppa = await _ppaRepository.GetByIdAsync(ppaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{ppaId}' no encontrado.");

        var attachments = await _ppaAttachmentRepository.GetByPpaAndTypeAsync(
            ppaId, type, includeDeleted: false, ct);

        return attachments.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Agrega un nuevo anexo a un PPA.
    /// NOTA: Este método solo maneja la metadata del anexo.
    /// La subida física del archivo al almacenamiento se debe manejar en otra capa/servicio.
    /// </summary>
    /// <param name="command">Comando con los datos del anexo a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del anexo creado.</returns>
    /// <exception cref="InvalidOperationException">Si el PPA o el usuario no existen.</exception>
    public async Task<Guid> AddAsync(AddPpaAttachmentCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Validar que el PPA exista
        var ppa = await _ppaRepository.GetByIdAsync(command.PpaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.PpaId}' no encontrado.");

        // 2. Validar que el usuario exista
        var user = await _userRepository.GetByIdAsync(command.UploadedByUserId, ct);
        if (user == null)
            throw new InvalidOperationException($"Usuario con ID '{command.UploadedByUserId}' no encontrado.");

        // 3. Crear la entidad PpaAttachment usando el método de fábrica del dominio
        var attachment = PpaAttachment.Create(
            ppaId: command.PpaId,
            type: command.Type,
            name: command.Name,
            fileKey: command.FileKey,
            uploadedByUserId: command.UploadedByUserId,
            contentType: command.ContentType);

        // 4. Guardar el anexo
        await _ppaAttachmentRepository.AddAsync(attachment, ct);

        return attachment.Id;
    }

    /// <summary>
    /// Elimina lógicamente un anexo de PPA (soft delete).
    /// </summary>
    /// <param name="attachmentId">ID del anexo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el anexo no existe.</exception>
    public async Task DeleteAsync(Guid attachmentId, CancellationToken ct = default)
    {
        // Obtener el anexo
        var attachment = await _ppaAttachmentRepository.GetByIdAsync(attachmentId, ct);
        if (attachment == null)
            throw new InvalidOperationException($"Anexo con ID '{attachmentId}' no encontrado.");

        // TODO: Validación futura - No permitir eliminar el último PpaDocument si el PPA está Completed
        // var ppa = await _ppaRepository.GetByIdAsync(attachment.PpaId, ct);
        // if (ppa != null && ppa.Status == PpaStatus.Completed && attachment.Type == PpaAttachmentType.PpaDocument)
        // {
        //     var documentCount = await _ppaAttachmentRepository.CountByTypeAsync(
        //         attachment.PpaId, PpaAttachmentType.PpaDocument, includeDeleted: false, ct);
        //     if (documentCount <= 1)
        //         throw new InvalidOperationException(
        //             "No se puede eliminar el último documento formal (PpaDocument) de un PPA Completed.");
        // }

        // Marcar como eliminado
        attachment.MarkAsDeleted(DateTime.UtcNow);

        // Guardar cambios
        await _ppaAttachmentRepository.UpdateAsync(attachment, ct);
    }

    /// <summary>
    /// Mapea una entidad de dominio PpaAttachment a un DTO.
    /// </summary>
    private static PpaAttachmentDto ToDto(PpaAttachment attachment)
    {
        return new PpaAttachmentDto
        {
            Id = attachment.Id,
            PpaId = attachment.PpaId,
            Type = attachment.Type,
            Name = attachment.Name,
            FileKey = attachment.FileKey,
            ContentType = attachment.ContentType,
            UploadedByUserId = attachment.UploadedByUserId,
            UploadedAt = attachment.UploadedAt,
            IsDeleted = attachment.IsDeleted
        };
    }
}
