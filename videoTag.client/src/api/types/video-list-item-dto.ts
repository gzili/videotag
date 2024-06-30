export interface VideoListItemDto {
  videoId: string;
  title: string;
  duration: number;
  resolution: string;
  size: number;
  lastModifiedUnixSeconds: number;
  thumbnailUrl: string;
}