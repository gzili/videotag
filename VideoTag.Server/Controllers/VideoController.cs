using Microsoft.AspNetCore.Mvc;
using VideoTag.Server.BackgroundServices;
using VideoTag.Server.Contracts;
using VideoTag.Server.Entities;
using VideoTag.Server.Services;

namespace VideoTag.Server.Controllers;

[ApiController]
[Route("api/videos")]
public class VideoController(IVideoService videoService, VideoLibrarySyncTrigger syncTrigger) : ControllerBase
{
    [HttpPost("sync")]
    public IActionResult TriggerSync()
    {
        syncTrigger.OnTriggered();
        return Ok();
    }

    [HttpGet]
    public async Task<IEnumerable<VideoListItemDto>> GetVideos([FromQuery] Guid[] tagIds)
    {
        IEnumerable<Video> videos;
        if (tagIds.Length == 0)
        {
            videos = await videoService.GetVideos();
        }
        else
        {
            videos = await videoService.GetVideosContainingAllTags(tagIds);
        }
        return videos.Select(VideoListItemDto.FromVideo);
    }

    [HttpGet("{videoId:guid}")]
    public async Task<VideoDto> GetVideo(Guid videoId)
    {
        var video = await videoService.GetVideo(videoId);
        return VideoDto.FromVideo(video);
    }

    [HttpPost("{videoId:guid}/play")]
    public async Task<IActionResult> Play(Guid videoId)
    {
        try
        {
            await videoService.PlayVideo(videoId);
            return Ok();
        }
        catch (FileNotFoundException)
        {
            return Conflict();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
    
    [HttpDelete("{videoId:guid}")]
    public async Task<IActionResult> DeleteVideo(Guid videoId, bool keepFileOnDisk)
    {
        try
        {
            await videoService.DeleteVideo(videoId, keepFileOnDisk);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{videoId:guid}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(Guid videoId, int seek)
    {
        try
        {
            var thumbnailBytes = await videoService.GetVideoThumbnailAtSeek(videoId, seek);
            return File(thumbnailBytes, "image/jpeg");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPut("{videoId:guid}/thumbnail")]
    public async Task<ActionResult<VideoDto>> UpdateThumbnailSeek(Guid videoId, int seek)
    {
        try
        {
            var video = await videoService.UpdateThumbnailSeek(videoId, seek);
            return Ok(VideoDto.FromVideo(video));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{videoId:guid}/custom-thumbnail")]
    public async Task<ActionResult<VideoDto>> UploadThumbnail(Guid videoId, UploadCustomThumbnailDto dto)
    {
        try
        {
            var stream = new MemoryStream((int)dto.File.Length);
            await dto.File.CopyToAsync(stream);
            
            var video = await videoService.SaveCustomThumbnail(videoId, stream.ToArray());
            
            return Ok(VideoDto.FromVideo(video));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{videoId:guid}/tags/{tagId:guid}")]
    public async Task<IEnumerable<TagDto>> AddTag(Guid videoId, Guid tagId)
    {
        await videoService.AddTag(videoId, tagId);
        var tags = await videoService.GetTags(videoId);
        return tags.Select(TagDto.FromTag);
    }

    [HttpGet("{videoId:guid}/tags")]
    public async Task<IEnumerable<TagDto>> GetTags(Guid videoId)
    {
        var tags = await videoService.GetTags(videoId);
        return tags.Select(TagDto.FromTag);
    }
    
    [HttpDelete("{videoId:guid}/tags/{tagId:guid}")]
    public async Task<IEnumerable<TagDto>> RemoveTag(Guid videoId, Guid tagId)
    {
        await videoService.RemoveTag(videoId, tagId);
        var tags = await videoService.GetTags(videoId);
        return tags.Select(TagDto.FromTag);
    }
}