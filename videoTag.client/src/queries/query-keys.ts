enum Queries {
  Categories = 'categories',
  Tags = 'tags',
  Videos = 'videos',
  Video = 'video',
  VideoTags = 'videoTags',
}

export const queryKeys = {
  tags: [Queries.Tags],
  categories: [Queries.Categories],
  videos: (tagIds?: string[]) => [Queries.Videos, ...(tagIds ? [{ tagIds }] : [])],
  video: (videoId: string) => [Queries.Video, videoId],
  videoTags: (videoId: string) => [Queries.VideoTags, videoId],
}