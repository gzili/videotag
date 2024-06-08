using Microsoft.AspNetCore.Mvc;
using VideoTag.Server.Contracts;
using VideoTag.Server.Services;

namespace VideoTag.Server.Controllers;

[ApiController]
[Route("api/tags")]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(TagCreateOrUpdateDto dto)
    {
        var tag = await tagService.CreateTag(dto);
        return Ok(TagDto.FromTag(tag));
    }

    [HttpGet]
    public async Task<IEnumerable<TagDto>> GetTags()
    {
        var tags = await tagService.GetTags();
        return tags.Select(TagDto.FromTag);
    }

    [HttpGet("{tagId:guid}")]
    public async Task<TagDto> GetTag(Guid tagId)
    {
        var tag = await tagService.GetTag(tagId);
        return TagDto.FromTag(tag);
    }

    [HttpPut("{tagId:guid}")]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid tagId, TagCreateOrUpdateDto dto)
    {
        try
        {
            var tag = await tagService.UpdateTag(tagId, dto);
            return Ok(TagDto.FromTag(tag));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> DeleteTag(Guid tagId)
    {
        try
        {
            await tagService.DeleteTag(tagId);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return Ok();
    }
}