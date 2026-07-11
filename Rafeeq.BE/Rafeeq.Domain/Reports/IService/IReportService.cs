using Rafeeq.Domain.Common;
using Rafeeq.Domain.Reports.DTOs;

namespace Rafeeq.Domain.Reports.IService;

public interface IReportService
{
    Task<ResultViewModel<bool>> Create(ReportCreateDto dto);            // reporter = current user
    Task<ResultViewModel<List<ReportListItemDto>>> GetAll(ReportStatus? status);
    Task<ResultViewModel<bool>> Handle(ReportHandleDto dto);           // admin
}
