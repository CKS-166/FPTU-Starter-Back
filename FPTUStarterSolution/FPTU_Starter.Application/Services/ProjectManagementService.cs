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

using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectDonate;
using System.Linq.Expressions;
using FPTU_Starter.Domain.Enum;
using Org.BouncyCastle.Asn1.Ocsp;
using System.CodeDom;


namespace FPTU_Starter.Application.Services
{
    public class ProjectManagementService : IProjectManagementService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly IUserManagementService _userManagement;
        private readonly IPackageManagementService _packageManagement;
        private ClaimsPrincipal _claimsPrincipal;
        private UserManager<ApplicationUser> _userManager;
       
        public ProjectManagementService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            IWalletService walletService,
            IUserManagementService userManagement,
            IPackageManagementService packageManagement,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _walletService = walletService;
            _userManagement = userManagement;
            _packageManagement = packageManagement;
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
                IQueryable<Project> projectQuery = _unitOfWork.ProjectRepository.GetQueryable()
                    .AsNoTracking()
                    .Include(p => p.Packages).ThenInclude(pa => pa.RewardItems)
                    .Include(p => p.ProjectOwner)
                    .Include(p => p.SubCategories).ThenInclude(s => s.Category)
                    .Include(p => p.Images);

                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    
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

                    if (searchType == "user")
                    {
                        projectQuery = projectQuery.Where(x => x.ProjectOwner.Id == applicationUser.Id);
                    }
                }

                // Apply filters before executing the query
                if (!string.IsNullOrEmpty(searchName))
                {
                    projectQuery = projectQuery.Where(x => x.ProjectName.ToLower().Contains(searchName.ToLower()));
                }

                if (moneyTarget.HasValue)
                {
                    switch (moneyTarget.Value)
                    {
                        case 1:
                            projectQuery = projectQuery.Where(x => x.ProjectTarget >= 0 && x.ProjectTarget < 1000000);
                            break;
                        case 2:
                            projectQuery = projectQuery.Where(x => x.ProjectTarget >= 1000000 && x.ProjectTarget < 10000000);
                            break;
                        case 3:
                            projectQuery = projectQuery.Where(x => x.ProjectTarget >= 10000000 && x.ProjectTarget < 100000000);
                            break;
                        case 4:
                            projectQuery = projectQuery.Where(x => x.ProjectTarget >= 100000000);
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(categoryName))
                {
                    projectQuery = projectQuery.Where(x => x.SubCategories.Any(s => s.Category.Name.ToLower().Contains(categoryName.ToLower())));
                }

                if (projectStatus.HasValue)
                {
                    projectQuery = projectQuery.Where(x => x.ProjectStatus == projectStatus.Value);
                }

                var projectList = await projectQuery.ToListAsync();

                var responses = _mapper.Map<List<ProjectViewResponse>>(projectList);
                return ResultDTO<List<ProjectViewResponse>>.Success(responses, "");
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
                    existedPrj.Images.Clear();
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
        public async Task<ResultDTO<ProjectDonateResponse>> DonateProject(ProjectDonateRequest request)
        {
            try
            {
                var user = _userManagement.GetUserInfo().Result;
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                //Check Project Status
                if (!project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Processing) &&
                    !project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Successful))
                {
                    return ResultDTO<ProjectDonateResponse>.Fail("Project cannot be donated to");
                }
                var userWallet = await _unitOfWork.WalletRepository.GetAsync(x => x.BackerId!.Equals(exitUser.Id));

                var IsEnoughMoney = await _walletService.CheckAccoutBallance(request.AmountDonate);
                if (IsEnoughMoney._isSuccess)
                {
                    // check enough money then allow to donate (minus the amount donation)
                    userWallet.Balance -= request.AmountDonate;
                    project.ProjectBalance += request.AmountDonate;

                    //create a transaction
                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = userWallet.Id,
                        Wallet = exitUser.Wallet,
                        CreateDate = DateTime.Now,
                        Description = $"{exitUser.Name} has just donated project {project.ProjectName}",
                        TotalAmount = request.AmountDonate,
                        TransactionType = TransactionTypes.FreeDonation,
                    };
                    await _unitOfWork.TransactionRepository.AddAsync(transaction);

                    //savechange
                    await _unitOfWork.CommitAsync();

                    //custom response
                    var response = new ProjectDonateResponse
                    {
                        ProjectName = project.ProjectName,
                        DonateAmount = request.AmountDonate,
                        status = true

                    };
                    return ResultDTO<ProjectDonateResponse>.Success(response, "Successfully donate to this project");
                }
                return ResultDTO<ProjectDonateResponse>.Fail($"Something went wrong, {IsEnoughMoney._message}");

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public async Task<ResultDTO<ProjectDonateResponse>> PackageDonateProject(PackageDonateRequest request)
        {
            try
            {
                var user = _userManagement.GetUserInfo().Result;
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                var userWallet = await _unitOfWork.WalletRepository.GetAsync(x => x.BackerId!.Equals(exitUser.Id));
                var package = await _packageManagement.FindPackagesByProjectId(request.ProjectId);

                //Check Project Status
                if (!project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Processing) &&
                    !project.ProjectStatus.Equals(ProjectEnum.ProjectStatus.Successful))
                {
                    return ResultDTO<ProjectDonateResponse>.Fail("Project cannot be donated to");
                }

                //check package exits in the project
                var IsFoundPackage = package._data.FirstOrDefault(x => x.Id.Equals(request.PackageId));
                if (IsFoundPackage is null)
                {
                    return ResultDTO<ProjectDonateResponse>.Fail("Project Packages cannot be found");
                }

                //check amount equal to the package
                if (IsFoundPackage.RequiredAmount != request.AmountDonate)
                {
                    return ResultDTO<ProjectDonateResponse>.Fail("You must donate the exact amount");
                }



                var IsEnoughMoney = await _walletService.CheckAccoutBallance(request.AmountDonate);
                if (IsEnoughMoney._isSuccess)
                {
                    // check enough money then allow to donate (minus the amount donation)
                    userWallet.Balance -= request.AmountDonate;
                    project.ProjectBalance += request.AmountDonate;
                    IsFoundPackage.LimitQuantity -= 1;
                    //create a transaction
                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = userWallet.Id,
                        Wallet = exitUser.Wallet,
                        CreateDate = DateTime.Now,
                        Description = $"{exitUser.Name} has just donated project {project.ProjectName} with package",
                        TotalAmount = request.AmountDonate,
                        TransactionType = TransactionTypes.PackageDonation,
                        PackageId = IsFoundPackage.Id
                    };
                    await _unitOfWork.TransactionRepository.AddAsync(transaction);

                    //savechange
                    await _unitOfWork.CommitAsync();

                    //custom response
                    var response = new ProjectDonateResponse
                    {
                        ProjectName = project.ProjectName,
                        DonateAmount = request.AmountDonate,
                        status = true

                    };
                    return ResultDTO<ProjectDonateResponse>.Success(response, "Successfully donate to this project");
                }
                return ResultDTO<ProjectDonateResponse>.Fail($"Something went wrong");

            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public async Task<ResultDTO<string>> FailedProject()
        {
            try
            {
                List<Project> projects = _unitOfWork.ProjectRepository.GetAll().ToList();
                foreach (Project project in projects)
                {
                    DateTime today = DateTime.Today;
                    if (project.StartDate >= today)
                    {
                        if (project.ProjectStatus == ProjectStatus.Pending)
                        {
                            project.ProjectStatus = ProjectStatus.Failed;
                        }                    }
                    await _unitOfWork.CommitAsync();
                }
                return ResultDTO<string>.Success("Project has been expired");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);

            }
        }
    }
}
