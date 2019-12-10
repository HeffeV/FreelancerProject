﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;

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
            return await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a=>a.Tag).Include(a => a.Company).Include(a=>a.Status).ToListAsync();
        }

        // GET: api/Assignment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(long id)
        {
            var assignment = await _context.Assignments.Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefaultAsync(a=> a.AssignmentID == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return assignment;
        }

        // PUT: api/Assignment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssignment(long id, Assignment assignment)
        {
            if (id != assignment.AssignmentID)
            {
                return BadRequest();
            }

            _context.Entry(assignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Assignment
        [HttpPost]
        public async Task<ActionResult<Assignment>> PostAssignment(Assignment assignment,int companyID)
        {
			Company company = _context.Companies.FirstOrDefault(c => c.CompanyID == companyID);
			Status status = _context.Status.FirstOrDefault(s => s.StatusID == 1);

			assignment.Company = company;
			assignment.Status = status;

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssignment", new { id = assignment.AssignmentID }, assignment);
        }

        // DELETE: api/Assignment/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Assignment>> DeleteAssignment(long id)
        {
            var assignment = await _context.Assignments.Include(a => a.TagAssignments).Include(a => a.Company).Include(a => a.Status).FirstOrDefaultAsync(a=> a.AssignmentID == id);
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

		[HttpGet("PossibleStatus")]
		public async Task<ActionResult<IEnumerable<Status>>> GetStatusses()
		{
			return await _context.Status.ToListAsync();
		}
		[HttpPut("PublishAssignment")]
		public async Task<ActionResult<Assignment>>  PublishAssignment(long id)
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
            var allAssignments = await _context.Assignments.ToListAsync();
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

		[HttpGet("UserAssignments")]
		public async Task<ActionResult<IEnumerable<UserAssignment>>> GetUserAssignments()
		{
			return await _context.UserAssignments.Include(u=> u.User).Include(u => u.Assignment).ToListAsync();
		}

		[HttpGet("byUserID")]
		public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignmentsByUserID(int userID)
		{
			var userAssignments = _context.UserAssignments.Include(ua=> ua.Assignment).Where(ua => ua.User.UserID == userID);
			List <Assignment> assignments = new List<Assignment>();
			foreach (var ua in userAssignments)
			{
				var found = _context.Assignments.FirstOrDefault(a => a.AssignmentID == ua.Assignment.AssignmentID);
				if (found != null)
				{
					assignments.Add(_context.Assignments.Include(a => a.TagAssignments).ThenInclude(a=>a.Tag).Include(a => a.Company).Include(a => a.Status).FirstOrDefault(a => a.AssignmentID == ua.Assignment.AssignmentID));
				} 
			}
			return assignments;
		}

        [HttpGet("filterAssignments")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetFilteredAssignments(string title)
        {
            List<Assignment> allAssignments = new List<Assignment>();

            allAssignments = await _context.Assignments.Where(e=>e.AssignmentName.Contains(title)).
                Include(a => a.TagAssignments).ThenInclude(a => a.Tag).Include(a => a.Company).Include(a => a.Status).ToListAsync();

            if (allAssignments.Count > 0)
            {
                return allAssignments;
            }

            return NotFound();
        }
    }
}
