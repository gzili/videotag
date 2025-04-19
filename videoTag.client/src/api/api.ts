import ky from "ky";
import { API_HOST } from "../env.ts";
import { CategoryCreateOrUpdateDto, CategoryDto, TagDto, VideoDto, VideoListItemDto } from "./types";
import { TagCreateOrUpdateDto } from "./types/tag-create-or-update-dto.ts";

const http = ky.create({
  prefixUrl: `${API_HOST}/api`,
  retry: 0,
});

export const api = {
  createCategory(dto: CategoryCreateOrUpdateDto) {
    return http.post('categories', { json: dto }).json<CategoryDto>();
  },
  getCategories(includeTags = false) {
    const searchParams = new URLSearchParams();
    
    if (includeTags) {
      searchParams.set('includeTags', 'true');
    }

    return http.get('categories', { searchParams }).json<CategoryDto[]>();
  },
  updateCategory(categoryId: string, dto: CategoryCreateOrUpdateDto) {
    return http.put(`categories/${categoryId}`, { json: dto }).json<CategoryDto>();
  },
  async deleteCategory(categoryId: string) {
    await http.delete(`categories/${categoryId}`);
  },
  createTag(dto: TagCreateOrUpdateDto) {
    return http.post('tags', { json: dto }).json<TagDto>();
  },
  getTags() {
    return http.get('tags').json<TagDto[]>();
  },
  getTag(tagId: string) {
    return http.get(`tags/${tagId}`).json<TagDto>();
  },
  updateTag(tagId: string, dto: TagCreateOrUpdateDto) {
    return http.put(`tags/${tagId}`, { json: dto }).json<TagDto>();
  },
  async deleteTag(tagId: string) {
    await http.delete(`tags/${tagId}`);
  },
  syncVideos() {
    http.post('videos/sync');
  },
  getVideos(tagIds: string[]) {
    const searchParams = new URLSearchParams();
    tagIds.forEach(tagId => {
      searchParams.append('tagIds', tagId);
    });

    return http.get('videos', { searchParams }).json<VideoListItemDto[]>();
  },
  getVideo(videoId: string) {
    return http.get(`videos/${videoId}`).json<VideoDto>();
  },
  playVideo(videoId: string) {
    return http.post(`videos/${videoId}/play`);
  },
  showInExplorer(videoId: string) {
    return http.post(`videos/${videoId}/show-in-explorer`);
  },
  deleteVideo(videoId: string, keepFileOnDisk: boolean) {
    return http.delete(`videos/${videoId}?keepFileOnDisk=${keepFileOnDisk}`);
  },
  addTagToVideo({ videoId, tagId }: { videoId: string, tagId: string }) {
    return http.post(`videos/${videoId}/tags/${tagId}`).json<TagDto[]>();
  },
  getVideoTags(videoId: string) {
    return http.get(`videos/${videoId}/tags`).json<TagDto[]>();
  },
  removeTagFromVideo({ videoId, tagId }: { videoId: string, tagId: string }) {
    return http.delete(`videos/${videoId}/tags/${tagId}`).json<TagDto[]>();
  },
  updateVideoThumbnailSeek({ videoId, seek }: { videoId: string, seek: number }) {
    return http.put(`videos/${videoId}/thumbnail?seek=${seek}`).json<VideoDto>();
  },
  uploadCustomThumbnail(videoId: string, file: File) {
    const formData = new FormData();
    formData.set('file', file);

    return http.post(`videos/${videoId}/custom-thumbnail`, { body: formData }).json<VideoDto>();
  }
};