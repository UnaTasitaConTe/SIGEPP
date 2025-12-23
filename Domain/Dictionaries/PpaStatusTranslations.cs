using Domain.Ppa;

namespace Domain.Dictionaries
{
    public static class PpaStatusTranslations
    {
        public static readonly Dictionary<PpaStatus, string> Spanish = new()
        {
            { PpaStatus.Proposal, "Propuesta" },
            { PpaStatus.InProgress, "En Progreso" },
            { PpaStatus.Completed, "Completado" },
            { PpaStatus.Archived, "Archivado" },
            { PpaStatus.InContinuing, "En continuación" }
        };

        public static readonly Dictionary<PpaStatus, string> English = new()
        {
            { PpaStatus.Proposal, "Proposal" },
            { PpaStatus.InProgress, "In Progress" },
            { PpaStatus.Completed, "Completed" },
            { PpaStatus.Archived, "Archived" },
            { PpaStatus.InContinuing, "In continuing" }
        };
    }
}
