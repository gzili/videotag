export interface VideoListItemDto {
  videoId: string;
  title: string;
  width: number;
  height: number;
  durationInSeconds: number;
  size: number;
  lastModifiedUnixSeconds: number;
  thumbnailUrl: string;
}