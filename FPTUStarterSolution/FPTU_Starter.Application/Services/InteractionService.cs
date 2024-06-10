using AutoMapper;
using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.InteractionDTO;
using FPTU_Starter.Domain.Entity;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public InteractionService(ILikeRepository likeRepository,
            ICommentRepository commentRepository,
            IUserManagementService userManagementService,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _userManagementService = userManagementService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultDTO<Comment>> CommentProject(CommentRequest request)
        {
            try
            {
                var user = _userManagementService.GetUserInfo().Result;
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                if (user is null)
                {
                    return ResultDTO<Comment>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<Comment>.Fail("Project can not found");
                }

                var exitComment =  _commentRepository.GetAsync(x => x.ProjectId.Equals(project.Id) && x.UserID.Equals(Guid.Parse(exitUser.Id)));
                if (exitComment is null)
                {
                    // add new comment
                    Comment newComment = new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = request.Content,
                        CreateDate = DateTime.Now,
                        ProjectId = project.Id,
                        UserID = Guid.Parse(exitUser.Id),
                    };
                    _commentRepository.Create(newComment);
                    return ResultDTO<Comment>.Success(newComment, "Successfully Add Comment");
                }
                else
                {
                    //update comment
                    exitComment.Content = request.Content;
                    _commentRepository.Update(x => x.Id == exitComment.Id, exitComment);
                    return ResultDTO<Comment>.Success(exitComment, "Successfully Update Comment");
                }
                

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<Like>> GetAll()
        {
            try
            {
                var list = _likeRepository.GetAll();
                return list.ToList();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
                
            }
        }

        public async Task<List<Comment>> GetAllComment()
        {
            try
            {
                var list = _commentRepository.GetAll();
                return list.ToList();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
                
            }
        }

        public async Task<ResultDTO<LikeResponse>> LikeProject(LikeRequest likeRequest)
        {
            try
            {
                var user = _userManagementService.GetUserInfo().Result;
                ApplicationUser exitUser = _mapper.Map<ApplicationUser>(user._data);
                var project = await _unitOfWork.ProjectRepository.GetAsync(x => x.Id.Equals(likeRequest.ProjectId));
                if (user is null)
                {
                    return ResultDTO<LikeResponse>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<LikeResponse>.Fail("Project can not found");
                }
                //check if the user and project already liked 
                var getLikedProjects = _likeRepository.GetAsync(x => x.ProjectId.Equals(project.Id) && x.UserId.Equals(Guid.Parse(exitUser.Id)));
                if (getLikedProjects == null)
                {
                    //liked a project
                    Like newLikeProject = new Like
                    {
                        ProjectId = likeRequest.ProjectId,
                        UserId = Guid.Parse(exitUser.Id),
                        IsLike = true,
                        CreateDate = DateTime.Now,
                        Id = Guid.NewGuid(),
                    };
                    _likeRepository.Create(newLikeProject);
                    return ResultDTO<LikeResponse>.Success(new LikeResponse { ProjectId = newLikeProject.ProjectId, UserID = newLikeProject.UserId }, "Succesfull like the project");
                }
                else
                {
                    if (getLikedProjects.IsLike) //isLike == true ? "dislike" : "liked"
                    {
                        _likeRepository.Remove(x => x.Id.Equals(getLikedProjects.Id));
                        return ResultDTO<LikeResponse>.Success(new LikeResponse { ProjectId = likeRequest.ProjectId, UserID = Guid.Parse(exitUser.Id) }, "Succesfull dislike the project");
                    }
                }

                return ResultDTO<LikeResponse>.Fail("some thing wrong : error ");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
