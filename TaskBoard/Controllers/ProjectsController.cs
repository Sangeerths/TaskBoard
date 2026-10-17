using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.Models.Requests;
using TaskBoard.Models.Responses;
using TaskBoard.WrapperService;

namespace TaskBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectsWrapperService _projectsWrapperService;
        public ProjectsController(IProjectsWrapperService projectsWrapperService)
        {
            _projectsWrapperService = projectsWrapperService;
        }
        private bool TryGetUserId(out Guid userId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out userId);
        }

        // GET /projects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _projectsWrapperService.GetProjectsAsync(userId);

            return Ok(response);
        }

        // GET /projects/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _projectsWrapperService.GetProjectAsync(id, userId);

            return StatusCode(response.StatusCode, response);
        }

        // POST /projects
        [HttpPost]
        public async Task<IActionResult> CreateProject(
            [FromBody] CreateProjectRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _projectsWrapperService.CreateProjectAsync(
                request, userId);

            if (response.StatusCode == 201)
            {
                return CreatedAtAction(
                    nameof(GetProject),
                    new { id = response.ProjectId },
                    response);
            }

            return StatusCode(response.StatusCode, response);
        }

        // PUT /projects/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProject(
            Guid id,
            [FromBody] UpdateProjectRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _projectsWrapperService.UpdateProjectAsync(
                id, request, userId);

            return StatusCode(response.StatusCode, response);
        }

        // DELETE /projects/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var response = await _projectsWrapperService.DeleteProjectAsync(
                id, userId);

            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetProjectMembers(Guid id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _projectsWrapperService.GetProjectMembersAsync(id, userId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
        }


        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddProjectMember(
            Guid id, [FromBody] AddProjectMemberRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var response = await _projectsWrapperService.AddProjectMemberAsync(
                    id, request, userId);

                return StatusCode(response.StatusCode, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseBase
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id:guid}/members/{userId:guid}")]
        public async Task<IActionResult> UpdateProjectMember(
            Guid id, Guid userId,
            [FromBody] UpdateProjectMemberRequest request)
        {
            if (!TryGetUserId(out var currentUserId))
                return Unauthorized();

            try
            {
                var response = await _projectsWrapperService.UpdateProjectMemberAsync(
                    id, userId, request, currentUserId);

                return StatusCode(response.StatusCode, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseBase
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveProjectMember(
            Guid id, Guid userId)
        {
            if (!TryGetUserId(out var currentUserId))
                return Unauthorized();

            try
            {
                var response = await _projectsWrapperService.RemoveProjectMemberAsync(
                    id, userId, currentUserId);

                return StatusCode(response.StatusCode, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseBase
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ResponseBase
                {
                    StatusCode = 403,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseBase
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }


    }
}
