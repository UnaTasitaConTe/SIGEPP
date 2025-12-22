namespace Domain.Ppa;

/// <summary>
/// Estados del ciclo de vida de un PPA (Proyecto Pedagógico de Aula).
/// </summary>
public enum PpaStatus
{
    /// <summary>
    /// Propuesta inicial. El PPA ha sido registrado pero aún no se ha iniciado formalmente.
    /// </summary>
    Proposal = 0,

    /// <summary>
    /// En desarrollo. El PPA está siendo ejecutado activamente.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Finalizado. El PPA ha concluido y ha cumplido sus objetivos.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Archivado. El PPA ha sido archivado y ya no está en gestión activa.
    /// Este es un estado terminal del que no se puede salir.
    /// </summary>
    Archived = 3,
    /// <summary>
    /// En continuación. El PPA ha sido Continuado.
    /// Este es un estado terminal del que no se puede salir.
    /// </summary>
    InContinuing = 4,
}
