using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services.IService
{
    public interface IProjectManagementService
    {
        Task<ResultDTO<string>> CreateProject(ProjectAddRequest projectAddRequest);
        Task<ResultDTO<List<ProjectViewResponse>>> ViewAllProjectsAsync();
    }
}
