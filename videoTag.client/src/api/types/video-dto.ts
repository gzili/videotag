export interface VideoDto {
  videoId: string;
  title: string;
  fullPath: string
  width: number;
  height: number;
  framerate: number;
  durationInSeconds: number;
  size: number;
  lastModifiedTimeUtc: string;
  thumbnailUrl: string;
  thumbnailSeek: number;
}