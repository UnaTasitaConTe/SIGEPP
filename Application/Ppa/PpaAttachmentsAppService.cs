using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Application.Security;
using Domain.Academics.Repositories;
using Domain.Dictionaries;
using Domain.Ppa;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IPpaHistoryRepository _historyRepository;
    private readonly ITeacherAssignmentRepository _teacherAssignmentRepository;

    public PpaAttachmentsAppService(
        IPpaAttachmentRepository ppaAttachmentRepository,
        IPpaRepository ppaRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPpaHistoryRepository historyRepository,
        ITeacherAssignmentRepository teacherAssignmentRepository)
    {
        _ppaAttachmentRepository = ppaAttachmentRepository ?? throw new ArgumentNullException(nameof(ppaAttachmentRepository));
        _ppaRepository = ppaRepository ?? throw new ArgumentNullException(nameof(ppaRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _teacherAssignmentRepository = teacherAssignmentRepository ?? throw new ArgumentNullException(nameof(teacherAssignmentRepository));
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
    /// <exception cref="InvalidOperationException">
    /// Si el PPA no existe, el usuario no tiene permisos, o el usuario no existe.
    /// </exception>
    public async Task<Guid> AddAsync(AddPpaAttachmentCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener usuario autenticado
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        var currentUserId = currentUser.UserId;

        // 2. Validar que el PPA exista
        var ppa = await _ppaRepository.GetByIdAsync(command.PpaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.PpaId}' no encontrado.");

        // 3. Validar permisos: ADMIN, responsable, o docente con asignaciones en el PPA
        var hasPermission = await ValidateUserHasAccessToPpaAsync(currentUser, ppa, ct);
        if (!hasPermission)
            throw new InvalidOperationException(
                "No tiene permisos para agregar anexos a este PPA. " +
                "Solo el docente responsable, docentes asignados al PPA, o administradores pueden agregar anexos.");

        // 4. Validar que el usuario exista
        var user = await _userRepository.GetByIdAsync(command.UploadedByUserId, ct);
        if (user == null)
            throw new InvalidOperationException($"Usuario con ID '{command.UploadedByUserId}' no encontrado.");

        // 5. Crear la entidad PpaAttachment usando el método de fábrica del dominio
        var attachment = PpaAttachment.Create(
            ppaId: command.PpaId,
            type: command.Type,
            name: command.Name,
            fileKey: command.FileKey,
            uploadedByUserId: command.UploadedByUserId,
            contentType: command.ContentType);

        // 6. Guardar el anexo
        await _ppaAttachmentRepository.AddAsync(attachment, ct);

        // 7. Registrar historial: AttachmentAdded
        var historyEntry = PpaHistoryEntry.Create(
            ppaId: ppa.Id,
            performedByUserId: currentUserId,
            actionType:  PpaHistoryActionType.AttachmentAdded,
            oldValue: null,
            newValue: $"Tipo: {PpaAttachmentTypeTranslations.Spanish[attachment.Type]}, Nombre: {attachment.Name}",
            notes: $"Archivo agregado. FileKey: {attachment.FileKey}, ContentType: {attachment.ContentType ?? "N/A"}");

        await _historyRepository.AddAsync(historyEntry, ct);

        return attachment.Id;
    }

    /// <summary>
    /// Elimina lógicamente un anexo de PPA (soft delete).
    /// </summary>
    /// <param name="attachmentId">ID del anexo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">
    /// Si el anexo no existe o el usuario no tiene permisos.
    /// </exception>
    public async Task DeleteAsync(Guid attachmentId, CancellationToken ct = default)
    {
        // 1. Obtener usuario autenticado
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        var currentUserId = currentUser.UserId;

        // 2. Obtener el anexo
        var attachment = await _ppaAttachmentRepository.GetByIdAsync(attachmentId, ct);
        if (attachment == null)
            throw new InvalidOperationException($"Anexo con ID '{attachmentId}' no encontrado.");

        // 3. Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(attachment.PpaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{attachment.PpaId}' no encontrado.");

        // 4. Validar permisos: ADMIN, responsable, o docente con asignaciones en el PPA
        var hasPermission = await ValidateUserHasAccessToPpaAsync(currentUser, ppa, ct);
        if (!hasPermission)
            throw new InvalidOperationException(
                "No tiene permisos para eliminar anexos de este PPA. " +
                "Solo el docente responsable, docentes asignados al PPA, o administradores pueden eliminar anexos.");

        // TODO: Validación futura - No permitir eliminar el último PpaDocument si el PPA está Completed
        // if (ppa.Status == PpaStatus.Completed && attachment.Type == PpaAttachmentType.PpaDocument)
        // {
        //     var documentCount = await _ppaAttachmentRepository.CountByTypeAsync(
        //         attachment.PpaId, PpaAttachmentType.PpaDocument, includeDeleted: false, ct);
        //     if (documentCount <= 1)
        //         throw new InvalidOperationException(
        //             "No se puede eliminar el último documento formal (PpaDocument) de un PPA Completed.");
        // }

        // 5. Marcar como eliminado
        attachment.MarkAsDeleted(DateTime.UtcNow);

        // 6. Guardar cambios
        await _ppaAttachmentRepository.UpdateAsync(attachment, ct);

        // 7. Registrar historial: AttachmentRemoved
        var historyEntry = PpaHistoryEntry.Create(
            ppaId: ppa.Id,
            performedByUserId: currentUserId,
            actionType: PpaHistoryActionType.AttachmentRemoved,
            oldValue: $"Tipo: {PpaAttachmentTypeTranslations.Spanish[attachment.Type]}, Nombre: {attachment.Name}",
            newValue: null,
            notes: $"Archivo eliminado. FileKey: {attachment.FileKey}");

        await _historyRepository.AddAsync(historyEntry, ct);
    }

    /// <summary>
    /// Valida si un usuario tiene acceso a un PPA para realizar operaciones.
    /// </summary>
    /// <param name="currentUser">Usuario autenticado.</param>
    /// <param name="ppa">PPA a validar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el usuario tiene acceso, false en caso contrario.</returns>
    /// <remarks>
    /// Un usuario tiene acceso si:
    /// - Es ADMIN, o
    /// - Es el docente responsable del PPA, o
    /// - Tiene asignaciones docente-asignatura asociadas al PPA.
    /// </remarks>
    private async Task<bool> ValidateUserHasAccessToPpaAsync(
        CurrentUser currentUser,
        Domain.Ppa.Entities.Ppa ppa,
        CancellationToken ct)
    {
        // ADMIN tiene acceso total
        if (currentUser.Roles.Contains("ADMIN"))
            return true;

        // Docente responsable tiene acceso
        if (ppa.ResponsibleTeacherId == currentUser.UserId)
            return true;

        // Verificar si el docente tiene asignaciones en este PPA
        var teacherAssignments = await _teacherAssignmentRepository.GetByTeacherAsync(currentUser.UserId, ct);

        // Verificar si alguna de las asignaciones del docente está en el PPA
        var hasAssignment = ppa.TeacherAssignmentIds.Any(assignmentId =>
            teacherAssignments.Any(ta => ta.Id == assignmentId));

        return hasAssignment;
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
