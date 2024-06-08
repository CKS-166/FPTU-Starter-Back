using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FPTU_Starter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InteractionController : ControllerBase
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;

        public InteractionController(ILikeRepository likeRepository, ICommentRepository commentRepository)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
        }
        // GET: api/<InteractionController>
        [HttpGet]
        public IEnumerable<Like> Get()
        {
            var result = _likeRepository.GetAll();
            return result.ToList();
        }

        // GET api/<InteractionController>/5
        [HttpGet("{id}")]
        public Like Get(Guid id)
        {
            return _likeRepository.GetAsync(x=>x.Id.Equals(id));
        }

        // POST api/<InteractionController>
        [HttpPost]
        public void Post([FromBody] Like value)
        {
            _likeRepository.Create(value);
        }
       
        // DELETE api/<InteractionController>/5
        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _likeRepository.Remove(x=>x.Id == id);
        }
    }
}
