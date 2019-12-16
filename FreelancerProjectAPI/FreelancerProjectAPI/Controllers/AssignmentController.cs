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
			return await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Location).Include(a => a.Status).Include(a => a.Status).Where(a => a.Status.CurrentStatus == "Open").ToListAsync();
		}

		// GET: api/Assignment/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Assignment>> GetAssignment(long id)
		{
			var assignment = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.UserAssignments).ThenInclude(a => a.User).Include(a => a.Company).Include(a => a.Location).Include(a => a.Status).FirstOrDefaultAsync(a => a.AssignmentID == id);

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


            List<TagAssignment> tagAssignments = new List<TagAssignment>();

            foreach (TagAssignment tagAssignment in assignment.TagAssignments)
            {
                if (_context.Tags.FirstOrDefault(t => t.TagName == tagAssignment.Tag.TagName) == null)
                {
                    tagAssignments.Add(tagAssignment);
                }
                else
                {
                    TagAssignment tmpTagAssignment = new TagAssignment();
                    tmpTagAssignment.Assignment=assignment;
                    Tag tmpTag = _context.Tags.FirstOrDefault(t => t.TagName == tagAssignment.Tag.TagName);
                    tmpTagAssignment.Tag = tmpTag;
                    tagAssignments.Add(tmpTagAssignment);
                }
            }

            assignment.TagAssignments = tagAssignments;

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
			var assignment = await _context.Assignments.Include(a => a.TagAssignments).Include(a => a.Location).Include(a=>a.UserAssignments).FirstOrDefaultAsync(a => a.AssignmentID == id);
			if (assignment == null)
			{
				return NotFound();
			}

            foreach(TagAssignment tagAssignment in assignment.TagAssignments)
            {
                _context.TagAssignments.Remove(tagAssignment);
            }

            foreach(UserAssignment userAssignment in assignment.UserAssignments)
            {
                _context.UserAssignments.Remove(userAssignment);
            }

            _context.Locations.Remove(assignment.Location);

			_context.Assignments.Remove(assignment);
			await _context.SaveChangesAsync();

			return Ok();
		}

		private bool AssignmentExists(long id)
		{
			return _context.Assignments.Any(e => e.AssignmentID == id);
		}

		[Authorize]
		// GET: api/Assignment/PossibleStatus
		[HttpGet("PossibleStatus")]
		//returns list of Status-objects
		public async Task<ActionResult<IEnumerable<Status>>> GetStatusses()
		{
			return await _context.Status.ToListAsync();
		}

		[Authorize]
		// PUT: api/Assignment/PublishAssignment?id=1
		[HttpPut("PublishAssignment")]
		//set status of assignment to Open -> users can apply now
		//returns Assignment-object - with status Open
		public async Task<ActionResult<Assignment>> PublishAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.CurrentStatus == "Open");
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

		[Authorize]
		// PUT: api/Assignment/closeAssignment?id=1
		[HttpPut("closeAssignment")]
		//set status of assignment to Closed -> users can't apply anymore
		//returns Assignment-object - with status Closed
		public async Task<ActionResult<Assignment>> CloseAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.CurrentStatus == "Closed");
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

		[Authorize]
		// PUT: api/Assignment/FinishAssignment?id=1
		[HttpPut("FinishAssignment")]
		//set status of assignment to Finished -> 1 user is selected for this assignment
		//returns Assignment-object - with status Finished
		public async Task<ActionResult<Assignment>> FinishAssignment(long id)
		{
			Assignment assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == id);

			Status status = _context.Status.FirstOrDefault(s => s.CurrentStatus == "Finished");
			assignment.Status = status;

			_context.Entry(assignment).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return assignment;
		}

		[HttpGet]
		// GET: api/Assignment/getRandoms
		[Route("getRandoms")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetRandomAssignments()
		{
			List<Assignment> allAssignments = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(e => e.Status).Where(e => e.Status.StatusID == 4).ToListAsync();
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
		//GET: api/Assignment/byUserID?userID=1
		[HttpGet("byUserID")]
		//returns all assignments that belong to a user
		//user can have multiple companies so all assignments of all their companies are shown
		//returns list of Assignment-objects
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
		//POST: api/Assignment/filterAssignments
		public async Task<ActionResult<IEnumerable<Assignment>>> GetFilteredAssignments(FilterModel filterModel)
		{

			List<Assignment> allAssignments = new List<Assignment>();
			if (filterModel.Title == "")
			{
				allAssignments = await _context.Assignments.
					Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).Include(e => e.Status).Where(a => a.Status.CurrentStatus == "Open").ToListAsync();
			}
			else
			{
				allAssignments = await _context.Assignments.Include(e => e.Status).
					Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).Where(e => e.AssignmentName.Contains(filterModel.Title) && e.Status.StatusID == 4).ToListAsync();
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
		//GET: api/Assignment/requestedAssignmentByUserID?userID=1
		[HttpGet("requestedAssignmentByUserID")]
		//returns assignments that the user has applied for
		//assignment status is still open  and user hasnt beent selected (yet) for this assignment
		//returns list of Assignment-objects
		public async Task<ActionResult<IEnumerable<Assignment>>> GetRequestedAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				//status is still open
				if (assignment.Status.CurrentStatus == "Open")
				{
					foreach (var ua in assignment.UserAssignments)
					{
						//user hasn't been accepted (yet)
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
		//GET: api/Assignment/inProgressAssignmentByUserID?userID=1
		[HttpGet("inProgressAssignmentByUserID")]
		//returns assignments that the user has applied for and thi user is selected for this assignment
		//returns list of Assignment-objects
		public async Task<ActionResult<IEnumerable<Assignment>>> GetInProgresAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				//status is closed (others can't apply anymore)
				if (assignment.Status.CurrentStatus == "Closed")
				{
					foreach (var ua in assignment.UserAssignments)
					{
						//user has been accepted for this assignment
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
		//GET: api/Assignment/finishedAssignmentByUserID?userID=1
		[HttpGet("finishedAssignmentByUserID")]
		//returns assignments that the user has finished
		//returns list of Assignment-objects
		public async Task<ActionResult<IEnumerable<Assignment>>> GetFinishedAssignmentsByUserID(int userID)
		{
			List<Assignment> assignmentsByUser = GetAssignmentsByUserID(userID);
			List<Assignment> assignments = new List<Assignment>();
			foreach (var assignment in assignmentsByUser)
			{
				//status is finished so this assignment is done
				if (assignment.Status.CurrentStatus == "Finished")
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


		//GET: api/Assignment/byCompany? userID = 1
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
		//POST: api/Assignment/ApplyForAssignment?assignmentID=0&userID=0
		[HttpPost("ApplyForAssignment")]
		//user applies for an assignment
		//userAssignment-object gets created
		//returns OK if succesfull - BadRequest
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
		//DELETE: api/Assignment/CancelAssignment?assignmentID=0&userID=0
		[HttpDelete("CancelAssignment")]
		//a user cancels his 'apply' from this assignment
		//userAssignment is deleted
		//returns Ok
		public async Task<ActionResult<UserAssignment>> CancelAssignment(int assignmentID, int userID)
		{
			UserAssignment userAssignment = _context.UserAssignments.Include(ua => ua.Assignment).FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == userID);

			_context.UserAssignments.Remove(userAssignment);
			await _context.SaveChangesAsync();

			return Ok();
		}

		//GET: api/Assignment/UserAssignment?assignmentID=0&userID=0
		[HttpGet("UserAssignment")]
		//returns UserAssignment-object if exists
		//exists if user has applied for this assignment
		//returns USerAssignment-object
		public async Task<ActionResult<UserAssignment>> GetUserAssignment(int assignmentID, int userID)
		{
			var userAssignment = _context.UserAssignments.FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == userID);

			if (userAssignment == null)
			{
				return null;
			}
			return userAssignment;
		}

		//GET: api/Assignment/CheckIfOwnAssignment?assignmentID=0&userID=0
		[HttpGet("CheckIfOwnAssignment")]
		//check if assignment is owned by a company of this user
		//returns boolean
		public Boolean CheckIfOwnAssignment(int assignmentID, int userID)
		{
			var userCompanies = _context.UserCompanies.Include(uc => uc.User).Include(uc => uc.Company).Where(uc => uc.User.UserID == userID);
			List<Company> companies = new List<Company>();
			List<Assignment> assignments = new List<Assignment>();

			//see which companies are owned by this user
			foreach (var uc in userCompanies)
			{
				companies.Add(_context.Companies.Include(c => c.Assignments).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
			}
			//for each company, get the assignments created by this company
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

		[Authorize]
		//PUT: api/Assignment/AcceptAssignmentCandidate?assignmentID=0&candidateID=0
		[HttpPut("AcceptAssignmentCandidate")]
		//accept a candidate for an assignments 
		//UserAssignment-object property Accepted is set to true
		// Status of this assignment becomes 'Closed'
		//the other candidates - UserAssignment-objects are deleted
		//returns ok
		public async Task<IActionResult> AcceptAssignmentCandidate(int assignmentID, int candidateID)
		{
			//get UserAssignment-object of this assignment and the user
			UserAssignment userAssignment = _context.UserAssignments.Include(ua => ua.Assignment).Include(ua => ua.User).FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == candidateID);
			
			//set Accepted to true - this user can do the assignment
			userAssignment.Accepted = true;
			_context.Entry(userAssignment).State = EntityState.Modified;

			//assignment-status is set to Closed
			Assignment assignment = _context.Assignments.Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == assignmentID);
			assignment.Status = _context.Status.FirstOrDefault(s => s.CurrentStatus == "Closed");
			_context.Entry(userAssignment).State = EntityState.Modified;

			// delete the userAssignment-object from the other candidates
			List<UserAssignment> userAssignments = _context.UserAssignments.Include(ua => ua.Assignment).Include(ua => ua.User).Where(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID != candidateID).ToList();
			foreach (var u in userAssignments)
			{
				_context.UserAssignments.Remove(u);
			}

			await _context.SaveChangesAsync();
			return Ok();

		}
		[Authorize]
		//PUT: api/Assignment/DeclineAssignmentCandidate?assignmentID=0&candidateID=0
		[HttpPut("DeclineAssignmentCandidate")]
		//a company declines this user for the assignment
		//UserAssignment gets deleted
		//returns ok
		public async Task<IActionResult> DeclineAssignmentCandidate(int assignmentID, int candidateID)
		{
			UserAssignment userAssignment = _context.UserAssignments.Include(ua => ua.Assignment).Include(ua => ua.User).FirstOrDefault(ua => ua.Assignment.AssignmentID == assignmentID && ua.User.UserID == candidateID);

			_context.UserAssignments.Remove(userAssignment);

			await _context.SaveChangesAsync();
			return Ok();

		}

        [Authorize]
        [HttpPost("adminFilteredAssignments")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetFilteredAssignments(AssignmentFilterModel filtermodel)
        {
            List<Assignment> allAssignments = await _context.Assignments.
                    Include(a => a.TagAssignments).ThenInclude(a => a.Tag)
                    .Include(a => a.Company)
                    .Include(a => a.Status)
                    .Include(a=>a.Location)
                    .Include(e => e.Status).ToListAsync();

            if (filtermodel.CompanyName != null && filtermodel.CompanyName != "" && filtermodel.CompanyName != " ")
            {
                allAssignments = allAssignments.Where(c => c.Company.CompanyName.ToLower().Contains(filtermodel.CompanyName.ToLower())).ToList();
            }
            if (filtermodel.Status != null && filtermodel.Status != "" && filtermodel.Status != " ")
            {
                allAssignments = allAssignments.Where(c => c.Status.CurrentStatus.ToLower().Contains(filtermodel.Status.ToLower())).ToList();
            }
            if (filtermodel.Title != null && filtermodel.Title != "")
            {
                allAssignments = allAssignments.Where(c => c.AssignmentName.ToLower().Contains(filtermodel.Title.ToLower())).ToList();
            }

            return allAssignments;
        }

		//GET: api/Assignment/CheckIfCandidateIsAccepted?assignmentID=2
		[HttpGet("CheckIfCandidateIsAccepted")]
		//check if there is a user that has been accepted for this assignment
		//returns boolean
		public Boolean CheckIfCandidateIsAccepted(int assignmentID)
		{
			Assignment assignment = _context.Assignments.Include(a => a.UserAssignments).FirstOrDefault(a => a.AssignmentID == assignmentID);

			foreach (var ua in assignment.UserAssignments)
			{
				if (ua.Accepted == true)
				{
					return true;
				}
			}

			return false;

		}
	}
}
