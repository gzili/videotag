import { useQuery } from "@tanstack/react-query";
import { api } from "../../api";
import { useVideoId } from "./hooks.ts";

export function useVideoQueryKey() {
  const videoId = useVideoId();
  return ['videos', videoId];
}

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

function fetchVideo(videoId: string) {
  return api.get(`videos/${videoId}`).json<VideoDto>();
}

export function useVideo() {
  const queryKey = useVideoQueryKey();
  const { data } = useQuery({
    queryKey,
    queryFn: () => fetchVideo(queryKey[1]),
  });
  return ({ video: data });
}

export interface TagDto {
  tagId: string;
  label: string;
}

function fetchTags() {
  return api.get('tags').json<TagDto[]>();
}

export function useTags() {
  const { data } = useQuery({
    queryKey: ['tags'],
    queryFn: fetchTags,
  });
  return ({ tags: data });
}

function fetchVideoTags(videoId: string) {
  return api.get(`videos/${videoId}/tags`).json<TagDto[]>();
}

export function useVideoTags() {
  const videoId = useVideoId();
  const queryKey = ['videos', videoId, 'tags'];
  const { data } = useQuery({
    queryKey,
    queryFn: () => fetchVideoTags(videoId),
  });
  return ({
    tags: data,
    queryKey,
  });
}