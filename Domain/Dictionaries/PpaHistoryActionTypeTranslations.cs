using Domain.Ppa;

namespace Domain.Dictionaries
{
    public static class PpaHistoryActionTypeTranslations
    {
        public static readonly Dictionary<PpaHistoryActionType, string> Spanish = new()
        {
            { PpaHistoryActionType.Created, "Creado" },
            { PpaHistoryActionType.UpdatedTitle, "Título Actualizado" },
            { PpaHistoryActionType.ChangedStatus, "Estado Cambiado" },
            { PpaHistoryActionType.ChangedResponsibleTeacher, "Docente Responsable Cambiado" },
            { PpaHistoryActionType.UpdatedAssignments, "Asignaciones Actualizadas" },
            { PpaHistoryActionType.UpdatedStudents, "Estudiantes Actualizados" },
            { PpaHistoryActionType.UpdatedContinuationSettings, "Configuración de Continuidad Actualizada" },
            { PpaHistoryActionType.AttachmentAdded, "Anexo Agregado" },
            { PpaHistoryActionType.AttachmentRemoved, "Anexo Eliminado" },
            { PpaHistoryActionType.ContinuationCreated, "Continuación Creada" },
            { PpaHistoryActionType.UpdatedGeneralObjective, "Objetivo General Actualizado" },
            { PpaHistoryActionType.UpdatedSpecificObjectives, "Objetivos Específicos Actualizados" },
            { PpaHistoryActionType.UpdatedDescription, "Descripción Actualizada" }
        };

        public static readonly Dictionary<PpaHistoryActionType, string> English = new()
        {
            { PpaHistoryActionType.Created, "Created" },
            { PpaHistoryActionType.UpdatedTitle, "Title Updated" },
            { PpaHistoryActionType.ChangedStatus, "Status Changed" },
            { PpaHistoryActionType.ChangedResponsibleTeacher, "Responsible Teacher Changed" },
            { PpaHistoryActionType.UpdatedAssignments, "Assignments Updated" },
            { PpaHistoryActionType.UpdatedStudents, "Students Updated" },
            { PpaHistoryActionType.UpdatedContinuationSettings, "Continuation Settings Updated" },
            { PpaHistoryActionType.AttachmentAdded, "Attachment Added" },
            { PpaHistoryActionType.AttachmentRemoved, "Attachment Removed" },
            { PpaHistoryActionType.ContinuationCreated, "Continuation Created" },
            { PpaHistoryActionType.UpdatedGeneralObjective, "General Objective Updated" },
            { PpaHistoryActionType.UpdatedSpecificObjectives, "Specific Objectives Updated" },
            { PpaHistoryActionType.UpdatedDescription, "Description Updated" }
        };
    }
}
