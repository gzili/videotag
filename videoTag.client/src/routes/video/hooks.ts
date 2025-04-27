import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "api";

export function useVideoId() {
  const { videoId } = useParams<{ videoId: string }>()
  
  if (videoId === undefined) {
    throw new Error("undefined videoId");
  }
  
  return videoId;
}

export function useVideoQueryKey() {
  const videoId = useVideoId();
  return ['videos', videoId];
}

export function useVideo() {
  const queryKey = useVideoQueryKey();
  const { data } = useQuery({
    queryKey,
    queryFn: () => api.getVideo(queryKey[1]),
  });
  return ({ video: data });
}

export function useTags() {
  const { data } = useQuery({
    queryKey: ['tags'],
    queryFn: async () => {
      const tags = await api.getTags();
      return tags.sort((a, b) => a.category.label.localeCompare(b.category.label, "en"))
    },
    staleTime: Infinity,
    gcTime: Infinity,
  });
  return ({ tags: data });
}

export function useVideoTags() {
  const videoId = useVideoId();
  const queryKey = ['videos', videoId, 'tags'];
  const { data } = useQuery({
    queryKey,
    queryFn: () => api.getVideoTags(videoId),
  });
  return ({
    tags: data,
    queryKey,
  });
}