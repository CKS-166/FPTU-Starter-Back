using AutoMapper;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using FPTU_Starter.Application.ViewModel.ProjectDTO.SubCategoryPrj;
using static FPTU_Starter.Domain.Enum.ProjectEnum;
using FPTU_Starter.Application.ViewModel.UserDTO;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace FPTU_Starter.Application.Services
{
    public class ProjectManagementService : IProjectManagementService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ClaimsPrincipal _claimsPrincipal;
        public ProjectManagementService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
        }

        public async Task<ResultDTO<string>> CreateProject(ProjectAddRequest projectAddRequest)
        {
            try
            {
                ApplicationUser owner = _unitOfWork.UserRepository.Get(p => p.Email == projectAddRequest.ProjectOwnerEmail);
                List<SubCategory> subCates = new List<SubCategory>();
                foreach (SubCatePrjAddRequest sa in projectAddRequest.SubCategories)
                {
                    SubCategory sub = _unitOfWork.SubCategoryRepository.Get(sc => sc.Id == sa.Id);
                    subCates.Add(sub);
                }
                Project project = _mapper.Map<Project>(projectAddRequest);
                project.SubCategories = subCates;
                project.ProjectOwner = owner;
                await _unitOfWork.ProjectRepository.AddAsync(project);
                await _unitOfWork.CommitAsync();
                return ResultDTO<string>.Success("", "Add Sucessfully");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<ResultDTO<List<ProjectViewResponse>>> ViewAllProjectsAsync()
        {
            try
            {
                IEnumerable<Project> projects = await _unitOfWork.ProjectRepository.GetQueryable()
                    .Include(p => p.Packages).ThenInclude(pa => pa.RewardItems)
                    .Include(p => p.ProjectOwner)
                    //.Include(p => p.Category)
                    .Include(p => p.Images)
                    .ToListAsync();
                IEnumerable<ProjectViewResponse> responses = _mapper.Map<IEnumerable<Project>, IEnumerable<ProjectViewResponse>>(projects);

                return ResultDTO<List<ProjectViewResponse>>.Success(responses.ToList(), "");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task<ResultDTO<string>> UpdateProjectStatus(Guid id, ProjectStatus projectStatus)
        {
            try
            {
                Project project = await _unitOfWork.ProjectRepository.GetByIdAsync(id);
                project.ProjectStatus = projectStatus;
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.CommitAsync();

                return ResultDTO<string>.Success("", "Update Sucessfully");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<ResultDTO<ProjectViewResponse>> GetProjectById(Guid id)
        {
            try
            {
                var project = await _unitOfWork.ProjectRepository.GetByIdAsync(id);
                var projectDto = _mapper.Map<ProjectViewResponse>(project);
                return ResultDTO<ProjectViewResponse>.Success(projectDto);
                }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResultDTO<List<ProjectViewResponse>>> GetUserProjects()
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    return ResultDTO<List<ProjectViewResponse>>.Fail("User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    return ResultDTO<List<ProjectViewResponse>>.Fail("User not found.");
                }
                var userEmail = userEmailClaims.Value;
                var applicationUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                IEnumerable<Project> projectList = await _unitOfWork.ProjectRepository.GetAllAsync(x => x.ProjectOwner.Id == applicationUser.Id);
                IEnumerable<ProjectViewResponse> responses = _mapper.Map<IEnumerable<Project>, IEnumerable<ProjectViewResponse>>(projectList);
                return ResultDTO<List<ProjectViewResponse>>.Success(responses.ToList(), "");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
