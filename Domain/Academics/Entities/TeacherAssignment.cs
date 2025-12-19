namespace Domain.Academics.Entities;

/// <summary>
/// Entity TeacherAssignment - Representa la asignación de un docente a una asignatura en un período académico.
/// Aggregate Root que gestiona la relación entre docentes, asignaturas y períodos académicos.
/// Esta entidad es clave para determinar la carga académica de un docente y filtrar sus PPAs.
/// </summary>
public sealed class TeacherAssignment
{
    // Constructor privado para EF Core
    private TeacherAssignment() { }

    /// <summary>
    /// Constructor privado para crear una nueva asignación docente.
    /// </summary>
    private TeacherAssignment(
        Guid id,
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        bool isActive,
        DateTime createdAt,
        string? teacherName,
        string? subjectCode,
        string? subjectName,
        string? academicPeriodCode,
        string? academicPeriodName
        )
    {
        if (teacherId == Guid.Empty)
            throw new ArgumentException("El ID del docente no puede ser vacío.", nameof(teacherId));

        if (subjectId == Guid.Empty)
            throw new ArgumentException("El ID de la asignatura no puede ser vacío.", nameof(subjectId));

        if (academicPeriodId == Guid.Empty)
            throw new ArgumentException("El ID del período académico no puede ser vacío.", nameof(academicPeriodId));

        Id = id;
        TeacherId = teacherId;
        SubjectId = subjectId;
        AcademicPeriodId = academicPeriodId;
        SubjectName = subjectName;
        SubjectCode = subjectCode;
        TeacherName = teacherName;
        AcademicPeriodCode = academicPeriodCode;
        AcademicPeriodName = academicPeriodName;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;

    }

    /// <summary>
    /// Identificador único de la asignación.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID del docente (User con rol DOCENTE).
    /// </summary>
    public Guid TeacherId { get; private set; }

    /// <summary>
    /// ID de la asignatura asignada.
    /// </summary>
    public Guid SubjectId { get; private set; }

    /// <summary>
    /// ID del período académico en el que se realiza la asignación.
    /// </summary>
    public Guid AcademicPeriodId { get; private set; }

    /// <summary>
    /// Indica si la asignación está activa.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación de la asignación.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización de la asignación.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    public string? TeacherName { get; private set; }
    public string? SubjectCode { get; private set; }
    public string? SubjectName { get; private set; }
    public string? AcademicPeriodCode { get; private set; }
    public string? AcademicPeriodName { get; private set; }

    /// <summary>
    /// Crea una nueva asignación docente.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="isActive">Indica si la asignación está activa (por defecto: true).</param>
    /// <returns>Nueva instancia de TeacherAssignment.</returns>
    public static TeacherAssignment Create(
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        string teacherName,
        string subjectCode,
        string subjectName,
        string academicPeriodCode,
        string academicPeriodName,
        bool isActive = true
        )
    {
        return new TeacherAssignment(
            Guid.NewGuid(),
            teacherId,
            subjectId,
            academicPeriodId,
            isActive,
            DateTime.UtcNow,
            teacherName,
            subjectCode,
            subjectName,
            academicPeriodCode,
            academicPeriodName
            );
    }

    /// <summary>
    /// Crea una asignación docente con un ID específico (útil para migraciones o seeds).
    /// </summary>
    /// <param name="id">ID específico de la asignación.</param>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="isActive">Indica si la asignación está activa.</param>
    /// <returns>Nueva instancia de TeacherAssignment con el ID especificado.</returns>
    public static TeacherAssignment CreateWithId(
        Guid id,
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        string? teacherName,
        string? subjectCode,
        string? subjectName,
        string? academicPeriodCode,
        string? academicPeriodName,
        bool isActive = true)
    {
        return new TeacherAssignment(
            id,
            teacherId,
            subjectId,
            academicPeriodId,
            isActive,
            DateTime.UtcNow,
            teacherName,
            subjectCode,
            subjectName,
            academicPeriodCode,
            academicPeriodName
            );
    }

    /// <summary>
    /// Activa la asignación docente.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva la asignación docente.
    /// Útil cuando un docente deja de dictar una asignatura en un período.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reasigna la asignatura a otro docente.
    /// </summary>
    /// <param name="newTeacherId">ID del nuevo docente.</param>
    public void ReassignTeacher(Guid newTeacherId)
    {
        if (newTeacherId == Guid.Empty)
            throw new ArgumentException("El ID del nuevo docente no puede ser vacío.", nameof(newTeacherId));

        if (TeacherId == newTeacherId)
            return;

        TeacherId = newTeacherId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia la asignatura de la asignación.
    /// </summary>
    /// <param name="newSubjectId">ID de la nueva asignatura.</param>
    public void ChangeSubject(Guid newSubjectId)
    {
        if (newSubjectId == Guid.Empty)
            throw new ArgumentException("El ID de la nueva asignatura no puede ser vacío.", nameof(newSubjectId));

        if (SubjectId == newSubjectId)
            return;

        SubjectId = newSubjectId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el período académico de la asignación.
    /// </summary>
    /// <param name="newAcademicPeriodId">ID del nuevo período académico.</param>
    public void ChangeAcademicPeriod(Guid newAcademicPeriodId)
    {
        if (newAcademicPeriodId == Guid.Empty)
            throw new ArgumentException("El ID del nuevo período académico no puede ser vacío.", nameof(newAcademicPeriodId));

        if (AcademicPeriodId == newAcademicPeriodId)
            return;

        AcademicPeriodId = newAcademicPeriodId;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"Asignación {Id}: Docente={TeacherId}, Asignatura={SubjectId}, Período={AcademicPeriodId}";
}
