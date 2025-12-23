import { useQuery } from '@tanstack/react-query';
import { api } from 'api';
import { queryKeys } from './query-keys';

export const useVideo = (videoId: string) => {
  return useQuery({
    queryKey: queryKeys.video(videoId),
    queryFn: () => api.getVideo(videoId),
  });
}