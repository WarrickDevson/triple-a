using KPW.Application.DTOs.Reports;

namespace KPW.Application.Interfaces;

public interface IPetReportPdfGenerator
{
    byte[] Generate(PetClinicalReportDto report);
}
