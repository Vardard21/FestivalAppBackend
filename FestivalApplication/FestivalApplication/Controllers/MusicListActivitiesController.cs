using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;

namespace FestivalApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicListActivitiesController : ControllerBase
    {
        private readonly DBContext _context;

        public MusicListActivitiesController(DBContext context)
        {
            _context = context;
        }

        // GET: api/MusicListActivities
        [HttpGet]
        public Response<List<Track>> GetMusicListActivity()
        {
            Response<List<Track>> response = new Response<List<Track>>();
            return response;
        }

        // GET: api/MusicListActivities/5
        [HttpGet("{id}")]
        public Response<List<PlaylistRequestDto>> GetMusicListActivity(int id)
        {
            Response<List<PlaylistRequestDto>> response = new Response<List<PlaylistRequestDto>>();
            

            if (!(_context.MusicList.Where(x=>x.ID==id).Count()==1))
            {
                response.InvalidData();
                return response;
            }

            //connect the list from 
            var playlist = _context.TrackActivity.Where(x => x.MusicListID == id).ToList();

            //create a list of tracks
            List<PlaylistRequestDto> RequestedTracks = new List<PlaylistRequestDto>();

            if (!RequestedTracks.Any())
            {

                foreach (TrackActivity trackactivity in playlist)
                {

                    Track track = _context.Track.Find(trackactivity.TrackID);
                    PlaylistRequestDto dto = new PlaylistRequestDto();
                    dto.Id = trackactivity.TrackID;
                    dto.TrackName = track.TrackName;
                    dto.TrackSource = track.TrackSource;
                    dto.Length = track.Length;
                    RequestedTracks.Add(dto);
                }
                response.Success = true;
                response.Data = RequestedTracks;
                return response;
            }
            else
            {
                response.ServerError();
                return response;
            }
        }

        // PUT: api/MusicListActivities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMusicListActivity(int id, MusicListActivity musicListActivity)
        {
            if (id != musicListActivity.ID)
            {
                return BadRequest();
            }

            _context.Entry(musicListActivity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MusicListActivityExists(id))
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

        // POST: api/MusicListActivities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MusicListActivity>> PostMusicListActivity(MusicListActivity musicListActivity)
        {
            _context.MusicListActivity.Add(musicListActivity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMusicListActivity", new { id = musicListActivity.ID }, musicListActivity);
        }

        // DELETE: api/MusicListActivities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMusicListActivity(int id)
        {
            var musicListActivity = await _context.MusicListActivity.FindAsync(id);
            if (musicListActivity == null)
            {
                return NotFound();
            }

            _context.MusicListActivity.Remove(musicListActivity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MusicListActivityExists(int id)
        {
            return _context.MusicListActivity.Any(e => e.ID == id);
        }
    }
}
