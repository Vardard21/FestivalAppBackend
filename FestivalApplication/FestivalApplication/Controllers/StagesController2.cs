using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FestivalApplication.Data;
using FestivalApplication.Model;

namespace FestivalApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StagesController2 : ControllerBase
    {
        private readonly DBContext _context;

        public StagesController2(DBContext context)
        {
            _context = context;
        }

        // GET: api/StagesController2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stage>>> GetStage()
        {
            return await _context.Stage.ToListAsync();
        }

        // GET: api/StagesController2/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stage>> GetStage(int id)
        {
            var stage = await _context.Stage.FindAsync(id);

            if (stage == null)
            {
                return NotFound();
            }

            return stage;
        }

        // PUT: api/StagesController2/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStage(int id, Stage stage)
        {
            if (id != stage.StageID)
            {
                return BadRequest();
            }

            _context.Entry(stage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StageExists(id))
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

        // POST: api/StagesController2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Stage>> PostStage(Stage stage)
        {
            _context.Stage.Add(stage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStage", new { id = stage.StageID }, stage);
        }

        // DELETE: api/StagesController2/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStage(int id)
        {
            var stage = await _context.Stage.FindAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            _context.Stage.Remove(stage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StageExists(int id)
        {
            return _context.Stage.Any(e => e.StageID == id);
        }
    }
}
