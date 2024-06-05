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
using Microsoft.AspNetCore.Identity;
using FPTU_Starter.Domain.Constrain;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using Org.BouncyCastle.Asn1.Ocsp;
using FPTU_Starter.Application.ViewModel.ProjectDTO.RewardItemDTO;

namespace FPTU_Starter.Application.Services
{
    public class ProjectManagementService : IProjectManagementService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ClaimsPrincipal _claimsPrincipal;
        private UserManager<ApplicationUser> _userManager;
        public ProjectManagementService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _userManager = userManager;
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
                return ResultDTO<string>.Success("Add Sucessfully", "");
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
                    .Include(p => p.SubCategories)
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
        public async Task<ResultDTO<List<ProjectViewResponse>>> GetUserProjects(string searchType, string? searchName, ProjectStatus? projectStatus, int? moneyTarget, string? categoryName)
        {
            try
            {
                IEnumerable<Project> projectList;
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    projectList = await _unitOfWork.ProjectRepository.GetQueryable()
                             .Include(p => p.Packages).ThenInclude(pa => pa.RewardItems)
                             .Include(p => p.ProjectOwner)
                             .Include(p => p.SubCategories)
                                 .ThenInclude(s => s.Category)
                             .Include(p => p.Images)
                             .ToListAsync();
                }
                else
                {
                    var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    if (userEmailClaims == null)
                    {
                        return ResultDTO<List<ProjectViewResponse>>.Fail("User not found.");
                    }
                    var userEmail = userEmailClaims.Value;
                    var applicationUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == userEmail);
                    switch (searchType)
                    {
                        case "user":
                            projectList = await _unitOfWork.ProjectRepository.GetQueryable()
                                .Include(p => p.Packages).ThenInclude(pa => pa.RewardItems)
                                .Include(p => p.ProjectOwner)
                                .Include(p => p.SubCategories)
                                    .ThenInclude(s => s.Category)
                                .Include(p => p.Images).Where(x => x.ProjectOwner.Id == applicationUser.Id)
                                .ToListAsync();
                            break;
                        default:
                            projectList = await _unitOfWork.ProjectRepository.GetQueryable()
                                .Include(p => p.Packages).ThenInclude(pa => pa.RewardItems)
                                .Include(p => p.ProjectOwner)
                                .Include(p => p.SubCategories)
                                    .ThenInclude(s => s.Category)
                                .Include(p => p.Images)
                                .ToListAsync();
                            break;
                    }
                }
                //Check searchName
                if (searchName != null)
                {
                    projectList = projectList.Where(x => x.ProjectName.ToLower().Contains(searchName.ToLower()));
                }
                //Check targetRange
                if(moneyTarget != null)
                {
                    switch(moneyTarget)
                    {
                        case 1:
                            projectList = projectList.Where(x => x.ProjectTarget >= 0 && x.ProjectTarget < 1000000);
                            break;
                        case 2:
                            projectList = projectList.Where(x => x.ProjectTarget >= 1000000 && x.ProjectTarget < 10000000);
                            break; 
                        case 3:
                            projectList = projectList.Where(x => x.ProjectTarget >= 10000000 && x.ProjectTarget < 100000000);
                            break;
                        case 4:
                            projectList = projectList.Where(x => x.ProjectTarget >= 100000000);
                            break;
                        default:
                            break;
                    }
                }
                //check categoryName
                if(categoryName != null)
                {
                    projectList = projectList.Where(x => x.SubCategories.FirstOrDefault().Category.Name.ToLower().Contains(categoryName.ToLower()));
                }
                //check projectStatus
                if(projectStatus != null)
                {
                    projectList = projectList.Where(x => x.ProjectStatus == projectStatus);
                }
                IEnumerable<ProjectViewResponse> responses = _mapper.Map<IEnumerable<Project>, IEnumerable<ProjectViewResponse>>(projectList);
                return ResultDTO<List<ProjectViewResponse>>.Success(responses.ToList(), "");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<ResultDTO<string>> UpdateProject(ProjectUpdateRequest request)
        {
            try
            {
                Project existedPrj = await _unitOfWork.ProjectRepository.GetByIdAsync(request.Id);
                if (existedPrj != null)
                {
                    List<ProjectImage> images = _mapper.Map<List<ProjectImage>>(request.Images);
                    //List<ProjectPackage> packages = _mapper.Map<List<ProjectPackage>>(request.Packages);
                    foreach (ProjectImage image in images)
                    {
                        image.ProjectId = existedPrj.Id;
                    }
                    _mapper.Map(request, existedPrj);
                    existedPrj.Images = images;
                    _unitOfWork.ProjectRepository.Update(existedPrj);
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<string>.Success("Update Sucessfully");
                }
                else
                {
                    return ResultDTO<string>.Fail("Project Not Found", 404);
                }
            }
            catch(Exception e)
            {
                return ResultDTO<string>.Fail(e.Message, 500);

            }
        }

        public async Task<ResultDTO<string>> UpdatePackages(Guid id, List<PackageViewResponse> req)
        {
            try
            {
                List<ProjectPackage> nPack = new List<ProjectPackage>();
                foreach(PackageViewResponse response in req)
                {
                    ProjectPackage pack = _unitOfWork.PackageRepository.GetQueryable().Include(p => p.RewardItems).FirstOrDefault(p => p.Id == response.Id);
                    List<RewardItem> rewardItems = new List<RewardItem>();
                    nPack.Add(pack);
                    _mapper.Map(response, pack);
                    foreach(RewardItemViewResponse item in response.RewardItems)
                    {
                        RewardItem reward = _unitOfWork.RewardItemRepository.GetById(item.Id);
                        reward.Name  =item.Name;
                        reward.Description =item.Description;
                        reward.Quantity = item.Quantity;
                        reward.ImageUrl = item.ImageUrl;
                        rewardItems.Add(reward);
                    }
                    pack.RewardItems = rewardItems;
                    _unitOfWork.PackageRepository.Update(pack);
                }
                _mapper.Map(req,nPack);
                //_unitOfWork.PackageRepository.UpdateRange(nPack);
                await _unitOfWork.CommitAsync();
                return ResultDTO<string>.Success("Update Sucessfully");
            }
            catch (Exception e)
            {
                return ResultDTO<string>.Fail(e.Message, 500);

            }

        }
    }
}
