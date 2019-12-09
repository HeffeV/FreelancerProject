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
            return await _context.Assignments.Include(a => a.Tags).Include(a => a.Company).Include(a=>a.Status).ToListAsync();
        }

        // GET: api/Assignment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(long id)
        {
            var assignment = await _context.Assignments.FindAsync(id);

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
            var assignment = await _context.Assignments.Include(a => a.Tags).Include(a => a.Company).Include(a => a.Status).FirstOrDefaultAsync(a=> a.AssignmentID == id);
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
	}
}
