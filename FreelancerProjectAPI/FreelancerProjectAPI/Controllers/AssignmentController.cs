using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreelancerProjectAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AssignmentController : ControllerBase
	{
		private readonly DatabaseContext _context;

		public AssignmentController(DatabaseContext context)
		{
			_context = context;
		}

		// GET: api/Assignment
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignments()
		{
			return await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Location).Include(a => a.Status).ToListAsync();
		}

        [HttpGet("AllOpenAssignments")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAllOpenAssignments()
        {
            return await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Location).Include(a => a.Status).Include(a=>a.Status).Where(a=>a.Status.CurrentStatus=="Open").ToListAsync();
        }

        // GET: api/Assignment/5
        [HttpGet("{id}")]
		public async Task<ActionResult<Assignment>> GetAssignment(long id)
		{
			var assignment = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Location).Include(a => a.Status).FirstOrDefaultAsync(a => a.AssignmentID == id);

			if (assignment == null)
			{
				return NotFound();
			}

			return assignment;
		}
        [Authorize]
        // PUT: api/Assignment/5
        [HttpPut]
		public async Task<IActionResult> PutAssignment(Assignment assignment)
		{
			Assignment tmpAssignment;

			tmpAssignment = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).Include(a => a.Location).FirstOrDefaultAsync(a => a.AssignmentID == assignment.AssignmentID);

            tmpAssignment.AssignmentName = assignment.AssignmentName;
            tmpAssignment.Description = assignment.Description;
            tmpAssignment.Image = assignment.Image;
			foreach (TagAssignment ta in assignment.TagAssignments)
			{
				TagAssignment tmpTagAssignment = _context.TagAssignments.Include(t => t.Tag).SingleOrDefault(t => t.Tag.TagName == ta.Tag.TagName && t.TagAssignmentID == ta.TagAssignmentID);
				if (tmpTagAssignment == null || tmpTagAssignment.Equals(null))
				{
					//tag is nog niet toegevoegd aan assignment
					Tag tmpTag = _context.Tags.SingleOrDefault(t => t.TagName == ta.Tag.TagName);
					if (tmpTag == null || tmpTag.Equals(null))
					{
						//tag bestaat niet
						tmpAssignment.TagAssignments.Add(new TagAssignment() { Tag = new Tag() { TagName = ta.Tag.TagName }, Assignment = tmpAssignment });
					}
					else
					{
						//tag bestaat
						tmpAssignment.TagAssignments.Add(new TagAssignment() { Tag = tmpTag, Assignment = tmpAssignment });
					}
				}
			}

			_context.Entry(tmpAssignment).State = EntityState.Modified;

			await _context.SaveChangesAsync();
			return Ok();
		}
        [Authorize]
        // POST: api/Assignment
        [HttpPost]
		public async Task<ActionResult<Assignment>> PostAssignment(Assignment assignment, int companyID)
		{
			Company company = _context.Companies.Include(c => c.Location).FirstOrDefault(c => c.CompanyID == companyID);
			Status status = _context.Status.FirstOrDefault(s => s.StatusID == 1);
			Location location = company.Location;

			assignment.Company = company;
			assignment.Status = status;
			assignment.Location = location;

			_context.Assignments.Add(assignment);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetAssignment", new { id = assignment.AssignmentID }, assignment);
		}
        [Authorize]
        // DELETE: api/Assignment/5
        [HttpDelete("{id}")]
		public async Task<ActionResult<Assignment>> DeleteAssignment(long id)
		{
			var assignment = await _context.Assignments.Include(a => a.TagAssignments).Include(a => a.Company).Include(a => a.Status).Include(a => a.Location).FirstOrDefaultAsync(a => a.AssignmentID == id);
			if (assignment == null)
			{
				return NotFound();
			}

			_context.Assignments.Remove(assignment);
			await _context.SaveChangesAsync();

			return assignment;
		}

		private bool AssignmentExists(long id)
		{
			return _context.Assignments.Any(e => e.AssignmentID == id);
		}

        [Authorize]
        [HttpGet("PossibleStatus")]
		public async Task<ActionResult<IEnumerable<Status>>> GetStatusses()
		{
			return await _context.Status.ToListAsync();
		}

        [Authorize]
        [HttpPut("PublishAssignment")]
		public async Task<ActionResult<Assignment>> PublishAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.StatusID == 4);
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

        [Authorize]
        [HttpPut("closeAssignment")]
		public async Task<ActionResult<Assignment>> CloseAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.StatusID == 3);
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

        [Authorize]
        [HttpPut("FinishAssignment")]
		public async Task<ActionResult<Assignment>> FinishAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.StatusID == 2);
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

		[HttpGet]
		[Route("getRandoms")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetRandomAssignments()
		{
			List<Assignment> allAssignments = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(e=>e.Status).Where(e=>e.Status.StatusID==4).ToListAsync();
			List<Assignment> assignments = new List<Assignment>();
			Random r = new Random();
			if (allAssignments.Count > 0)
			{
				for (int i = 0; i < 3; i++)
				{
					assignments.Add(allAssignments[r.Next(0, allAssignments.Count)]);
				}
			}
			else
			{
				NotFound();
			}

			return assignments;
		}

        [Authorize]
        [HttpGet("byUserID")]
		public List<Assignment> GetAssignmentsByUserID(int userID)
		{
			var userAssignments = _context.UserAssignments.Include(ua => ua.Assignment).Include(ua => ua.User).Where(ua => ua.User.UserID == userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var ua in userAssignments)
			{
				var found = _context.Assignments.FirstOrDefault(a => a.AssignmentID == ua.Assignment.AssignmentID);
				if (found != null)
				{
					assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == ua.Assignment.AssignmentID));
				}
			}
			return assignments;
		}

		[HttpPost("filterAssignments")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetFilteredAssignments(FilterModel filterModel)
		{

			List<Assignment> allAssignments = new List<Assignment>();
			if (filterModel.Title == "")
			{
				allAssignments = await _context.Assignments.
					Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).Include(e => e.Status).Where(a=>a.Status.CurrentStatus == "Open").ToListAsync();
			}
			else
			{
				allAssignments = await _context.Assignments.Include(e => e.Status).
					Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).Where(e => e.AssignmentName.Contains(filterModel.Title)&&e.Status.StatusID==4).ToListAsync();
			}

			if (filterModel.Tags.Count > 0)
			{
				List<Assignment> tmpAssignments = new List<Assignment>();
				foreach (Tag tag in filterModel.Tags)
				{
					//allAssignments = allAssignments.Where(t => t.TagAssignments.Where(t => t.Tag.TagID == tag.TagID));
					foreach (Assignment assignment in allAssignments)
					{
						foreach (TagAssignment tagAssignment in assignment.TagAssignments)
						{
							if (tagAssignment.Tag.TagID == tag.TagID)
							{
								tmpAssignments.Add(assignment);
							}
						}
					}
					allAssignments = tmpAssignments;
					tmpAssignments = new List<Assignment>();
				}
			}

			return allAssignments;
		}

        [Authorize]
        [HttpGet("requestedAssignmentByUserID")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetRequestedAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				if (assignment.Status.StatusID == 4)
				{
					foreach (var ua in assignment.UserAssignments)
					{
						if (ua.User.UserID == userID && ua.Accepted == false)
						{
							var found = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID);
							if (found != null)
							{
								assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID));
							}
						}
					}
				}
			}
			return assignments;
		}

        [Authorize]
        [HttpGet("inProgressAssignmentByUserID")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetInProgresAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				if (assignment.Status.StatusID == 3)
				{
					foreach (var ua in assignment.UserAssignments)
					{
						if (ua.User.UserID == userID && ua.Accepted == true)
						{
							var found = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID);
							if (found != null)
							{
								assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID));
							}
						}
					}
				}
			}
			return assignments;
		}

        [Authorize]
        [HttpGet("finishedAssignmentByUserID")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetFinishedAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				if (assignment.Status.StatusID == 2)
				{
					foreach (var ua in assignment.UserAssignments)
					{
						if (ua.User.UserID == userID && ua.Accepted == true)
						{
							var found = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID);
							if (found != null)
							{
								assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID));
							}
						}
					}
				}
			}
			return assignments;
		}


		[HttpGet("byCompany")]
		public List<Assignment> GetAssignmentsByCompany(int userID)
		{
			var userCompanies = _context.UserCompanies.Include(uc => uc.User).Include(uc => uc.Company).Where(uc => uc.User.UserID == userID);
			List<Company> companies = new List<Company>();
			List<Assignment> assignments = new List<Assignment>();

			foreach (var uc in userCompanies)
			{
				companies.Add(_context.Companies.Include(c => c.Assignments).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
			}
			foreach (var c in companies)
			{
				foreach (var assignment in c.Assignments)
				{
					assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == assignment.AssignmentID));
				}
			}
			return assignments;
		}

        [Authorize]
        [HttpPost("ApplyForAssignment")]
		public async Task<ActionResult<UserAssignment>> ApplyForAssignment(int assignmentID, int userID)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignmentID);
			User user = _context.Users.FirstOrDefault(u => u.UserID == userID);

			UserAssignment userAssignment = new UserAssignment();
			userAssignment.User = user;
			userAssignment.Assignment = assignment;
			userAssignment.Accepted = false;

			if (_context.UserAssignments.FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == userID) == null)
			{
				_context.UserAssignments.Add(userAssignment);
				await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [Authorize]
        [HttpDelete("CancelAssignment")]
		public async Task<ActionResult<UserAssignment>> CancelAssignment(int assignmentID, int userID)
		{
			UserAssignment userAssignment = _context.UserAssignments.Include(ua => ua.Assignment).Include(ua => ua.User).FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == userID);

			_context.UserAssignments.Remove(userAssignment);
			await _context.SaveChangesAsync();

			return userAssignment;
		}

		[HttpGet("UserAssignment")]
		public async Task<ActionResult<UserAssignment>> GetUserAssignment(int assignmentID, int userID)
		{
			var userAssignment = _context.UserAssignments.FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == userID);

			if (userAssignment == null)
			{
				return null;
			}
			return userAssignment;
		}

		[HttpGet("CheckIfOwnAssignment")]
		public Boolean CheckIfOwnAssignment(int assignmentID, int userID)
		{
			var userCompanies = _context.UserCompanies.Include(uc => uc.User).Include(uc => uc.Company).Where(uc => uc.User.UserID == userID);
			List<Company> companies = new List<Company>();
			List<Assignment> assignments = new List<Assignment>();

			foreach (var uc in userCompanies)
			{
				companies.Add(_context.Companies.Include(c => c.Assignments).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
			}
			foreach (var c in companies)
			{
				foreach (var assignment in c.Assignments)
				{
					if (assignment.AssignmentID == assignmentID)
					{
						return true;
					}
				}
			}
			return false;

		}
	}
}
