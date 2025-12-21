namespace Domain.Ppa;

/// <summary>
/// Tipos de acciones que se registran en el historial de un PPA.
/// </summary>
public enum PpaHistoryActionType
{
    /// <summary>
    /// El PPA fue creado.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Se actualizó el título del PPA.
    /// </summary>
    UpdatedTitle = 1,

    /// <summary>
    /// Se cambió el estado del PPA (Proposal, InProgress, Completed, Archived).
    /// </summary>
    ChangedStatus = 2,

    /// <summary>
    /// Se cambió el docente responsable del PPA.
    /// </summary>
    ChangedResponsibleTeacher = 3,

    /// <summary>
    /// Se actualizaron las asignaciones docente-asignatura asociadas al PPA.
    /// </summary>
    UpdatedAssignments = 4,

    /// <summary>
    /// Se actualizó la lista de estudiantes asociados al PPA.
    /// </summary>
    UpdatedStudents = 5,

    /// <summary>
    /// Se actualizó la configuración de continuidad del PPA
    /// (ContinuationOfPpaId o ContinuedByPpaId).
    /// </summary>
    UpdatedContinuationSettings = 6,

    /// <summary>
    /// Se agregó un anexo al PPA.
    /// </summary>
    AttachmentAdded = 7,

    /// <summary>
    /// Se eliminó un anexo del PPA.
    /// </summary>
    AttachmentRemoved = 8,

    /// <summary>
    /// Se creó un PPA de continuación a partir de este PPA.
    /// </summary>
    ContinuationCreated = 9,

    /// <summary>
    /// Se actualizó el objetivo general del PPA.
    /// </summary>
    UpdatedGeneralObjective = 10,

    /// <summary>
    /// Se actualizaron los objetivos específicos del PPA.
    /// </summary>
    UpdatedSpecificObjectives = 11,

    /// <summary>
    /// Se actualizó la descripción del PPA.
    /// </summary>
    UpdatedDescription = 12
}
