import { useQuery } from '@tanstack/react-query';
import { queryKeys } from './query-keys';
import { api } from 'api';

export const useVideoTags = (videoId: string) => {
  return useQuery({
    queryKey: queryKeys.videoTags(videoId),
    queryFn: () => api.getVideoTags(videoId),
  });
}