export interface VideoDto {
  videoId: string;
  title: string;
  fullPath: string
  duration: number;
  resolution: string;
  size: number;
  lastModifiedTimeUtc: string;
  thumbnailUrl: string;
  thumbnailSeek: number;
}